namespace CipherDeck.Core.Detection;

public sealed record CipherDetection(
    string? CipherId,
    string CipherName,
    double Confidence,
    string SuggestedPlainText,
    string Reason,
    CipherKey? SuggestedKey);
