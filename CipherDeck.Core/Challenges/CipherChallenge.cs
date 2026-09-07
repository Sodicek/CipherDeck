namespace CipherDeck.Core.Challenges;

public sealed record CipherChallenge(
    ChallengeDifficulty Difficulty,
    string PlainText,
    string EncryptedText,
    string CipherName,
    CipherKey? Key,
    string Hint)
{
    public bool IsCorrect(string answer) => string.Equals(
        Normalize(answer),
        Normalize(PlainText),
        StringComparison.CurrentCultureIgnoreCase);

    private static string Normalize(string value) => string.Join(
        ' ',
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
