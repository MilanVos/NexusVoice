using NAudio.CoreAudioApi;

namespace NexusVoice.Services;

public class AudioOutputDevice
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public override string ToString() => Name;
}

public static class AudioDeviceService
{
    public static List<AudioOutputDevice> GetOutputDevices()
        => GetDevices(DataFlow.Render, "Standaard apparaat (uitvoer)");

    public static List<AudioOutputDevice> GetLoopbackDevices()
        => GetDevices(DataFlow.Render, "Standaard apparaat (Discord uitvoer)");

    public static List<AudioOutputDevice> GetInputDevices()
        => GetDevices(DataFlow.Capture, "Standaard microfoon");

    private static List<AudioOutputDevice> GetDevices(DataFlow flow, string defaultLabel)
    {
        var result = new List<AudioOutputDevice>
        {
            new AudioOutputDevice { Id = string.Empty, Name = defaultLabel }
        };

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active))
                result.Add(new AudioOutputDevice { Id = device.ID, Name = device.FriendlyName });
        }
        catch
        {
        }

        return result;
    }
}
