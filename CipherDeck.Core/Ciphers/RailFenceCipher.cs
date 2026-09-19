using System.Text;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Ciphers;

public sealed class RailFenceCipher : ICipher
{
    public string Id => CipherIds.RailFence;
    public string Name => CoreText.Get("RailFenceName");
    public string Description => CoreText.Get("RailFenceDescription");
    public CipherKeyType KeyType => CipherKeyType.Number;
    public string KeyLabel => CoreText.Get("RailFenceKeyLabel");
    public int MinimumNumericKey => 2;
    public int MaximumNumericKey => 20;
    public int DefaultNumericKey => 3;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null) => Encrypt(input, key, CancellationToken.None);

    public string Encrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        var rails = GetRailCount(key);
        var elements = TextElementUtility.Split(input, cancellationToken);
        var rows = Enumerable.Range(0, rails).Select(_ => new StringBuilder()).ToArray();
        var pattern = CreatePattern(elements.Length, rails, cancellationToken);

        for (var index = 0; index < elements.Length; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            rows[pattern[index]].Append(elements[index]);
        }

        return string.Concat(rows.Select(row => row.ToString()));
    }

    public string Decrypt(string input, CipherKey? key = null) => Decrypt(input, key, CancellationToken.None);

    public string Decrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        var rails = GetRailCount(key);
        var elements = TextElementUtility.Split(input, cancellationToken);
        var pattern = CreatePattern(elements.Length, rails, cancellationToken);
        var rows = Enumerable.Range(0, rails).Select(_ => new Queue<string>()).ToArray();
        var counts = new int[rails];
        var sourceIndex = 0;

        for (var index = 0; index < pattern.Length; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            counts[pattern[index]]++;
        }

        for (var rail = 0; rail < rails; rail++)
        {
            for (var index = 0; index < counts[rail]; index++)
                rows[rail].Enqueue(elements[sourceIndex++]);
        }

        var result = new StringBuilder(input.Length);
        for (var index = 0; index < pattern.Length; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            result.Append(rows[pattern[index]].Dequeue());
        }

        return result.ToString();
    }

    private static int GetRailCount(CipherKey? key)
    {
        if (key?.Number is not { } rails || rails is < 2 or > 20)
            throw new ArgumentException(CoreText.Get("RailFenceInvalidKey"), nameof(key));
        return rails;
    }

    private static int[] CreatePattern(int length, int rails, CancellationToken cancellationToken)
    {
        var pattern = new int[length];
        var rail = 0;
        var direction = 1;

        for (var index = 0; index < length; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            pattern[index] = rail;
            if (rail == 0)
                direction = 1;
            else if (rail == rails - 1)
                direction = -1;
            rail += direction;
        }

        return pattern;
    }
}
