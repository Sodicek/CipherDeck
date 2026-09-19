using System.Text;

namespace CipherDeck;

internal sealed class TextFileService
{
    internal const long MaximumFileSizeBytes = 5 * 1024 * 1024;

    public async Task<TextFileReadResult> ReadUtf8Async(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            if (stream.Length > MaximumFileSizeBytes)
                return new TextFileReadResult(TextFileReadStatus.TooLarge, null);

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var text = await reader.ReadToEndAsync(cancellationToken);
            return new TextFileReadResult(TextFileReadStatus.Success, text);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new TextFileReadResult(TextFileReadStatus.Failed, null);
        }
    }
}

internal readonly record struct TextFileReadResult(TextFileReadStatus Status, string? Text);

internal enum TextFileReadStatus
{
    Success,
    TooLarge,
    Failed
}
