using System.IO;
using Newtonsoft.Json;

namespace NexusVoice.Services;

public class AppSettings
{
    public string SubscriptionKey { get; set; } = string.Empty;
    public string Region { get; set; } = "westeurope";
    public string DutchVoice { get; set; } = "nl-NL-MaartenNeural";
    public string GermanVoice { get; set; } = "de-DE-ConradNeural";
    public string InputDeviceId { get; set; } = string.Empty;
    public string OutputDeviceId { get; set; } = string.Empty;
    public string LoopbackDeviceId { get; set; } = string.Empty;


    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NexusVoice",
        "settings.json");

    public static AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new AppSettings();
        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SubscriptionKey) && !string.IsNullOrWhiteSpace(Region);
}
