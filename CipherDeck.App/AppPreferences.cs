namespace CipherDeck;

internal sealed class AppPreferences
{
    private const int MaximumSettingsFileSizeBytes = 64 * 1024;

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CipherDeck");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    public bool DarkTheme { get; set; } = true;

    public string? SelectedCipherName { get; set; }

    public static AppPreferences Load()
    {
        return JsonFileStore.Load<AppPreferences>(SettingsPath, MaximumSettingsFileSizeBytes)
            ?? new AppPreferences();
    }

    public bool Save() => JsonFileStore.Save(SettingsPath, this, MaximumSettingsFileSizeBytes);
}
