namespace CipherDeck.Core.Challenges;

public sealed record CipherChallenge(
    ChallengeDifficulty Difficulty,
    string PlainText,
    string EncryptedText,
    string CipherId,
    string CipherName,
    CipherKey? Key,
    string Hint)
{
    public bool IsCorrect(string answer) => string.Equals(
        Normalize(answer),
        Normalize(PlainText),
        StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string value) => string.Join(
        ' ',
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
