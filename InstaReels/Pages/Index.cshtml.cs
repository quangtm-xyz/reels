using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InstaReels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace InstaReels.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly LocalizationSettings _localizationSettings;
    private static readonly Dictionary<string, string> LanguageResourceKeyMap = new()
    {
        ["en-US"] = "Lang.English",
        ["vi"] = "Lang.Vietnamese",
        ["id"] = "Lang.Indonesian",
        ["bn"] = "Lang.Bengali",
        ["ar"] = "Lang.Arabic"
    };

    public IndexViewModel ViewModel { get; private set; } = new();

    public IndexModel(
        ILogger<IndexModel> logger,
        IStringLocalizerFactory localizerFactory,
        IOptions<LocalizationSettings> localizationOptions)
    {
        _logger = logger;
        _localizer = localizerFactory.Create("Pages.Index", typeof(IndexModel).Assembly.GetName().Name!);
        _localizationSettings = localizationOptions.Value;
    }

    public void OnGet()
    {
        PopulateLanguages();
        PopulateCopy();
        PopulateFaqs();

        ViewData["Title"] = ViewModel.AppTitle;
    }

    private void PopulateLanguages()
    {
        var languages = (_localizationSettings.SupportedCultures ?? new())
            .Select(c => new CultureOption
            {
                Name = c.Name,
                DisplayName = ResolveLanguageDisplayName(c),
                NativeName = string.IsNullOrWhiteSpace(c.NativeName) ? c.DisplayName : c.NativeName
            })
            .ToList();

        var currentCulture = CultureInfo.CurrentUICulture.Name;

        ViewModel.Languages = languages;
        ViewModel.CurrentLanguage = currentCulture;
        ViewModel.CurrentLanguageDisplayName =
            languages.FirstOrDefault(l => l.Name.Equals(currentCulture, StringComparison.OrdinalIgnoreCase))
                ?.DisplayName ?? currentCulture;
    }

    private string ResolveLanguageDisplayName(CultureOption culture)
    {
        if (LanguageResourceKeyMap.TryGetValue(culture.Name, out var resourceKey))
        {
            var value = _localizer[resourceKey];
            if (!value.ResourceNotFound)
            {
                return value;
            }
        }

        return string.IsNullOrWhiteSpace(culture.DisplayName) ? culture.Name : culture.DisplayName;
    }

    private void PopulateCopy()
    {
        ViewModel.AppTitle = _localizer["App.Title"];
        ViewModel.ContactLinkText = _localizer["UI.Contact"];
        ViewModel.InputPlaceholder = _localizer["UI.Input.Placeholder"];
        ViewModel.DownloadButtonText = _localizer["UI.Download"];
        ViewModel.IntroText = _localizer["UI.Intro"];
        ViewModel.DescriptionTitle = _localizer["Description.Title"];
        ViewModel.DescriptionText = _localizer["Description.Text"];
        ViewModel.FaqTitle = _localizer["FAQ.Title"];

        ViewModel.FooterContact = _localizer["Footer.Contact"];
        ViewModel.FooterLegal = _localizer["Footer.Legal"];
        ViewModel.FooterTerms = _localizer["Footer.Terms"];
        ViewModel.FooterPrivacy = _localizer["Footer.Privacy"];
        ViewModel.FooterTools = _localizer["Footer.Tools"];

        ViewModel.LocalizationStrings = new Dictionary<string, string>
        {
            ["errorGeneral"] = _localizer["Error.General"],
            ["errorInvalidLink"] = _localizer["Error.InvalidLink"],
            ["statusProcessing"] = _localizer["Status.Processing"]
        };
    }

    private void PopulateFaqs()
    {
        var items = new List<FaqItem>();

        for (var index = 1; index <= 13; index++)
        {
            var questionKey = $"FAQ.{index}.Question";
            var answerKey = $"FAQ.{index}.Answer";

            items.Add(new FaqItem
            {
                Question = _localizer[questionKey],
                Answer = _localizer[answerKey]
            });
        }

        ViewModel.Faqs = items;
    }
}