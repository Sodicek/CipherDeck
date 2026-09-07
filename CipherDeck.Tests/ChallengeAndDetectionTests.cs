using CipherDeck.Core;
using CipherDeck.Core.Challenges;
using CipherDeck.Core.Detection;
using Xunit;

namespace CipherDeck.Tests;

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
            var cipher = CipherCatalog.All.Single(item => item.Name == challenge.CipherName);

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
            "Pozpátku",
            null,
            "Začni od konce.");

        Assert.True(challenge.IsCorrect("  tajna    zprava  "));
        Assert.False(challenge.IsCorrect("jiná zpráva"));
    }

    [Theory]
    [InlineData("KHOOR ZRUOG", "Caesarova šifra", "HELLO WORLD")]
    [InlineData("SVOOL DLIOW", "Atbash", "HELLO WORLD")]
    [InlineData("DLROW OLLEH", "Pozpátku", "HELLO WORLD")]
    public void DetectorRecognizesSimpleKnownCipher(string encrypted, string expectedCipher, string expectedPlainText)
    {
        var result = CipherDetector.Detect(encrypted);

        Assert.NotEmpty(result);
        Assert.StartsWith(expectedCipher, result[0].CipherName);
        Assert.Equal(expectedPlainText, result[0].SuggestedPlainText);
        Assert.InRange(result[0].Confidence, 0d, 1d);
    }

    [Fact]
    public void DetectorReturnsNoGuessForTextWithoutLetters()
    {
        Assert.Empty(CipherDetector.Detect("1234 !? 🔐"));
    }
}
