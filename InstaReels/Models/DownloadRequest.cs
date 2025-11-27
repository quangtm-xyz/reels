using System.ComponentModel.DataAnnotations;

namespace InstaReels.Models;

public class DownloadRequest
{
    [Required]
    public string Url { get; set; } = string.Empty;
}




