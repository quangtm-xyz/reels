using System.Collections.Generic;

namespace InstaReels.Models;

public class IndexViewModel
{
    public List<CultureOption> Languages { get; set; } = new();
    public List<FaqItem> Faqs { get; set; } = new();
    public string CurrentLanguage { get; set; } = string.Empty;
    public string CurrentLanguageDisplayName { get; set; } = string.Empty;
    public string AppTitle { get; set; } = string.Empty;
    public string ContactLinkText { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string DescriptionTitle { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public string FaqTitle { get; set; } = string.Empty;
    public string InputPlaceholder { get; set; } = string.Empty;
    public string DownloadButtonText { get; set; } = string.Empty;
    public string FooterContact { get; set; } = string.Empty;
    public string FooterLegal { get; set; } = string.Empty;
    public string FooterTools { get; set; } = string.Empty;
    public string FooterTerms { get; set; } = string.Empty;
    public string FooterPrivacy { get; set; } = string.Empty;
    public Dictionary<string, string> LocalizationStrings { get; set; } = new();
}

public class CultureOption
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
}

public class FaqItem
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "en-US";
    public List<CultureOption> SupportedCultures { get; set; } = new();
}



