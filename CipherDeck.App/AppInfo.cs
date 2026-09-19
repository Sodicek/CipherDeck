using System.Reflection;

namespace CipherDeck;

internal static class AppInfo
{
    private static readonly Version Version = typeof(AppInfo).Assembly.GetName().Version ?? new Version(0, 0);

    public static string DisplayVersion => Version.Build == 0
        ? $"v{Version.Major}.{Version.Minor}"
        : $"v{Version.Major}.{Version.Minor}.{Version.Build}";
}
