using System.Globalization;
using System.Resources;

namespace CipherDeck.Core.Localization;

internal static class CoreText
{
    private static readonly ResourceManager Resources = new(
        "CipherDeck.Core.Resources.CoreStrings",
        typeof(CoreText).Assembly);

    public static string Get(string key) => Get(key, CultureInfo.CurrentUICulture);

    internal static string Get(string key, CultureInfo culture) =>
        Resources.GetString(key, culture)
        ?? throw new InvalidOperationException($"Missing localized Core string: {key} ({culture.Name}).");
}
