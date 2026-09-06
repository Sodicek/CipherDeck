using System.Text;

namespace CipherDeck.Core.Ciphers;

public sealed class VigenereCipher : ICipher
{
    public string Name => "Vigenèrova šifra";
    public string Description => "Posouvá písmena podle opakujícího se textového klíče. Používá latinská písmena A–Z.";
    public CipherKeyType KeyType => CipherKeyType.Text;
    public string KeyLabel => "KLÍČ";
    public int MinimumNumericKey => 0;
    public int MaximumNumericKey => 0;
    public int DefaultNumericKey => 0;
    public string DefaultTextKey => "KLIC";

    public string Encrypt(string input, CipherKey? key = null) => Transform(input, GetShifts(key), decrypt: false);
    public string Decrypt(string input, CipherKey? key = null) => Transform(input, GetShifts(key), decrypt: true);

    private static int[] GetShifts(CipherKey? key)
    {
        var normalizedKey = new string((key?.Text ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray());

        if (normalizedKey.Length == 0)
            throw new ArgumentException("Vigenèrova šifra vyžaduje textový klíč.", nameof(key));

        if (normalizedKey.Any(character => !IsAsciiLetter(character)))
            throw new ArgumentException("Klíč může obsahovat pouze písmena A–Z a mezery.", nameof(key));

        return normalizedKey
            .Select(character => char.ToUpperInvariant(character) - 'A')
            .ToArray();
    }

    private static string Transform(string input, IReadOnlyList<int> shifts, bool decrypt)
    {
        ArgumentNullException.ThrowIfNull(input);
        var result = new StringBuilder(input.Length);
        var keyIndex = 0;

        foreach (var character in input)
        {
            if (!IsAsciiLetter(character))
            {
                result.Append(character);
                continue;
            }

            var shift = shifts[keyIndex % shifts.Count] * (decrypt ? -1 : 1);
            var alphabetStart = char.IsUpper(character) ? 'A' : 'a';
            result.Append((char)(alphabetStart + ((character - alphabetStart + shift) % 26 + 26) % 26));
            keyIndex++;
        }

        return result.ToString();
    }

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
