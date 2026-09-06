using System.Text;

namespace CipherDeck.Core.Ciphers;

public sealed class RailFenceCipher : ICipher
{
    public string Name => "Rail Fence";
    public string Description => "Zapíše text cikcak do několika řádků („hradeb“) a potom jej přečte po řádcích.";
    public CipherKeyType KeyType => CipherKeyType.Number;
    public string KeyLabel => "ŘÁDKY";
    public int MinimumNumericKey => 2;
    public int MaximumNumericKey => 20;
    public int DefaultNumericKey => 3;
    public string DefaultTextKey => string.Empty;

    public string Encrypt(string input, CipherKey? key = null)
    {
        var rails = GetRailCount(key);
        var elements = TextElementUtility.Split(input);
        var rows = Enumerable.Range(0, rails).Select(_ => new StringBuilder()).ToArray();
        var pattern = CreatePattern(elements.Length, rails);

        for (var index = 0; index < elements.Length; index++)
            rows[pattern[index]].Append(elements[index]);

        return string.Concat(rows.Select(row => row.ToString()));
    }

    public string Decrypt(string input, CipherKey? key = null)
    {
        var rails = GetRailCount(key);
        var elements = TextElementUtility.Split(input);
        var pattern = CreatePattern(elements.Length, rails);
        var rows = Enumerable.Range(0, rails).Select(_ => new Queue<string>()).ToArray();
        var sourceIndex = 0;

        for (var rail = 0; rail < rails; rail++)
        {
            var count = pattern.Count(value => value == rail);
            for (var index = 0; index < count; index++)
                rows[rail].Enqueue(elements[sourceIndex++]);
        }

        var result = new StringBuilder(input.Length);
        foreach (var rail in pattern)
            result.Append(rows[rail].Dequeue());

        return result.ToString();
    }

    private static int GetRailCount(CipherKey? key)
    {
        if (key?.Number is not { } rails || rails is < 2 or > 20)
            throw new ArgumentException("Počet řádků musí být mezi 2 a 20.", nameof(key));
        return rails;
    }

    private static int[] CreatePattern(int length, int rails)
    {
        var pattern = new int[length];
        var rail = 0;
        var direction = 1;

        for (var index = 0; index < length; index++)
        {
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
