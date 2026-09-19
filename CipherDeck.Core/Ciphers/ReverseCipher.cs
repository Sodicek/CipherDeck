using System.Text;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Ciphers;

public sealed class ReverseCipher : ICipher
{
    public string Id => CipherIds.Reverse;
    public string Name => CoreText.Get("ReverseName");
    public string Description => CoreText.Get("ReverseDescription");
    public CipherKeyType KeyType => CipherKeyType.None;
    public string KeyLabel => string.Empty;
    public int MinimumNumericKey => 0;
    public int MaximumNumericKey => 0;
    public int DefaultNumericKey => 0;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null) => Reverse(input);
    public string Decrypt(string input, CipherKey? key = null) => Reverse(input);

    private static string Reverse(string input)
    {
        var elements = TextElementUtility.Split(input);

        var result = new StringBuilder(input.Length);
        for (var index = elements.Length - 1; index >= 0; index--)
        {
            result.Append(elements[index]);
        }

        return result.ToString();
    }
}
