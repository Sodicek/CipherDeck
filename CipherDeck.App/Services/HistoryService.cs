using System.Collections.ObjectModel;
using CipherDeck.Core;

namespace CipherDeck;

internal sealed class HistoryService
{
    private const int MaximumEntries = 30;
    private readonly List<HistoryEntry> _entries = [];
    private readonly ReadOnlyCollection<HistoryEntry> _readOnlyEntries;
    private readonly Func<IEnumerable<HistoryEntry>, bool> _save;

    public HistoryService()
        : this(HistoryStore.Load, HistoryStore.Save)
    {
    }

    internal HistoryService(
        Func<IReadOnlyList<HistoryEntry>> load,
        Func<IEnumerable<HistoryEntry>, bool> save)
    {
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(save);

        _save = save;
        _entries.AddRange(HistoryStore.Normalize(load()).Take(MaximumEntries));
        _readOnlyEntries = _entries.AsReadOnly();
    }

    public IReadOnlyList<HistoryEntry> Entries => _readOnlyEntries;

    public HistoryAddResult Add(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (!HistoryStore.CanStore(entry))
            return HistoryAddResult.EntryTooLarge;

        var normalizedEntry = entry with { CipherId = HistoryStore.ResolveCipher(entry)!.Id };
        _entries.Insert(0, normalizedEntry);
        if (_entries.Count > MaximumEntries)
            _entries.RemoveAt(_entries.Count - 1);

        return _save(_entries)
            ? HistoryAddResult.Saved
            : HistoryAddResult.SaveFailed;
    }

    public bool Clear()
    {
        _entries.Clear();
        return _save(_entries);
    }

    public ICipher? ResolveCipher(HistoryEntry entry) => HistoryStore.ResolveCipher(entry);
}

internal enum HistoryAddResult
{
    Saved,
    EntryTooLarge,
    SaveFailed
}
