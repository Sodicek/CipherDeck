using System.Text;

namespace CipherDeck.Core.Analysis;

public static class FrequencyAnalyzer
{
    public static IReadOnlyList<LetterFrequency> AnalyzeLetters(string input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var counts = new Dictionary<Rune, int>();
        var total = 0;

        var processed = 0;
        foreach (var rune in input.EnumerateRunes())
        {
            if ((processed++ & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            if (!Rune.IsLetter(rune))
                continue;

            var normalized = Rune.ToUpperInvariant(rune);
            counts[normalized] = counts.GetValueOrDefault(normalized) + 1;
            total++;
        }

        if (total == 0)
            return [];

        return counts
            .Select(pair => new LetterFrequency(
                pair.Key.ToString(),
                pair.Value,
                pair.Value * 100d / total))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Symbol, StringComparer.CurrentCulture)
            .ToList();
    }
}
