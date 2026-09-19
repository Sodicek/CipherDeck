using CipherDeck.Core;
using CipherDeck.Core.Ciphers;
using Xunit;

namespace CipherDeck.Tests;

public sealed class ApplicationServicesTests
{
    [Fact]
    public async Task TransformationSessionReturnsCipherOutput()
    {
        using var service = new TransformationSessionService();

        var result = await service.TransformAsync(
            new TransformationRequest(new AtbashCipher(), "Hello", null, IsEncryption: true));

        Assert.True(result.IsCurrent);
        Assert.Equal("Svool", result.Value.Output);
        Assert.Null(result.Value.ErrorMessage);
    }

    [Fact]
    public async Task TransformationSessionReturnsValidationError()
    {
        using var service = new TransformationSessionService();

        var result = await service.TransformAsync(
            new TransformationRequest(new VigenereCipher(), "Hello", new CipherKey(Text: ""), IsEncryption: true));

        Assert.True(result.IsCurrent);
        Assert.Null(result.Value.Output);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.ErrorMessage));
    }

    [Fact]
    public void HistoryServiceAddsNewestEntryAndPersistsStableCipherId()
    {
        IReadOnlyList<HistoryEntry>? savedEntries = null;
        var service = new HistoryService(
            () => [],
            entries =>
            {
                savedEntries = entries.ToList();
                return true;
            });

        var result = service.Add(CreateHistoryEntry("hello"));

        Assert.Equal(HistoryAddResult.Saved, result);
        var entry = Assert.Single(service.Entries);
        Assert.Equal(CipherIds.Atbash, entry.CipherId);
        Assert.Equal(service.Entries, savedEntries);
    }

    [Fact]
    public void HistoryServiceRejectsOversizedEntryWithoutSaving()
    {
        var saveCalled = false;
        var service = new HistoryService(
            () => [],
            _ =>
            {
                saveCalled = true;
                return true;
            });

        var result = service.Add(CreateHistoryEntry(new string('x', 300 * 1024)));

        Assert.Equal(HistoryAddResult.EntryTooLarge, result);
        Assert.Empty(service.Entries);
        Assert.False(saveCalled);
    }

    [Fact]
    public void HistoryServiceKeepsThirtyNewestEntries()
    {
        var initialEntries = Enumerable.Range(0, 30)
            .Select(index => CreateHistoryEntry(index.ToString()))
            .ToList();
        var service = new HistoryService(() => initialEntries, _ => true);

        var result = service.Add(CreateHistoryEntry("new"));

        Assert.Equal(HistoryAddResult.Saved, result);
        Assert.Equal(30, service.Entries.Count);
        Assert.Equal("new", service.Entries[0].Input);
        Assert.Equal("28", service.Entries[^1].Input);
    }

    [Fact]
    public void HistoryServiceClearsMemoryEvenWhenPersistenceFails()
    {
        var service = new HistoryService(
            () => [CreateHistoryEntry("hello")],
            _ => false);

        var saved = service.Clear();

        Assert.False(saved);
        Assert.Empty(service.Entries);
    }

    [Fact]
    public async Task TextFileServiceReadsUtf8Text()
    {
        var path = CreateTemporaryPath();
        try
        {
            await File.WriteAllTextAsync(path, "Příliš žluťoučký kůň 🐴");

            var result = await new TextFileService().ReadUtf8Async(path);

            Assert.Equal(TextFileReadStatus.Success, result.Status);
            Assert.Equal("Příliš žluťoučký kůň 🐴", result.Text);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
    }

    [Fact]
    public async Task TextFileServiceRejectsOversizedAndMissingFiles()
    {
        var path = CreateTemporaryPath();
        try
        {
            await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                stream.SetLength(TextFileService.MaximumFileSizeBytes + 1);

            var service = new TextFileService();
            var oversized = await service.ReadUtf8Async(path);
            var missing = await service.ReadUtf8Async(Path.Combine(Path.GetDirectoryName(path)!, "missing.txt"));

            Assert.Equal(TextFileReadStatus.TooLarge, oversized.Status);
            Assert.Null(oversized.Text);
            Assert.Equal(TextFileReadStatus.Failed, missing.Status);
            Assert.Null(missing.Text);
        }
        finally
        {
            DeleteTemporaryDirectory(path);
        }
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
        return Path.Combine(directory, "input.txt");
    }

    private static void DeleteTemporaryDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}
