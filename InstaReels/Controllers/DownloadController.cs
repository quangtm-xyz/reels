using System.Text.Json;
using InstaReels.Models;
using InstaReels.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;

namespace InstaReels.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private static readonly MemoryCache _rateCache = new(new MemoryCacheOptions());
    private readonly HttpClient _httpClient;
    private readonly RapidApiSettings _rapidApiSettings;
    private readonly ILogger<DownloadController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DownloadController(
        IHttpClientFactory httpClientFactory, 
        IOptions<RapidApiSettings> rapidApiSettings,
        ILogger<DownloadController> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _httpClient = httpClientFactory.CreateClient();
        _rapidApiSettings = rapidApiSettings.Value;
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Cleans an Instagram URL by removing query parameters and ensuring a clean format.
    /// This ensures only the base URL with the reel ID is sent to the API.
    /// </summary>
    /// <param name="url">The raw Instagram URL that may contain query parameters</param>
    /// <returns>A cleaned URL without query parameters</returns>
    private static string CleanInstagramUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        // Use UriBuilder to properly parse and clean the URL
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var builder = new UriBuilder(uri)
            {
                Query = string.Empty // Remove all query parameters
            };
            return builder.Uri.ToString();
        }

        // Fallback: Use string manipulation if Uri parsing fails
        var queryIndex = url.IndexOf('?');
        if (queryIndex >= 0)
        {
            return url.Substring(0, queryIndex);
        }

        return url;
    }
    private static bool IsUrlSafe(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        var allowed = new[] { "instagram.com", "www.instagram.com", "tiktok.com", "www.tiktok.com", 
            "twitter.com", "x.com", "youtube.com", "youtu.be", "facebook.com", "fb.com", "fb.watch" };
        if (!allowed.Any(d => uri.Host.EndsWith(d) || uri.Host == d)) return false;
        if (uri.IsLoopback || uri.Host.Contains("localhost")) return false;
        return true;
    }
    [HttpPost("reels")]
    public async Task<IActionResult> PostDownload([FromBody] DownloadRequest request)
    {
        // Trong PostDownload, ngay đầu method:
var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
if (_rateCache.TryGetValue(ip, out int count) && count >= 15)
    return StatusCode(429, new { success = false, error = "Quá nhiều yêu cầu, vui lòng thử lại sau 1 phút" });

_rateCache.Set(ip, count + 1, TimeSpan.FromMinutes(1));
        try
        {
            // Validate the incoming Instagram URL
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { success = false, error = _localizer["InvalidLinkError"].Value });
            }

            // Clean the URL by removing query parameters before validation
            var cleanedUrl = CleanInstagramUrl(request.Url);
            // Trong PostDownload, ngay sau cleanedUrl:
            if (!IsUrlSafe(cleanedUrl))
                return BadRequest(new { success = false, error = _localizer["InvalidLinkError"].Value });
            if (!Uri.TryCreate(cleanedUrl, UriKind.Absolute, out var uri) || 
                !(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return BadRequest(new { success = false, error = _localizer["InvalidLinkError"].Value });
            }

            // Validate RapidAPI configuration
            if (string.IsNullOrWhiteSpace(_rapidApiSettings.Key))
            {
                _logger.LogError("RapidAPI:Key is not configured");
                return StatusCode(500, new { success = false, error = _localizer["GeneralDownloadError"].Value });
            }

            if (string.IsNullOrWhiteSpace(_rapidApiSettings.Host))
            {
                _logger.LogError("RapidAPI:Host is not configured");
                return StatusCode(500, new { success = false, error = _localizer["GeneralDownloadError"].Value });
            }

        // Construct the RapidAPI request URL using the correct endpoint path
        var rapidApiUrl = $"https://{_rapidApiSettings.Host}/download?url={Uri.EscapeDataString(cleanedUrl)}";

            // Create HTTP request message
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, rapidApiUrl);
            httpRequest.Headers.Add("X-RapidAPI-Key", _rapidApiSettings.Key);
            httpRequest.Headers.Add("X-RapidAPI-Host", _rapidApiSettings.Host);

            // Send request to RapidAPI
            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("RapidAPI call failed with status code: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, new { success = false, error = _localizer["InvalidLinkError"].Value });
            }

        // Parse JSON response
        var responseContent = await response.Content.ReadAsStringAsync();
        
            var jsonDocument = JsonDocument.Parse(responseContent);
            var root = jsonDocument.RootElement;

            // Extract the video URL from data.medias[0].url
            string? downloadLink = null;
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Object &&
                dataElement.TryGetProperty("medias", out var mediasElement) &&
                mediasElement.ValueKind == JsonValueKind.Array &&
                mediasElement.GetArrayLength() > 0)
            {
                var firstMedia = mediasElement[0];
                if (firstMedia.ValueKind == JsonValueKind.Object &&
                    firstMedia.TryGetProperty("url", out var urlElement) &&
                    urlElement.ValueKind == JsonValueKind.String)
                {
                    downloadLink = urlElement.GetString();
                }
            }

            if (string.IsNullOrWhiteSpace(downloadLink))
            {
                _logger.LogError("RapidAPI response does not contain a valid video URL. Response: {Response}", responseContent);
                return StatusCode(500, new { success = false, error = _localizer["InvalidLinkError"].Value });
            }

            // Return success response with the direct video URL
            return Ok(new
            {
                success = true,
                message = _localizer["DownloadStartingMessage"].Value,
                downloadLink = downloadLink
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing download request");
            return StatusCode(500, new { success = false, error = _localizer["GeneralDownloadError"].Value });
        }
    }
    
    
}

