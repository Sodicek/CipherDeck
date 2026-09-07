namespace CipherDeck.Core.Learning;

public sealed record CipherExplanation(
    string CipherName,
    string Summary,
    string Result,
    IReadOnlyList<ExplanationStep> Steps);
