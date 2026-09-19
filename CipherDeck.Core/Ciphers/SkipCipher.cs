using System.Text;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Ciphers;

public sealed class SkipCipher : ICipher
{
    public string Id => CipherIds.Skip;
    public string Name => CoreText.Get("SkipName");
    public string Description => CoreText.Get("SkipDescription");
    public CipherKeyType KeyType => CipherKeyType.Number;
    public string KeyLabel => CoreText.Get("SkipKeyLabel");
    public int MinimumNumericKey => 2;
    public int MaximumNumericKey => 20;
    public int DefaultNumericKey => 3;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null)
    {
        var step = GetStep(key);
        var elements = TextElementUtility.Split(input);
        var result = new StringBuilder(input.Length);

        foreach (var index in CreateOrder(elements.Length, step))
            result.Append(elements[index]);

        return result.ToString();
    }

    public string Decrypt(string input, CipherKey? key = null)
    {
        var step = GetStep(key);
        var elements = TextElementUtility.Split(input);
        var restored = new string[elements.Length];
        var sourceIndex = 0;

        foreach (var targetIndex in CreateOrder(elements.Length, step))
            restored[targetIndex] = elements[sourceIndex++];

        return string.Concat(restored);
    }

    private static int GetStep(CipherKey? key)
    {
        if (key?.Number is not { } step || step is < 2 or > 20)
            throw new ArgumentException(CoreText.Get("SkipInvalidKey"), nameof(key));
        return step;
    }

    private static IEnumerable<int> CreateOrder(int length, int step)
    {
        for (var offset = 0; offset < step; offset++)
        {
            for (var index = offset; index < length; index += step)
                yield return index;
        }
    }
}
