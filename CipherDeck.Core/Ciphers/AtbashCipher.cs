using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Ciphers;

public sealed class AtbashCipher : ICipher
{
    public string Id => CipherIds.Atbash;
    public string Name => CoreText.Get("AtbashName");
    public string Description => CoreText.Get("AtbashDescription");
    public CipherKeyType KeyType => CipherKeyType.None;
    public string KeyLabel => string.Empty;
    public int MinimumNumericKey => 0;
    public int MaximumNumericKey => 0;
    public int DefaultNumericKey => 0;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null) => Encrypt(input, key, CancellationToken.None);
    public string Decrypt(string input, CipherKey? key = null) => Decrypt(input, key, CancellationToken.None);
    public string Encrypt(string input, CipherKey? key, CancellationToken cancellationToken) => Transform(input, cancellationToken);
    public string Decrypt(string input, CipherKey? key, CancellationToken cancellationToken) => Transform(input, cancellationToken);

    private static string Transform(string input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return string.Create(input.Length, (input, cancellationToken), static (output, state) =>
        {
            for (var index = 0; index < state.input.Length; index++)
            {
                if ((index & 1023) == 0)
                    state.cancellationToken.ThrowIfCancellationRequested();
                output[index] = MirrorCharacter(state.input[index]);
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
