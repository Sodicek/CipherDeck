using System.Text;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Ciphers;

public sealed class VigenereCipher : ICipher
{
    public string Id => CipherIds.Vigenere;
    public string Name => CoreText.Get("VigenereName");
    public string Description => CoreText.Get("VigenereDescription");
    public CipherKeyType KeyType => CipherKeyType.Text;
    public string KeyLabel => CoreText.Get("VigenereKeyLabel");
    public int MinimumNumericKey => 0;
    public int MaximumNumericKey => 0;
    public int DefaultNumericKey => 0;
    public string DefaultTextKey => "KLIC";

    public string Encrypt(string input, CipherKey? key = null) => Encrypt(input, key, CancellationToken.None);
    public string Decrypt(string input, CipherKey? key = null) => Decrypt(input, key, CancellationToken.None);
    public string Encrypt(string input, CipherKey? key, CancellationToken cancellationToken) => Transform(input, GetShifts(key), decrypt: false, cancellationToken);
    public string Decrypt(string input, CipherKey? key, CancellationToken cancellationToken) => Transform(input, GetShifts(key), decrypt: true, cancellationToken);

    private static int[] GetShifts(CipherKey? key)
    {
        var normalizedKey = new string((key?.Text ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray());

        if (normalizedKey.Length == 0)
            throw new ArgumentException(CoreText.Get("VigenereMissingKey"), nameof(key));

        if (normalizedKey.Any(character => !IsAsciiLetter(character)))
            throw new ArgumentException(CoreText.Get("VigenereInvalidKey"), nameof(key));

        return normalizedKey
            .Select(character => char.ToUpperInvariant(character) - 'A')
            .ToArray();
    }

    private static string Transform(string input, IReadOnlyList<int> shifts, bool decrypt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var result = new StringBuilder(input.Length);
        var keyIndex = 0;

        for (var index = 0; index < input.Length; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            var character = input[index];
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
