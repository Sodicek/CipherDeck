using System.Globalization;

namespace CipherDeck;

internal static class AppLanguage
{
    public const string Czech = "cs";
    public const string English = "en";

    public static string Normalize(string? languageCode) =>
        string.Equals(languageCode, English, StringComparison.OrdinalIgnoreCase) ? English : Czech;

    public static void Apply(string? languageCode)
    {
        var culture = CultureInfo.GetCultureInfo(Normalize(languageCode));
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
