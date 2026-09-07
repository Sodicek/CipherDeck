using System.Text;
using CipherDeck.Core.Ciphers;

namespace CipherDeck.Core.Learning;

public static class CipherExplainer
{
    public static CipherExplanation Explain(ICipher cipher, string input, bool encrypt, CipherKey? key = null)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(input);

        var result = encrypt ? cipher.Encrypt(input, key) : cipher.Decrypt(input, key);
        var steps = cipher switch
        {
            ReverseCipher => ExplainReverse(input, result),
            CaesarCipher => ExplainCaesar(input, result, key, encrypt),
            AtbashCipher => ExplainAtbash(input, result),
            VigenereCipher => ExplainVigenere(input, result, key, encrypt),
            RailFenceCipher => ExplainRailFence(input, result, key, encrypt),
            SkipCipher => ExplainSkip(input, result, key, encrypt),
            _ => [new ExplanationStep("Výsledek", cipher.Description, result)]
        };

        return new CipherExplanation(
            cipher.Name,
            encrypt ? "Jak se vstupní text zašifroval" : "Jak se vstupní text odšifroval",
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
            new("Rozdělení na znaky", "Text se rozdělí na uživatelské znaky. Emoji a kombinovaná diakritika zůstávají pohromadě.", indexed),
            new("Obrácení pořadí", "Znaky se přečtou od posledního k prvnímu.", reversed),
            new("Složení výsledku", "Obrácené znaky se znovu spojí do jednoho textu.", result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainCaesar(string input, string result, CipherKey? key, bool encrypt)
    {
        var shift = key?.Number ?? 0;
        var signedShift = encrypt ? shift : -shift;
        var changes = CreateCharacterChanges(input, result, (source, target, _) =>
            $"{source} → {target}   (posun {(signedShift >= 0 ? "+" : string.Empty)}{signedShift})");

        return WithResult(
            new ExplanationStep("Nastavení posunu", "Abeceda A–Z se chápe jako kruh; po Z následuje znovu A.", $"Posun: {signedShift}"),
            changes,
            result);
    }

    private static IReadOnlyList<ExplanationStep> ExplainAtbash(string input, string result)
    {
        var changes = CreateCharacterChanges(input, result, (source, target, _) =>
            $"{source} ↔ {target}");

        return WithResult(
            new ExplanationStep("Zrcadlová abeceda", "První písmeno se páruje s posledním: A↔Z, B↔Y, C↔X…", "ABCDEFGHIJKLMNOPQRSTUVWXYZ\nZYXWVUTSRQPONMLKJIHGFEDCBA"),
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
            new("Opakování klíče", "Klíč se opakuje pouze nad písmeny; mezery a interpunkce jej neposouvají.", normalizedKey),
            new("Posuny písmen", encrypt ? "Hodnota písmene klíče se přičte." : "Hodnota písmene klíče se odečte.", string.Join(Environment.NewLine, details)),
            new("Složení výsledku", "Změněná písmena a zachované ostatní znaky se spojí.", result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainRailFence(string input, string result, CipherKey? key, bool encrypt)
    {
        var railCount = key?.Number ?? 2;
        var elements = TextElementUtility.Split(input);
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

        var snapshot = string.Join(Environment.NewLine, rows.Select((row, index) => $"Řádek {index + 1}: {row}"));
        return
        [
            new("Cikcak vzor", $"Text se rozloží do {railCount} řádků pohybem dolů a nahoru.", snapshot),
            new(encrypt ? "Čtení po řádcích" : "Obnovení cikcak pořadí", encrypt ? "Jednotlivé řádky se spojí shora dolů." : "Znaky se vrátí na pozice podle cikcak vzoru.", result),
            new("Výsledek", "Transpozice mění pořadí, ale žádný znak nepřidává ani neodebírá.", result)
        ];
    }

    private static IReadOnlyList<ExplanationStep> ExplainSkip(string input, string result, CipherKey? key, bool encrypt)
    {
        var step = key?.Number ?? 2;
        var elements = TextElementUtility.Split(input);
        var columns = new List<string>();

        for (var offset = 0; offset < step; offset++)
            columns.Add(string.Concat(Enumerable.Range(0, Math.Max(0, (elements.Length - offset + step - 1) / step)).Select(index => elements[offset + index * step])));

        return
        [
            new("Rozdělení podle kroku", $"Použije se krok {step}. Znaky se rozdělí podle zbytku jejich pozice.", string.Join(Environment.NewLine, columns.Select((column, index) => $"Sloupec {index + 1}: {column}"))),
            new(encrypt ? "Čtení sloupců" : "Vrácení na původní pozice", encrypt ? "Sloupce se přečtou postupně zleva doprava." : "Znaky se ze sloupců rozloží zpět na původní pozice.", result),
            new("Výsledek", "Každý vstupní znak se ve výsledku objeví právě jednou.", result)
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
            new ExplanationStep("Převod písmen", "Ukázka změněných písmen; mezery, čísla a interpunkce zůstávají beze změny.", string.Join(Environment.NewLine, lines))
        ];
    }

    private static IReadOnlyList<ExplanationStep> WithResult(
        ExplanationStep introduction,
        IReadOnlyList<ExplanationStep> changes,
        string result) =>
        [introduction, .. changes, new ExplanationStep("Výsledek", "Převedené znaky se spojí do výstupního textu.", result)];

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
