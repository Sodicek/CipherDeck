using System.Globalization;
using System.Resources;
using System.Collections;

namespace CipherDeck.Core.Localization;

internal static class CoreText
{
    private static readonly ResourceManager Resources = new(
        "CipherDeck.Core.Resources.CoreStrings",
        typeof(CoreText).Assembly);

    public static string Get(string key) => Get(key, CultureInfo.CurrentUICulture);

    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentUICulture, Get(key), arguments);

    public static string[] GetList(string key) =>
        Get(key).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal static string Get(string key, CultureInfo culture) =>
        Resources.GetString(key, culture)
        ?? throw new InvalidOperationException($"Missing localized Core string: {key} ({culture.Name}).");

    internal static IReadOnlySet<string> GetKeys(CultureInfo culture) =>
        Resources.GetResourceSet(culture, true, false)?
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .ToHashSet(StringComparer.Ordinal)
        ?? throw new InvalidOperationException($"Missing Core resource set: {culture.Name}.");
}
