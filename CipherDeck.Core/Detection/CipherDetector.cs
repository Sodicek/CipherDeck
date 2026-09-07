using CipherDeck.Core.Ciphers;

namespace CipherDeck.Core.Detection;

public static class CipherDetector
{
    private static readonly string[] CommonWords =
    [
        " ahoj ", " svet ", " zprava ", " sifra ", " klic ", " tajny ", " text ",
        " the ", " hello ", " world ", " secret ", " cipher ", " message ", " key "
    ];

    private static readonly string[] CommonFragments =
    [
        "ER", "EN", "ST", "CH", "PR", "NI", "OV", "OU", "JE", "SE", "NA", "TO",
        "TH", "HE", "IN", "AN", "RE", "ON", "AT", "ND", "ING"
    ];

    public static IReadOnlyList<CipherDetection> Detect(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Any(char.IsLetter))
            return [];

        var candidates = new List<Candidate>();
        var reverse = new ReverseCipher().Decrypt(input);
        candidates.Add(new Candidate("Pozpátku", reverse, ScoreLanguage(reverse), "Text přečtený odzadu připomíná přirozený jazyk.", null));

        var atbash = new AtbashCipher().Decrypt(input);
        candidates.Add(new Candidate("Atbash", atbash, ScoreLanguage(atbash), "Zrcadlová abeceda vytváří jazykově pravděpodobný výsledek.", null));

        var caesar = new CaesarCipher();
        for (var shift = 1; shift <= 25; shift++)
        {
            var key = new CipherKey(Number: shift);
            var decoded = caesar.Decrypt(input, key);
            candidates.Add(new Candidate(
                $"Caesarova šifra · posun {shift}",
                decoded,
                ScoreLanguage(decoded),
                $"Nejlépe působí posun abecedy o {shift} míst.",
                key));
        }

        candidates.Add(new Candidate(
            "Transpoziční nebo nezašifrovaný text",
            input,
            ScoreLanguage(input) * 0.85,
            "Četnost písmen zůstává zachovaná; změněné může být pouze jejich pořadí.",
            null));

        var best = candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Name, StringComparer.CurrentCulture)
            .Take(3)
            .ToList();
        var totalWeight = best.Sum(candidate => Math.Max(1, candidate.Score));

        return best.Select(candidate => new CipherDetection(
                candidate.Name,
                Math.Max(1, candidate.Score) / totalWeight,
                candidate.PlainText,
                candidate.Reason,
                candidate.Key))
            .ToList();
    }

    private static double ScoreLanguage(string text)
    {
        var normalized = $" {RemoveDiacritics(text).ToLowerInvariant()} ";
        var upper = normalized.ToUpperInvariant();
        var score = CommonWords.Sum(word => CountOccurrences(normalized, word) * 14d);
        score += CommonFragments.Sum(fragment => CountOccurrences(upper, fragment) * 1.5d);

        var letters = upper.Count(character => character is >= 'A' and <= 'Z');
        var vowels = upper.Count(character => "AEIOUY".Contains(character));
        if (letters > 0)
        {
            var ratio = vowels / (double)letters;
            if (ratio is >= 0.28 and <= 0.52)
                score += 4;
        }

        return score;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        return new string(normalized
            .Where(character => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray())
            .Normalize(System.Text.NormalizationForm.FormC);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private sealed record Candidate(string Name, string PlainText, double Score, string Reason, CipherKey? Key);
}
