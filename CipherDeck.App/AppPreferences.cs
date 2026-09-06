using System.Text.Json;

namespace CipherDeck;

internal sealed class AppPreferences
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CipherDeck");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    public bool DarkTheme { get; set; } = true;

    public string? SelectedCipherName { get; set; }

    public static AppPreferences Load()
    {
        try
        {
            return File.Exists(SettingsPath)
                ? JsonSerializer.Deserialize<AppPreferences>(File.ReadAllText(SettingsPath)) ?? new AppPreferences()
                : new AppPreferences();
        }
        catch (IOException)
        {
            return new AppPreferences();
        }
        catch (JsonException)
        {
            return new AppPreferences();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppPreferences();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
