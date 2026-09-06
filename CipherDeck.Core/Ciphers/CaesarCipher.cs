namespace CipherDeck.Core.Ciphers;

public sealed class CaesarCipher : ICipher
{
    public string Name => "Caesarova šifra";
    public string Description => "Posune písmena A–Z o zvolený počet míst. Ostatní znaky ponechá beze změny.";
    public CipherKeyType KeyType => CipherKeyType.Number;
    public string KeyLabel => "POSUN";
    public int MinimumNumericKey => 1;
    public int MaximumNumericKey => 25;
    public int DefaultNumericKey => 3;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null) => Transform(input, GetKey(key));
    public string Decrypt(string input, CipherKey? key = null) => Transform(input, -GetKey(key));

    private static int GetKey(CipherKey? key)
    {
        if (key?.Number is null)
        {
            throw new ArgumentException("Caesarova šifra vyžaduje číselný posun.", nameof(key));
        }

        return ((key.Number.Value % 26) + 26) % 26;
    }

    private static string Transform(string input, int shift)
    {
        ArgumentNullException.ThrowIfNull(input);
        return string.Create(input.Length, (input, shift), static (output, state) =>
        {
            for (var index = 0; index < state.input.Length; index++)
            {
                output[index] = ShiftCharacter(state.input[index], state.shift);
            }
        });
    }

    private static char ShiftCharacter(char character, int shift)
    {
        if (character is >= 'A' and <= 'Z')
            return (char)('A' + ((character - 'A' + shift) % 26 + 26) % 26);
        if (character is >= 'a' and <= 'z')
            return (char)('a' + ((character - 'a' + shift) % 26 + 26) % 26);
        return character;
    }
}
