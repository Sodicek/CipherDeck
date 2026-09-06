using System.Globalization;

namespace CipherDeck.Core.Ciphers;

internal static class TextElementUtility
{
    public static string[] Split(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var elements = new List<string>();
        var enumerator = StringInfo.GetTextElementEnumerator(input);

        while (enumerator.MoveNext())
            elements.Add(enumerator.GetTextElement());

        return [.. elements];
    }
}
