using System.Globalization;

namespace CipherDeck.Core.Ciphers;

internal static class TextElementUtility
{
    public static string[] Split(string input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var elements = new List<string>();
        var enumerator = StringInfo.GetTextElementEnumerator(input);

        while (enumerator.MoveNext())
        {
            if ((elements.Count & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            elements.Add(enumerator.GetTextElement());
        }

        return [.. elements];
    }
}
