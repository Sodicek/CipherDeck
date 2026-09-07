using CipherDeck.Core;
using CipherDeck.Core.Ciphers;
using CipherDeck.Core.Learning;
using Xunit;

namespace CipherDeck.Tests;

public sealed class LearningTests
{
    public static IEnumerable<object[]> AllCiphers =>
        CipherCatalog.All.Select(cipher => new object[] { cipher });

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void ExplanationProducesStepsAndCorrectResult(ICipher cipher)
    {
        var key = DefaultKeyFor(cipher);
        const string input = "HELLO WORLD";

        var explanation = CipherExplainer.Explain(cipher, input, encrypt: true, key);

        Assert.Equal(cipher.Name, explanation.CipherName);
        Assert.Equal(cipher.Encrypt(input, key), explanation.Result);
        Assert.True(explanation.Steps.Count >= 3);
        Assert.All(explanation.Steps, step =>
        {
            Assert.False(string.IsNullOrWhiteSpace(step.Title));
            Assert.False(string.IsNullOrWhiteSpace(step.Detail));
        });
    }

    [Fact]
    public void VigenereExplanationShowsKeyLetters()
    {
        var explanation = CipherExplainer.Explain(
            new VigenereCipher(),
            "ATTACK",
            encrypt: true,
            new CipherKey(Text: "LEMON"));

        Assert.Contains(explanation.Steps, step => step.Snapshot.Contains("LEMON"));
        Assert.Contains(explanation.Steps, step => step.Snapshot.Contains("A  +  L"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void NumericKeyGeneratorStaysInsideCipherRange(int seed)
    {
        var random = new Random(seed);

        foreach (var cipher in CipherCatalog.All.Where(item => item.KeyType == CipherKeyType.Number))
        {
            for (var sample = 0; sample < 100; sample++)
            {
                var number = CipherKeyGenerator.Generate(cipher, random)!.Number;
                Assert.InRange(number!.Value, cipher.MinimumNumericKey, cipher.MaximumNumericKey);
            }
        }
    }

    [Fact]
    public void TextKeyGeneratorCreatesEightUppercaseLetters()
    {
        var key = CipherKeyGenerator.Generate(new VigenereCipher(), new Random(42));

        Assert.NotNull(key?.Text);
        Assert.Equal(8, key.Text.Length);
        Assert.All(key.Text, character => Assert.InRange(character, 'A', 'Z'));
    }

    [Fact]
    public void KeyGeneratorReturnsNullForKeylessCipher()
    {
        Assert.Null(CipherKeyGenerator.Generate(new AtbashCipher(), new Random(42)));
    }

    private static CipherKey? DefaultKeyFor(ICipher cipher) => cipher.KeyType switch
    {
        CipherKeyType.Number => new CipherKey(Number: cipher.DefaultNumericKey),
        CipherKeyType.Text => new CipherKey(Text: cipher.DefaultTextKey),
        _ => null
    };
}
