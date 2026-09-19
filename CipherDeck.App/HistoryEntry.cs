using CipherDeck.Core;

namespace CipherDeck;

internal sealed record HistoryEntry(
    DateTime Timestamp,
    string CipherName,
    bool IsEncryption,
    string Input,
    string Output,
    CipherKey? Key,
    string? CipherId = null)
{
    public string DisplayText => AppText.Format(
        "HistoryDisplay",
        Timestamp,
        HistoryStore.ResolveCipher(this)?.Name ?? CipherName,
        AppText.Get(IsEncryption ? "HistoryEncryption" : "HistoryDecryption"));
}
