using System.Text;

namespace CipherDeck;

internal static class HistoryStore
{
    private const int MaximumEntryTextSizeBytes = 256 * 1024;
    private const int MaximumHistoryFileSizeBytes = 8 * 1024 * 1024;

    private static readonly string HistoryDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CipherDeck");

    private static readonly string HistoryPath = Path.Combine(HistoryDirectory, "history.json");

    public static IReadOnlyList<HistoryEntry> Load()
    {
        var entries = JsonFileStore.Load<List<HistoryEntry?>>(HistoryPath, MaximumHistoryFileSizeBytes);
        return entries is null ? [] : Normalize(entries);
    }

    public static bool Save(IEnumerable<HistoryEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var recentEntries = Normalize(entries);
        return JsonFileStore.Save(HistoryPath, recentEntries, MaximumHistoryFileSizeBytes);
    }

    internal static bool CanStore(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (string.IsNullOrWhiteSpace(entry.CipherName) || entry.Input is null || entry.Output is null)
            return false;

        var inputSize = Encoding.UTF8.GetByteCount(entry.Input);
        var outputSize = Encoding.UTF8.GetByteCount(entry.Output);
        return inputSize + (long)outputSize <= MaximumEntryTextSizeBytes;
    }

    internal static IReadOnlyList<HistoryEntry> Normalize(IEnumerable<HistoryEntry?> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return entries
            .OfType<HistoryEntry>()
            .Where(CanStore)
            .Take(30)
            .ToList();
    }
}
