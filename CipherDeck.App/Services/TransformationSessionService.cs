using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Detection;
using CipherDeck.Core.Learning;

namespace CipherDeck;

internal sealed class TransformationSessionService : IDisposable
{
    private readonly LatestOperationRunner _operations = new();

    public Task<LatestOperationResult<TransformationResult>> TransformAsync(TransformationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _operations.RunAsync(cancellationToken =>
        {
            try
            {
                var result = request.IsEncryption
                    ? request.Cipher.Encrypt(request.Input, request.Key, cancellationToken)
                    : request.Cipher.Decrypt(request.Input, request.Key, cancellationToken);
                return new TransformationResult(result, null);
            }
            catch (ArgumentException exception)
            {
                return new TransformationResult(null, exception.Message);
            }
        });
    }

    public Task<LatestOperationResult<IReadOnlyList<LetterFrequency>>> AnalyzeAsync(string text) =>
        _operations.RunAsync(cancellationToken => FrequencyAnalyzer.AnalyzeLetters(text, cancellationToken));

    public Task<LatestOperationResult<CipherExplanation>> ExplainAsync(
        ICipher cipher,
        string input,
        bool isEncryption,
        CipherKey? key) =>
        _operations.RunAsync(cancellationToken =>
            CipherExplainer.Explain(cipher, input, isEncryption, key, cancellationToken));

    public Task<LatestOperationResult<IReadOnlyList<CipherDetection>>> DetectAsync(string text) =>
        _operations.RunAsync(cancellationToken => CipherDetector.Detect(text, cancellationToken));

    public void Cancel() => _operations.Cancel();

    public void Dispose() => _operations.Dispose();
}

internal sealed record TransformationRequest(
    ICipher Cipher,
    string Input,
    CipherKey? Key,
    bool IsEncryption);

internal sealed record TransformationResult(string? Output, string? ErrorMessage);
