using System.Globalization;
using CipherDeck.Core;
using CipherDeck.Core.Ciphers;
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
        var resourceKeys = new[]
        {
            "ReverseName", "ReverseDescription", "CaesarName", "CaesarDescription", "CaesarKeyLabel",
            "CaesarMissingKey", "AtbashName", "AtbashDescription", "VigenereName", "VigenereDescription",
            "VigenereKeyLabel", "VigenereMissingKey", "VigenereInvalidKey", "RailFenceName",
            "RailFenceDescription", "RailFenceKeyLabel", "RailFenceInvalidKey", "SkipName", "SkipDescription",
            "SkipKeyLabel", "SkipInvalidKey"
        };

        foreach (var key in resourceKeys)
        {
            Assert.False(string.IsNullOrWhiteSpace(CoreText.Get(key, CultureInfo.GetCultureInfo("cs-CZ"))));
            Assert.False(string.IsNullOrWhiteSpace(CoreText.Get(key, CultureInfo.GetCultureInfo("en-US"))));
        }
    }

    [Fact]
    public void LegacyCzechNamesResolveWhileEnglishIsActive()
    {
        using var culture = new TemporaryUiCulture("en-US");

        Assert.Equal(CipherIds.Reverse, CipherCatalog.FindByName("Pozpátku")?.Id);
        Assert.Equal(CipherIds.Vigenere, CipherCatalog.FindByName("Vigenèrova šifra")?.Id);
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
