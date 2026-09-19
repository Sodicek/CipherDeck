using CipherDeck.Core;
using CipherDeck.Core.Challenges;
using CipherDeck.Core.Detection;
using Xunit;

namespace CipherDeck.Tests;

[Collection("Culture-sensitive tests")]
public sealed class ChallengeAndDetectionTests
{
    [Theory]
    [InlineData(ChallengeDifficulty.Easy)]
    [InlineData(ChallengeDifficulty.Medium)]
    [InlineData(ChallengeDifficulty.Hard)]
    public void GeneratedChallengeCanBeSolvedWithItsCipher(ChallengeDifficulty difficulty)
    {
        for (var seed = 0; seed < 20; seed++)
        {
            var challenge = ChallengeGenerator.Generate(difficulty, new Random(seed));
            var cipher = CipherCatalog.FindById(challenge.CipherId);

            Assert.NotNull(cipher);
            Assert.Equal(challenge.PlainText, cipher.Decrypt(challenge.EncryptedText, challenge.Key));
            Assert.NotEqual(challenge.PlainText, challenge.EncryptedText);
            Assert.False(string.IsNullOrWhiteSpace(challenge.Hint));
        }
    }

    [Fact]
    public void ChallengeAnswerIgnoresCaseAndRepeatedWhitespace()
    {
        var challenge = new CipherChallenge(
            ChallengeDifficulty.Easy,
            "TAJNA ZPRAVA",
            "AVARPZ ANJAT",
            CipherIds.Reverse,
            "Pozpátku",
            null,
            "Začni od konce.");

        Assert.True(challenge.IsCorrect("  tajna    zprava  "));
        Assert.False(challenge.IsCorrect("jiná zpráva"));
    }

    [Fact]
    public void ChallengeAnswerComparisonDoesNotDependOnCurrentCulture()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            var challenge = new CipherChallenge(
                ChallengeDifficulty.Easy,
                "CIPHER IS FUN",
                "NUF SI REHPIC",
                CipherIds.Reverse,
                "Reverse",
                null,
                "Start at the end.");

            Assert.True(challenge.IsCorrect("cipher is fun"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("KHOOR ZRUOG", CipherIds.Caesar, "HELLO WORLD")]
    [InlineData("SVOOL DLIOW", CipherIds.Atbash, "HELLO WORLD")]
    [InlineData("DLROW OLLEH", CipherIds.Reverse, "HELLO WORLD")]
    public void DetectorRecognizesSimpleKnownCipher(string encrypted, string expectedCipherId, string expectedPlainText)
    {
        var result = CipherDetector.Detect(encrypted);

        Assert.NotEmpty(result);
        Assert.Equal(expectedCipherId, result[0].CipherId);
        Assert.Equal(expectedPlainText, result[0].SuggestedPlainText);
        Assert.InRange(result[0].Confidence, 0d, 1d);
    }

    [Fact]
    public void DetectorReturnsNoGuessForTextWithoutLetters()
    {
        Assert.Empty(CipherDetector.Detect("1234 !? 🔐"));
    }

    [Fact]
    public void DetectorRecognizesSupplementaryPlaneLettersAsText()
    {
        Assert.NotEmpty(CipherDetector.Detect("𐐀𐐁𐐂"));
    }
}
