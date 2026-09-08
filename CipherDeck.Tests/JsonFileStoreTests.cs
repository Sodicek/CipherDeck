using Xunit;

namespace CipherDeck.Tests;

public sealed class JsonFileStoreTests
{
    [Fact]
    public void SaveAndLoadRoundTripJsonData()
    {
        var path = CreateTemporaryPath();
        try
        {
            var value = new Dictionary<string, string> { ["theme"] = "dark" };

            var saved = JsonFileStore.Save(path, value, 1024);
            var loaded = JsonFileStore.Load<Dictionary<string, string>>(path, 1024);

            Assert.True(saved);
            Assert.NotNull(loaded);
            Assert.Equal("dark", loaded["theme"]);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void LoadRejectsMalformedAndOversizedFiles()
    {
        var path = CreateTemporaryPath();
        try
        {
            File.WriteAllText(path, "not-json");
            Assert.Null(JsonFileStore.Load<Dictionary<string, string>>(path, 1024));

            File.WriteAllText(path, new string('x', 65));
            Assert.Null(JsonFileStore.Load<Dictionary<string, string>>(path, 64));
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void FailedSaveKeepsPreviousFileAndRemovesTemporaryFile()
    {
        var path = CreateTemporaryPath();
        try
        {
            var original = new Dictionary<string, string> { ["value"] = "original" };
            Assert.True(JsonFileStore.Save(path, original, 1024));

            var selfReferencing = new Dictionary<string, object?>();
            selfReferencing["self"] = selfReferencing;

            Assert.False(JsonFileStore.Save(path, selfReferencing, 1024));
            var loaded = JsonFileStore.Load<Dictionary<string, string>>(path, 1024);

            Assert.NotNull(loaded);
            Assert.Equal("original", loaded["value"]);
            Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*.tmp"));
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void OversizedSaveDoesNotReplacePreviousFile()
    {
        var path = CreateTemporaryPath();
        try
        {
            var original = new Dictionary<string, string> { ["value"] = "ok" };
            Assert.True(JsonFileStore.Save(path, original, 1024));

            var oversized = new Dictionary<string, string> { ["value"] = new string('x', 2048) };
            Assert.False(JsonFileStore.Save(path, oversized, 128));

            var loaded = JsonFileStore.Load<Dictionary<string, string>>(path, 1024);
            Assert.NotNull(loaded);
            Assert.Equal("ok", loaded["value"]);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public void HistoryNormalizationDropsInvalidAndOversizedEntries()
    {
        var valid = CreateHistoryEntry("hello");
        var invalid = valid with { CipherName = " " };
        var oversized = CreateHistoryEntry(new string('x', 300 * 1024));

        var normalized = HistoryStore.Normalize([null, invalid, oversized, valid]);

        Assert.Equal([valid], normalized);
    }

    [Fact]
    public void HistoryNormalizationKeepsOnlyThirtyNewestEntries()
    {
        var entries = Enumerable.Range(0, 35)
            .Select(index => CreateHistoryEntry(index.ToString()))
            .ToList();

        var normalized = HistoryStore.Normalize(entries);

        Assert.Equal(30, normalized.Count);
        Assert.Equal("0", normalized[0].Input);
        Assert.Equal("29", normalized[^1].Input);
    }

    private static HistoryEntry CreateHistoryEntry(string text) => new(
        DateTime.UnixEpoch,
        "Atbash",
        true,
        text,
        text,
        null);

    private static string CreateTemporaryPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "CipherDeck.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "data.json");
    }

    private static void DeleteTemporaryDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}
