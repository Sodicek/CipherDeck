using System.Text.Json;

namespace CipherDeck;

internal static class HistoryStore
{
    private static readonly string HistoryDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CipherDeck");

    private static readonly string HistoryPath = Path.Combine(HistoryDirectory, "history.json");

    public static IReadOnlyList<HistoryEntry> Load()
    {
        try
        {
            return File.Exists(HistoryPath)
                ? JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(HistoryPath)) ?? []
                : [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    public static void Save(IEnumerable<HistoryEntry> entries)
    {
        try
        {
            Directory.CreateDirectory(HistoryDirectory);
            var recentEntries = entries.Take(30).ToList();
            File.WriteAllText(HistoryPath, JsonSerializer.Serialize(recentEntries));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
