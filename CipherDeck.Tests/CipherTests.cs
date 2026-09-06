using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Ciphers;
using Xunit;

namespace CipherDeck.Tests;

public sealed class CipherTests
{
    [Fact]
    public void FrequencyAnalyzerGroupsCaseAndKeepsDiacritics()
    {
        var result = FrequencyAnalyzer.AnalyzeLetters("Aa bb! Čč 123");

        Assert.Equal(3, result.Count);
        Assert.All(result, item => Assert.Equal(2, item.Count));
        Assert.All(result, item => Assert.Equal(100d / 3d, item.Percentage, precision: 8));
        Assert.Equal(["A", "B", "Č"], result.Select(item => item.Symbol));
    }

    [Fact]
    public void FrequencyAnalyzerReturnsEmptyResultWhenTextHasNoLetters()
    {
        Assert.Empty(FrequencyAnalyzer.AnalyzeLetters("123 !? 🔐"));
    }

    [Fact]
    public void CatalogContainsFirstThreeCiphers()
    {
        Assert.Collection(
            CipherCatalog.All,
            cipher => Assert.IsType<ReverseCipher>(cipher),
            cipher => Assert.IsType<CaesarCipher>(cipher),
            cipher => Assert.IsType<AtbashCipher>(cipher),
            cipher => Assert.IsType<VigenereCipher>(cipher),
            cipher => Assert.IsType<RailFenceCipher>(cipher),
            cipher => Assert.IsType<SkipCipher>(cipher));
    }

    [Theory]
    [InlineData("CipherDeck", "kceDrehpiC")]
    [InlineData("Ahoj 👋!", "!👋 johA")]
    [InlineData("", "")]
    public void ReverseCipherReversesTextElements(string input, string expected)
    {
        var cipher = new ReverseCipher();

        Assert.Equal(expected, cipher.Encrypt(input));
        Assert.Equal(input, cipher.Decrypt(expected));
    }

    [Theory]
    [InlineData("Hello, World!", 3, "Khoor, Zruog!")]
    [InlineData("XYZ xyz", 3, "ABC abc")]
    [InlineData("Příliš žluťoučký kůň", 1, "Qřímjš žmvťpvčlý lůň")]
    public void CaesarCipherShiftsLatinLettersAndPreservesOtherCharacters(
        string input,
        int key,
        string expected)
    {
        var cipher = new CaesarCipher();

        var cipherKey = new CipherKey(Number: key);
        var encrypted = cipher.Encrypt(input, cipherKey);

        Assert.Equal(expected, encrypted);
        Assert.Equal(input, cipher.Decrypt(encrypted, cipherKey));
    }

    [Fact]
    public void CaesarCipherRequiresKey()
    {
        var cipher = new CaesarCipher();

        Assert.Throws<ArgumentException>(() => cipher.Encrypt("abc"));
    }

    [Theory]
    [InlineData("ABC xyz", "ZYX cba")]
    [InlineData("Ahoj, světe! 123", "Zslq, heěgv! 123")]
    [InlineData("", "")]
    public void AtbashCipherMirrorsLatinAlphabet(string input, string expected)
    {
        var cipher = new AtbashCipher();

        var encrypted = cipher.Encrypt(input);

        Assert.Equal(expected, encrypted);
        Assert.Equal(input, cipher.Decrypt(encrypted));
    }

    [Theory]
    [InlineData("ATTACKATDAWN", "LEMON", "LXFOPVEFRNHR")]
    [InlineData("Attack at dawn!", "LEMON", "Lxfopv ef rnhr!")]
    public void VigenereCipherUsesRepeatingTextKey(string input, string key, string expected)
    {
        var cipher = new VigenereCipher();
        var cipherKey = new CipherKey(Text: key);

        var encrypted = cipher.Encrypt(input, cipherKey);

        Assert.Equal(expected, encrypted);
        Assert.Equal(input, cipher.Decrypt(encrypted, cipherKey));
    }

    [Fact]
    public void VigenereCipherRejectsNonLatinKey()
    {
        var cipher = new VigenereCipher();

        Assert.Throws<ArgumentException>(() => cipher.Encrypt("text", new CipherKey(Text: "klíč")));
    }

    [Theory]
    [InlineData("WEAREDISCOVEREDFLEEATONCE", 3, "WECRLTEERDSOEEFEAOCAIVDEN")]
    [InlineData("HELLO", 2, "HLOEL")]
    public void RailFenceCipherFollowsZigzagPattern(string input, int rails, string expected)
    {
        var cipher = new RailFenceCipher();
        var cipherKey = new CipherKey(Number: rails);

        var encrypted = cipher.Encrypt(input, cipherKey);

        Assert.Equal(expected, encrypted);
        Assert.Equal(input, cipher.Decrypt(encrypted, cipherKey));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(21)]
    public void RailFenceCipherRejectsInvalidRailCount(int rails)
    {
        var cipher = new RailFenceCipher();

        Assert.Throws<ArgumentException>(() => cipher.Encrypt("text", new CipherKey(Number: rails)));
    }

    [Theory]
    [InlineData("ABCDEFGHIJ", 3, "ADGJBEHCFI")]
    [InlineData("1234567", 2, "1357246")]
    public void SkipCipherReadsTextByColumns(string input, int step, string expected)
    {
        var cipher = new SkipCipher();
        var cipherKey = new CipherKey(Number: step);

        var encrypted = cipher.Encrypt(input, cipherKey);

        Assert.Equal(expected, encrypted);
        Assert.Equal(input, cipher.Decrypt(encrypted, cipherKey));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(21)]
    public void SkipCipherRejectsInvalidStep(int step)
    {
        var cipher = new SkipCipher();

        Assert.Throws<ArgumentException>(() => cipher.Encrypt("text", new CipherKey(Number: step)));
    }

    [Theory]
    [InlineData("Text with spaces 123!", 7)]
    [InlineData("Čeština + emoji 🔐", 25)]
    public void EveryCipherCanRestoreItsInput(string input, int key)
    {
        foreach (var cipher in CipherCatalog.All)
        {
            var cipherKey = cipher.KeyType switch
            {
                CipherKeyType.Number => new CipherKey(
                    Number: Math.Clamp(key, cipher.MinimumNumericKey, cipher.MaximumNumericKey)),
                CipherKeyType.Text => new CipherKey(Text: "SECRET"),
                _ => null
            };
            var encrypted = cipher.Encrypt(input, cipherKey);

            Assert.Equal(input, cipher.Decrypt(encrypted, cipherKey));
        }
    }
}
