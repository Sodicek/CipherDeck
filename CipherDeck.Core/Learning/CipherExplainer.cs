using System.Text;
using CipherDeck.Core.Ciphers;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Learning;

public static class CipherExplainer
{
    public static CipherExplanation Explain(
        ICipher cipher,
        string input,
        bool encrypt,
        CipherKey? key = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(input);

        var result = encrypt
            ? cipher.Encrypt(input, key, cancellationToken)
            : cipher.Decrypt(input, key, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var steps = cipher switch
        {
            ReverseCipher => ExplainReverse(input, result),
            CaesarCipher => ExplainCaesar(input, result, key, encrypt),
            AtbashCipher => ExplainAtbash(input, result),
            VigenereCipher => ExplainVigenere(input, result, key, encrypt),
            RailFenceCipher => ExplainRailFence(input, result, key, encrypt),
            SkipCipher => ExplainSkip(input, result, key, encrypt),
            _ => [new ExplanationStep(CoreText.Get("ExplanationResultTitle"), cipher.Description, result)]
        };

        return new CipherExplanation(
            cipher.Id,
            cipher.Name,
            CoreText.Get(encrypt ? "ExplanationEncryptSummary" : "ExplanationDecryptSummary"),
            result,
            steps);
    }

    private static IReadOnlyList<ExplanationStep> ExplainReverse(string input, string result)
    {
        var elements = TextElementUtility.Split(input);
        var indexed = string.Join("   ", elements.Take(18).Select((value, index) => $"{index + 1}:{value}"));
        var reversed = string.Join("   ", elements.Reverse().Take(18));

        return
        [
            new(CoreText.Get("ExplanationSplitTitle"), CoreText.Get("ExplanationSplitDetail"), indexed),
            new(CoreText.Get("ExplanationReverseTitle"), CoreText.Get("ExplanationReverseDetail"), reversed),
            new(CoreText.Get("ExplanationComposeTitle"), CoreText.Get("ExplanationReverseComposeDetail"), result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainCaesar(string input, string result, CipherKey? key, bool encrypt)
    {
        var shift = key?.Number ?? 0;
        var signedShift = encrypt ? shift : -shift;
        var changes = CreateCharacterChanges(input, result, (source, target, _) =>
            $"{source} → {target}   (posun {(signedShift >= 0 ? "+" : string.Empty)}{signedShift})");

        return WithResult(
            new ExplanationStep(CoreText.Get("ExplanationShiftTitle"), CoreText.Get("ExplanationShiftDetail"), CoreText.Format("ExplanationShiftSnapshot", signedShift)),
            changes,
            result);
    }

    private static IReadOnlyList<ExplanationStep> ExplainAtbash(string input, string result)
    {
        var changes = CreateCharacterChanges(input, result, (source, target, _) =>
            $"{source} ↔ {target}");

        return WithResult(
            new ExplanationStep(CoreText.Get("ExplanationAtbashTitle"), CoreText.Get("ExplanationAtbashDetail"), "ABCDEFGHIJKLMNOPQRSTUVWXYZ\nZYXWVUTSRQPONMLKJIHGFEDCBA"),
            changes,
            result);
    }

    private static IReadOnlyList<ExplanationStep> ExplainVigenere(string input, string result, CipherKey? key, bool encrypt)
    {
        var normalizedKey = new string((key?.Text ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character))
            .Select(char.ToUpperInvariant)
            .ToArray());
        var details = new List<string>();
        var keyIndex = 0;

        for (var index = 0; index < input.Length && index < result.Length && details.Count < 14; index++)
        {
            if (!IsAsciiLetter(input[index]))
                continue;

            var keyLetter = normalizedKey[keyIndex % normalizedKey.Length];
            var shift = keyLetter - 'A';
            details.Add($"{input[index]}  {(encrypt ? "+" : "−")}  {keyLetter} ({shift})  →  {result[index]}");
            keyIndex++;
        }

        return
        [
            new(CoreText.Get("ExplanationKeyRepeatTitle"), CoreText.Get("ExplanationKeyRepeatDetail"), normalizedKey),
            new(CoreText.Get("ExplanationLetterShiftsTitle"), CoreText.Get(encrypt ? "ExplanationKeyAddDetail" : "ExplanationKeySubtractDetail"), string.Join(Environment.NewLine, details)),
            new(CoreText.Get("ExplanationComposeTitle"), CoreText.Get("ExplanationVigenereComposeDetail"), result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainRailFence(string input, string result, CipherKey? key, bool encrypt)
    {
        var railCount = key?.Number ?? 2;
        var elements = TextElementUtility.Split(encrypt ? input : result);
        var rows = Enumerable.Range(0, railCount).Select(_ => new StringBuilder()).ToArray();
        var rail = 0;
        var direction = 1;

        foreach (var element in elements)
        {
            rows[rail].Append(element).Append(' ');
            if (rail == 0)
                direction = 1;
            else if (rail == railCount - 1)
                direction = -1;
            rail += direction;
        }

        var snapshot = string.Join(Environment.NewLine, rows.Select((row, index) => CoreText.Format("ExplanationRailRow", index + 1, row)));
        return
        [
            new(CoreText.Get("ExplanationRailPatternTitle"), CoreText.Format("ExplanationRailPatternDetail", railCount), snapshot),
            new(CoreText.Get(encrypt ? "ExplanationRailReadTitle" : "ExplanationRailRestoreTitle"), CoreText.Get(encrypt ? "ExplanationRailReadDetail" : "ExplanationRailRestoreDetail"), result),
            new(CoreText.Get("ExplanationResultTitle"), CoreText.Get("ExplanationTranspositionResultDetail"), result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainSkip(string input, string result, CipherKey? key, bool encrypt)
    {
        var step = key?.Number ?? 2;
        var elements = TextElementUtility.Split(encrypt ? input : result);
        var columns = new List<string>();

        for (var offset = 0; offset < step; offset++)
            columns.Add(string.Concat(Enumerable.Range(0, Math.Max(0, (elements.Length - offset + step - 1) / step)).Select(index => elements[offset + index * step])));

        return
        [
            new(CoreText.Get("ExplanationSkipSplitTitle"), CoreText.Format("ExplanationSkipSplitDetail", step), string.Join(Environment.NewLine, columns.Select((column, index) => CoreText.Format("ExplanationSkipColumn", index + 1, column)))),
            new(CoreText.Get(encrypt ? "ExplanationSkipReadTitle" : "ExplanationSkipRestoreTitle"), CoreText.Get(encrypt ? "ExplanationSkipReadDetail" : "ExplanationSkipRestoreDetail"), result),
            new(CoreText.Get("ExplanationResultTitle"), CoreText.Get("ExplanationSkipResultDetail"), result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> CreateCharacterChanges(
        string input,
        string result,
        Func<char, char, int, string> formatter)
    {
        var lines = new List<string>();
        for (var index = 0; index < Math.Min(input.Length, result.Length) && lines.Count < 14; index++)
        {
            if (input[index] != result[index])
                lines.Add(formatter(input[index], result[index], index));
        }

        return
        [
            new ExplanationStep(CoreText.Get("ExplanationChangesTitle"), CoreText.Get("ExplanationChangesDetail"), string.Join(Environment.NewLine, lines))
        ];
    }

    private static IReadOnlyList<ExplanationStep> WithResult(
        ExplanationStep introduction,
        IReadOnlyList<ExplanationStep> changes,
        string result) =>
        [introduction, .. changes, new ExplanationStep(CoreText.Get("ExplanationResultTitle"), CoreText.Get("ExplanationConvertedResultDetail"), result)];

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
