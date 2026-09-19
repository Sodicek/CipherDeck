using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Ciphers;
using Xunit;

namespace CipherDeck.Tests;

public sealed class CipherEdgeCaseTests
{
    public static IEnumerable<object[]> AllCiphers =>
        CipherCatalog.All.Select(cipher => new object[] { cipher });

    private static CipherKey? DefaultKeyFor(ICipher cipher) => cipher.KeyType switch
    {
        CipherKeyType.Number => new CipherKey(Number: cipher.DefaultNumericKey),
        CipherKeyType.Text => new CipherKey(Text: cipher.DefaultTextKey),
        _ => null
    };

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void EmptyInputRoundTripsToEmptyString(ICipher cipher)
    {
        var key = DefaultKeyFor(cipher);

        var encrypted = cipher.Encrypt(string.Empty, key);

        Assert.Equal(string.Empty, encrypted);
        Assert.Equal(string.Empty, cipher.Decrypt(encrypted, key));
    }

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void WhitespaceOnlyInputRoundTrips(ICipher cipher)
    {
        var key = DefaultKeyFor(cipher);
        const string input = "   \t  ";

        var encrypted = cipher.Encrypt(input, key);

        Assert.Equal(input, cipher.Decrypt(encrypted, key));
    }

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void LongMixedContentInputRoundTrips(ICipher cipher)
    {
        var key = DefaultKeyFor(cipher);
        // "Příliš žluťoučký kůň" is the
        // classic Czech pangram; the rest mixes digits, punctuation, accented
        // Latin letters and astral-plane emoji.
        var input = string.Concat(Enumerable.Repeat(
            "Příliš žluťoučký kůň úpěl " +
            "ďábelské ódy! 123 – naïve café résumé " +
            "\U0001F510\U0001F389 ",
            50));

        var encrypted = cipher.Encrypt(input, key);

        Assert.Equal(input, cipher.Decrypt(encrypted, key));
    }

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void CombiningDiacriticsRoundTrip(ICipher cipher)
    {
        var key = DefaultKeyFor(cipher);
        // "cafe" where the final "e" is followed by a combining acute accent
        // (U+0301). Together they form one grapheme cluster ("café") made
        // of two UTF-16 chars, unlike a precomposed "é".
        var input = "caf" + "é" + " na" + "ï" + "ve";

        var encrypted = cipher.Encrypt(input, key);

        Assert.Equal(input, cipher.Decrypt(encrypted, key));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    public void CaesarCipherAcceptsBoundaryShiftValues(int shift)
    {
        var cipher = new CaesarCipher();
        var key = new CipherKey(Number: shift);

        var encrypted = cipher.Encrypt("Hello", key);

        Assert.Equal("Hello", cipher.Decrypt(encrypted, key));
        Assert.NotEqual("Hello", encrypted);
    }

    [Fact]
    public void CaesarCipherWrapsNegativeAndOversizedShifts()
    {
        var cipher = new CaesarCipher();

        var withNegative = cipher.Encrypt("Hello", new CipherKey(Number: -3));
        var withEquivalentPositive = cipher.Encrypt("Hello", new CipherKey(Number: 23));

        Assert.Equal(withEquivalentPositive, withNegative);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    public void RailFenceCipherAcceptsBoundaryRailCounts(int rails)
    {
        var cipher = new RailFenceCipher();
        var key = new CipherKey(Number: rails);
        const string input = "The quick brown fox jumps over the lazy dog";

        var encrypted = cipher.Encrypt(input, key);

        Assert.Equal(input, cipher.Decrypt(encrypted, key));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    public void SkipCipherAcceptsBoundaryStepValues(int step)
    {
        var cipher = new SkipCipher();
        var key = new CipherKey(Number: step);
        const string input = "The quick brown fox jumps over the lazy dog";

        var encrypted = cipher.Encrypt(input, key);

        Assert.Equal(input, cipher.Decrypt(encrypted, key));
    }

    [Fact]
    public void VigenereCipherAcceptsLowercaseAndMixedCaseKeys()
    {
        var cipher = new VigenereCipher();
        const string input = "Attack at dawn!";

        var lowerKeyResult = cipher.Encrypt(input, new CipherKey(Text: "lemon"));
        var mixedKeyResult = cipher.Encrypt(input, new CipherKey(Text: "LeMoN"));

        Assert.Equal(lowerKeyResult, mixedKeyResult);
        Assert.Equal(input, cipher.Decrypt(lowerKeyResult, new CipherKey(Text: "lemon")));
    }

    [Fact]
    public void VigenereCipherStripsWhitespaceFromKey()
    {
        var cipher = new VigenereCipher();
        const string input = "SECRETMESSAGE";

        var withSpaces = cipher.Encrypt(input, new CipherKey(Text: "LE MON"));
        var withoutSpaces = cipher.Encrypt(input, new CipherKey(Text: "LEMON"));

        Assert.Equal(withoutSpaces, withSpaces);
    }

    [Fact]
    public void VigenereCipherRejectsEmptyKey()
    {
        var cipher = new VigenereCipher();

        Assert.Throws<ArgumentException>(() => cipher.Encrypt("text", new CipherKey(Text: string.Empty)));
        Assert.Throws<ArgumentException>(() => cipher.Encrypt("text", new CipherKey(Text: "   ")));
    }

    [Fact]
    public void FrequencyAnalyzerCountsDecomposedDiacriticsAsBaseLetterOnly()
    {
        // "e" + combining acute accent (U+0301): the mark itself is not a
        // letter, so this must be counted as two occurrences of "E", not of
        // some composed grapheme.
        var result = FrequencyAnalyzer.AnalyzeLetters("éé");

        Assert.Single(result);
        Assert.Equal("E", result[0].Symbol);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void FrequencyAnalyzerHandlesSurrogatePairEmojiWithoutCountingThemAsLetters()
    {
        var result = FrequencyAnalyzer.AnalyzeLetters("Aa\U0001F510\U0001F389");

        Assert.Single(result);
        Assert.Equal("A", result[0].Symbol);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void FrequencyAnalyzerReturnsEmptyResultForEmptyInput()
    {
        Assert.Empty(FrequencyAnalyzer.AnalyzeLetters(string.Empty));
    }
}
