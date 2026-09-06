using CipherDeck.Core;

namespace CipherDeck;

internal sealed record HistoryEntry(
    DateTime Timestamp,
    string CipherName,
    bool IsEncryption,
    string Input,
    string Output,
    CipherKey? Key)
{
    public string DisplayText => $"{Timestamp:HH:mm:ss}  ·  {CipherName}  ·  {(IsEncryption ? "šifrování" : "odšifrování")}";
}
