using System.Collections;
using System.Globalization;
using System.Resources;

namespace CipherDeck;

internal static class AppText
{
    private static readonly ResourceManager Resources = new(
        "CipherDeck.Resources.AppStrings",
        typeof(AppText).Assembly);

    public static string Get(string key) => Get(key, CultureInfo.CurrentUICulture);

    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentUICulture, Get(key), arguments);

    internal static string Get(string key, CultureInfo culture) =>
        Resources.GetString(key, culture)
        ?? throw new InvalidOperationException($"Missing localized app string: {key} ({culture.Name}).");

    internal static IReadOnlySet<string> GetKeys(CultureInfo culture) =>
        Resources.GetResourceSet(culture, true, false)?
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .ToHashSet(StringComparer.Ordinal)
        ?? throw new InvalidOperationException($"Missing app resource set: {culture.Name}.");
}
