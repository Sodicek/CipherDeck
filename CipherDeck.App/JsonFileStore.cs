using System.Text.Json;

namespace CipherDeck;

internal static class JsonFileStore
{
    public static T? Load<T>(string path, long maximumFileSizeBytes)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFileSizeBytes);

        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.SequentialScan);

            if (stream.Length > maximumFileSizeBytes)
                return null;

            return JsonSerializer.Deserialize<T>(stream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    public static bool Save<T>(string path, T value, long maximumFileSizeBytes)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFileSizeBytes);

        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directory))
            return false;

        string? temporaryPath = null;
        try
        {
            Directory.CreateDirectory(directory);
            temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            {
                JsonSerializer.Serialize(stream, value);
                if (stream.Length > maximumFileSizeBytes)
                    return false;

                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path, overwrite: true);
            temporaryPath = null;
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
        finally
        {
            if (temporaryPath is not null)
                TryDelete(temporaryPath);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }
}
