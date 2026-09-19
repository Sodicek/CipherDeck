using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Detection;
using CipherDeck.Core.Learning;
using Xunit;

namespace CipherDeck.Tests;

public sealed class CancellationTests
{
    public static IEnumerable<object[]> AllCiphers =>
        CipherCatalog.All.Select(cipher => new object[] { cipher });

    [Theory]
    [MemberData(nameof(AllCiphers))]
    public void EveryCipherHonorsCancellation(ICipher cipher)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            cipher.Encrypt("A long enough input", DefaultKeyFor(cipher), cancellation.Token));
    }

    [Fact]
    public void FrequencyAnalysisHonorsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            FrequencyAnalyzer.AnalyzeLetters("SOME TEXT", cancellation.Token));
    }

    [Fact]
    public void CipherDetectionHonorsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            CipherDetector.Detect("KHOOR ZRUOG", cancellation.Token));
    }

    [Fact]
    public void ExplanationHonorsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            CipherExplainer.Explain(
                CipherCatalog.FindById(CipherIds.Caesar)!,
                "HELLO",
                encrypt: true,
                new CipherKey(Number: 3),
                cancellation.Token));
    }

    private static CipherKey? DefaultKeyFor(ICipher cipher) => cipher.KeyType switch
    {
        CipherKeyType.Number => new CipherKey(Number: cipher.DefaultNumericKey),
        CipherKeyType.Text => new CipherKey(Text: cipher.DefaultTextKey),
        _ => null
    };
}
