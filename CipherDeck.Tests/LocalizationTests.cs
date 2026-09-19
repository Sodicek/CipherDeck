using System.Globalization;
using CipherDeck.Core;
using CipherDeck.Core.Ciphers;
using CipherDeck.Core.Challenges;
using CipherDeck.Core.Detection;
using CipherDeck.Core.Learning;
using CipherDeck.Core.Localization;
using Xunit;

namespace CipherDeck.Tests;

[Collection("Culture-sensitive tests")]
public sealed class LocalizationTests
{
    [Theory]
    [InlineData("cs-CZ", "Pozpátku", "POSUN", "Přeskakování")]
    [InlineData("en-US", "Reverse", "SHIFT", "Skip transposition")]
    public void CipherMetadataUsesCurrentUiCulture(
        string cultureName,
        string reverseName,
        string caesarKeyLabel,
        string skipName)
    {
        using var culture = new TemporaryUiCulture(cultureName);

        Assert.Equal(reverseName, new ReverseCipher().Name);
        Assert.Equal(caesarKeyLabel, new CaesarCipher().KeyLabel);
        Assert.Equal(skipName, new SkipCipher().Name);
        Assert.NotEmpty(new RailFenceCipher().Description);
    }

    [Fact]
    public void EnglishValidationMessageIsLocalized()
    {
        using var culture = new TemporaryUiCulture("en-US");

        var exception = Assert.Throws<ArgumentException>(() => new SkipCipher().Encrypt("text", new CipherKey(Number: 1)));

        Assert.StartsWith("The step must be between 2 and 20.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryBaseResourceHasAnEnglishTranslation()
    {
        var czech = CultureInfo.GetCultureInfo("cs-CZ");
        var english = CultureInfo.GetCultureInfo("en-US");
        var resourceKeys = CoreText.GetKeys(czech);

        foreach (var key in resourceKeys)
        {
            Assert.False(string.IsNullOrWhiteSpace(CoreText.Get(key, czech)));
            Assert.False(string.IsNullOrWhiteSpace(CoreText.Get(key, english)));
        }

        Assert.True(resourceKeys.SetEquals(CoreText.GetKeys(english)));
    }

    [Fact]
    public void ChallengeDetectionAndExplanationUseEnglishResources()
    {
        using var culture = new TemporaryUiCulture("en-US");

        var challenge = ChallengeGenerator.Generate(ChallengeDifficulty.Easy, new Random(0));
        var detection = CipherDetector.Detect("DLROW OLLEH")[0];
        var explanation = CipherExplainer.Explain(new ReverseCipher(), "HELLO", encrypt: true);

        Assert.Contains(challenge.PlainText, CoreText.GetList("ChallengeEasyPhrases"));
        Assert.DoesNotContain("Začni", challenge.Hint, StringComparison.Ordinal);
        Assert.Equal("Reverse", detection.CipherName);
        Assert.Equal("How the input text was encrypted", explanation.Summary);
        Assert.Equal("Split into characters", explanation.Steps[0].Title);
    }

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
    public void LegacyCzechNamesResolveWhileEnglishIsActive()
    {
        using var culture = new TemporaryUiCulture("en-US");

        Assert.Equal(CipherIds.Reverse, CipherCatalog.FindByName("Pozpátku")?.Id);
        Assert.Equal(CipherIds.Vigenere, CipherCatalog.FindByName("Vigenèrova šifra")?.Id);
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
