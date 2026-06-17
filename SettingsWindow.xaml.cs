using System.Windows;
using NexusVoice.Services;

namespace NexusVoice;

public partial class SettingsWindow : Window
{
    private readonly Action<AppSettings> _onSaved;

    private record VoiceOption(string Id, string Display);

    private static readonly VoiceOption[] DutchVoices =
    [
        new("nl-NL-MaartenNeural",  "Maarten — Man 🇳🇱"),
        new("nl-NL-ColetteNeural",  "Colette — Vrouw 🇳🇱"),
        new("nl-BE-ArnaudNeural",   "Arnaud — Man 🇧🇪"),
        new("nl-BE-DenaNeural",     "Dena — Vrouw 🇧🇪")
    ];

    private static readonly VoiceOption[] GermanVoices =
    [
        new("de-DE-ConradNeural",              "Conrad — Man 🇩🇪"),
        new("de-DE-BerndNeural",               "Bernd — Man 🇩🇪"),
        new("de-DE-FlorianMultilingualNeural", "Florian — Man 🇩🇪"),
        new("de-AT-JonasNeural",               "Jonas — Man 🇦🇹"),
        new("de-DE-KatjaNeural",               "Katja — Vrouw 🇩🇪"),
        new("de-DE-AmalaNeural",               "Amala — Vrouw 🇩🇪"),
        new("de-AT-IngridNeural",              "Ingrid — Vrouw 🇦🇹"),
        new("de-CH-LeniNeural",                "Leni — Vrouw 🇨🇭")
    ];

    private SettingsWindow(Window owner, AppSettings current, Action<AppSettings> onSaved)
    {
        InitializeComponent();
        Owner = owner;
        _onSaved = onSaved;

        KeyBox.Text = current.SubscriptionKey;
        RegionBox.Text = current.Region;

        var inputDevices = AudioDeviceService.GetInputDevices();
        InputDeviceBox.ItemsSource = inputDevices;
        InputDeviceBox.SelectedItem = inputDevices.FirstOrDefault(d => d.Id == current.InputDeviceId)
                                      ?? inputDevices[0];

        var outputDevices = AudioDeviceService.GetOutputDevices();
        OutputDeviceBox.ItemsSource = outputDevices;
        OutputDeviceBox.SelectedItem = outputDevices.FirstOrDefault(d => d.Id == current.OutputDeviceId)
                                       ?? outputDevices[0];

        var loopbackDevices = AudioDeviceService.GetLoopbackDevices();
        LoopbackDeviceBox.ItemsSource = loopbackDevices;
        LoopbackDeviceBox.SelectedItem = loopbackDevices.FirstOrDefault(d => d.Id == current.LoopbackDeviceId)
                                         ?? loopbackDevices[0];

        DutchVoiceBox.ItemsSource = DutchVoices;
        DutchVoiceBox.DisplayMemberPath = "Display";
        DutchVoiceBox.SelectedItem = DutchVoices.FirstOrDefault(v => v.Id == current.DutchVoice) ?? DutchVoices[0];

        GermanVoiceBox.ItemsSource = GermanVoices;
        GermanVoiceBox.DisplayMemberPath = "Display";
        GermanVoiceBox.SelectedItem = GermanVoices.FirstOrDefault(v => v.Id == current.GermanVoice) ?? GermanVoices[0];

    }

    public static void ShowDialog(Window owner, AppSettings current, Action<AppSettings> onSaved)
        => new SettingsWindow(owner, current, onSaved).ShowDialog();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var key = KeyBox.Text.Trim();
        var region = RegionBox.Text.Trim();

        if (string.IsNullOrEmpty(key))
        {
            MessageBox.Show("Voer een geldige API-sleutel in.", "Validatiefout",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            KeyBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(region))
        {
            MessageBox.Show("Voer een geldige regio in.", "Validatiefout",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            RegionBox.Focus();
            return;
        }

        var settings = new AppSettings
        {
            SubscriptionKey = key,
            Region = region,
            InputDeviceId = (InputDeviceBox.SelectedItem as AudioOutputDevice)?.Id ?? string.Empty,
            OutputDeviceId = (OutputDeviceBox.SelectedItem as AudioOutputDevice)?.Id ?? string.Empty,
            LoopbackDeviceId = (LoopbackDeviceBox.SelectedItem as AudioOutputDevice)?.Id ?? string.Empty,
            DutchVoice = (DutchVoiceBox.SelectedItem as VoiceOption)?.Id ?? DutchVoices[0].Id,
            GermanVoice = (GermanVoiceBox.SelectedItem as VoiceOption)?.Id ?? GermanVoices[0].Id
        };

        settings.Save();
        _onSaved(settings);
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
