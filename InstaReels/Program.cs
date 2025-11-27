using System.Globalization;
using System.Linq;
using InstaReels.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var localizationSection = builder.Configuration.GetSection("Localization");
var localizationSettings = localizationSection.Get<LocalizationSettings>() ?? new LocalizationSettings();

// Add services to the container.
//Added
builder.Services.AddRazorPages()
    .AddViewLocalization();
builder.Services.AddControllers();
builder.Services.Configure<LocalizationSettings>(localizationSection);

// Register HttpClient for external API calls
builder.Services.AddHttpClient();

// === THAY ĐOẠN NÀY VÀO Program.cs (thay hoàn toàn đoạn cũ) ===
var rapidApiKey = builder.Configuration["RapidAPI:Key"]
                  ?? Environment.GetEnvironmentVariable("RAPIDAPI_KEY")
                  ?? throw new InvalidOperationException("RAPIDAPI_KEY không được cấu hình!");

builder.Services.Configure<RapidApiSettings>(options =>
{
    options.Key = rapidApiKey;
    // Bind phần Hosts từ appsettings.json
    builder.Configuration.GetSection("RapidAPI:Hosts").Bind(options.Host);
    
    // Nếu bạn vẫn còn dùng thuộc tính Host cũ (để tương thích Instagram hiện tại)
    options.Host = builder.Configuration["RapidAPI:Host"] 
                   ?? builder.Configuration["RapidAPI:Hosts:Instagram"]
                   ?? throw new InvalidOperationException("RapidAPI Host không được cấu hình!");
});

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultureInfos = localizationSettings.SupportedCultures?.Any() == true
        ? localizationSettings.SupportedCultures.Select(c => new CultureInfo(c.Name)).ToArray()
        : new[] { new CultureInfo(localizationSettings.DefaultCulture ?? "en-US") };

    var defaultCultureName = localizationSettings.DefaultCulture ?? cultureInfos.First().Name;

    options.DefaultRequestCulture = new RequestCulture(defaultCultureName);
    options.SupportedCultures = cultureInfos;
    options.SupportedUICultures = cultureInfos;

    // Thêm provider cho QueryString để chuyển đổi ngôn ngữ dễ dàng
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// SỬA LỖI QUAN TRỌNG: Lấy RequestLocalizationOptions đã đăng ký và truyền vào middleware
var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value;
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthorization();

// Map API controllers before Razor Pages to ensure API routes are prioritized
app.MapControllers();
app.MapRazorPages();

app.Run();