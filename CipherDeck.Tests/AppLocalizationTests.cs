using System.Globalization;
using CipherDeck.Core;
using Xunit;

namespace CipherDeck.Tests;

[Collection("Culture-sensitive tests")]
public sealed class AppLocalizationTests
{
    [Fact]
    public void EveryAppResourceHasAnEnglishTranslation()
    {
        var czech = CultureInfo.GetCultureInfo("cs-CZ");
        var english = CultureInfo.GetCultureInfo("en-US");
        var resourceKeys = AppText.GetKeys(czech);

        foreach (var key in resourceKeys)
        {
            Assert.False(string.IsNullOrWhiteSpace(AppText.Get(key, czech)));
            Assert.False(string.IsNullOrWhiteSpace(AppText.Get(key, english)));
        }

        Assert.True(resourceKeys.SetEquals(AppText.GetKeys(english)));
    }

    [Fact]
    public void HistoryUsesCurrentLanguageAndStableCipherId()
    {
        using var culture = new TemporaryUiCulture("en-US");
        var entry = new HistoryEntry(
            new DateTime(2026, 9, 19, 12, 34, 56),
            "Pozpátku",
            true,
            "ABC",
            "CBA",
            null,
            CipherIds.Reverse);

        Assert.Contains("Reverse", entry.DisplayText, StringComparison.Ordinal);
        Assert.Contains("encryption", entry.DisplayText, StringComparison.Ordinal);
        Assert.DoesNotContain("Pozpátku", entry.DisplayText, StringComparison.Ordinal);
    }

    [Fact]
    public void DisplayVersionMatchesReleaseVersion()
    {
        Assert.Equal("v0.10.0", AppInfo.DisplayVersion);
    }

    [Theory]
    [InlineData(null, AppLanguage.Czech)]
    [InlineData("de", AppLanguage.Czech)]
    [InlineData("CS", AppLanguage.Czech)]
    [InlineData("en", AppLanguage.English)]
    [InlineData("EN", AppLanguage.English)]
    public void AppLanguageNormalizesSupportedLanguageCodes(string? input, string expected)
    {
        Assert.Equal(expected, AppLanguage.Normalize(input));
    }

    private sealed class TemporaryUiCulture : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentUICulture;

        public TemporaryUiCulture(string cultureName) =>
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

        public void Dispose() => CultureInfo.CurrentUICulture = _original;
    }
}

[CollectionDefinition("Culture-sensitive tests", DisableParallelization = true)]
public sealed class CultureSensitiveTestCollection;
