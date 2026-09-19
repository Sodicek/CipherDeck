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

    public string Encrypt(string input, CipherKey? key = null) => Encrypt(input, key, CancellationToken.None);

    public string Encrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        var step = GetStep(key);
        var elements = TextElementUtility.Split(input, cancellationToken);
        var result = new StringBuilder(input.Length);
        var processed = 0;

        foreach (var index in CreateOrder(elements.Length, step))
        {
            if ((processed++ & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            result.Append(elements[index]);
        }

        return result.ToString();
    }

    public string Decrypt(string input, CipherKey? key = null) => Decrypt(input, key, CancellationToken.None);

    public string Decrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        var step = GetStep(key);
        var elements = TextElementUtility.Split(input, cancellationToken);
        var restored = new string[elements.Length];
        var sourceIndex = 0;

        foreach (var targetIndex in CreateOrder(elements.Length, step))
        {
            if ((sourceIndex & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            restored[targetIndex] = elements[sourceIndex++];
        }

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
