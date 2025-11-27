# Index.cshtml (Final)

```cshtml
@page
@model IndexModel
@using System.Text.Json
@{
    var localizedPayload = JsonSerializer.Serialize(Model.ViewModel.LocalizationStrings);
}

<div id="index-page"
     class="min-h-screen bg-white"
     data-download-endpoint="/api/download/reels"
     data-localized='@Html.Raw(localizedPayload)'>

    <partial name="Partials/_Header" model="Model.ViewModel"/>

    <main class="w-[95%] md:w-[80%] mx-auto px-4 py-8 space-y-10">
        <section class="rounded-xl bg-gradient-to-br from-blue-50 to-white p-6 md:p-10 shadow-sm">
            <div id="errorAlert" class="hidden mb-4 rounded-lg border border-red-300 bg-red-50 px-4 py-3 text-red-800" role="alert">
                <div class="flex items-center gap-2">
                    <svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20" aria-hidden="true">
                        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
                    </svg>
                    <span id="errorMessage"></span>
                </div>
            </div>

            <div class="flex flex-col gap-4 md:flex-row md:items-center">
                <input type="text"
                       id="instagramLink"
                       placeholder="@Model.ViewModel.InputPlaceholder"
                       class="w-full rounded-lg border border-gray-200 px-5 py-4 text-lg text-gray-900 focus:border-blue-500 focus:ring-blue-500"/>
                <button type="button"
                        id="downloadButton"
                        class="w-full rounded-lg bg-blue-600 px-8 py-4 text-lg font-semibold text-white transition hover:bg-blue-700 focus-visible:outline-blue-500 md:w-auto">
                    @Model.ViewModel.DownloadButtonText
                </button>
            </div>
        </section>

        <section class="text-center">
            <p class="text-lg text-gray-800">@Model.ViewModel.IntroText</p>
        </section>

        <section class="space-y-4">
            <h2 class="text-2xl font-bold text-gray-900 md:text-3xl">@Model.ViewModel.DescriptionTitle</h2>
            <p class="text-base leading-relaxed text-gray-700">@Model.ViewModel.DescriptionText</p>
        </section>

        <section class="space-y-6">
            <h2 class="text-2xl font-bold text-gray-900 md:text-3xl">@Model.ViewModel.FaqTitle</h2>
            <partial name="Partials/_FAQList" model="Model.ViewModel.Faqs"/>
        </section>
    </main>

    <partial name="Partials/_Footer" model="Model.ViewModel"/>
</div>

@section Scripts
{
    <script src="~/js/index.js" asp-append-version="true"></script>
}
```



