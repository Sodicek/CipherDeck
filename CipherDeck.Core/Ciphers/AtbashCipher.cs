namespace CipherDeck.Core.Ciphers;

public sealed class AtbashCipher : ICipher
{
    public string Name => "Atbash";
    public string Description => "Nahradí A za Z, B za Y a tak dále. Stejná operace text zašifruje i odšifruje.";
    public CipherKeyType KeyType => CipherKeyType.None;
    public string KeyLabel => string.Empty;
    public int MinimumNumericKey => 0;
    public int MaximumNumericKey => 0;
    public int DefaultNumericKey => 0;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null) => Transform(input);
    public string Decrypt(string input, CipherKey? key = null) => Transform(input);

    private static string Transform(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return string.Create(input.Length, input, static (output, source) =>
        {
            for (var index = 0; index < source.Length; index++)
            {
                output[index] = MirrorCharacter(source[index]);
            }
        });
    }

    private static char MirrorCharacter(char character)
    {
        if (character is >= 'A' and <= 'Z')
            return (char)('Z' - (character - 'A'));
        if (character is >= 'a' and <= 'z')
            return (char)('z' - (character - 'a'));
        return character;
    }
}
