using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NexusVoice.Services;

public class SessionCallbacks
{
    public Action<string>? OnNLInterim { get; set; }
    public Action<string, string>? OnNLFinal { get; set; }
    public Action<string>? OnDEInterim { get; set; }
    public Action<string, string>? OnDEFinal { get; set; }
    public Action<string>? OnError { get; set; }
}

public class SpeechTranslationService
{
    private readonly AppSettings _settings;

    public SpeechTranslationService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task RunSessionAsync(SessionCallbacks callbacks, CancellationToken token)
    {
        var t1 = RunDirectionAsync(
            fromLocale: "nl-NL",
            toLangCode: "de",
            ttsVoice: _settings.GermanVoice,
            audioConfig: CreateMicAudioConfig(),
            onInterim: callbacks.OnNLInterim,
            onFinal: callbacks.OnNLFinal,
            onError: callbacks.OnError,
            token: token);

        var t2 = RunLoopbackDirectionAsync(
            fromLocale: "de-DE",
            toLangCode: "nl",
            onInterim: callbacks.OnDEInterim,
            onFinal: callbacks.OnDEFinal,
            onError: callbacks.OnError,
            token: token);

        await Task.WhenAll(t1, t2);
    }

    private AudioConfig CreateMicAudioConfig()
    {
        return string.IsNullOrEmpty(_settings.InputDeviceId)
            ? AudioConfig.FromDefaultMicrophoneInput()
            : AudioConfig.FromMicrophoneInput(_settings.InputDeviceId);
    }

    private async Task RunDirectionAsync(
        string fromLocale,
        string toLangCode,
        string? ttsVoice,
        AudioConfig audioConfig,
        Action<string>? onInterim,
        Action<string, string>? onFinal,
        Action<string>? onError,
        CancellationToken token)
    {
        try
        {
            var config = SpeechTranslationConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);
            config.SpeechRecognitionLanguage = fromLocale;
            config.AddTargetLanguage(toLangCode);

            using var recognizer = new TranslationRecognizer(config, audioConfig);

            recognizer.Recognizing += (_, e) =>
            {
                if (e.Result.Reason == ResultReason.TranslatingSpeech)
                    onInterim?.Invoke(e.Result.Text);
            };

            recognizer.Recognized += async (_, e) =>
            {
                if (e.Result.Reason == ResultReason.TranslatedSpeech
                    && !string.IsNullOrWhiteSpace(e.Result.Text))
                {
                    var translated = e.Result.Translations[toLangCode];
                    onFinal?.Invoke(e.Result.Text, translated);

                    if (ttsVoice != null && !string.IsNullOrWhiteSpace(translated))
                        await SynthesizeAsync(translated, ttsVoice);
                }
            };

            recognizer.Canceled += (_, e) =>
            {
                if (e.Reason == CancellationReason.Error)
                    onError?.Invoke($"API fout: {e.ErrorDetails}");
            };

            await recognizer.StartContinuousRecognitionAsync();

            try { await Task.Delay(Timeout.Infinite, token); }
            catch (OperationCanceledException) { }
            finally { await recognizer.StopContinuousRecognitionAsync(); }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { onError?.Invoke(ex.Message); }
    }

    private async Task RunLoopbackDirectionAsync(
        string fromLocale,
        string toLangCode,
        Action<string>? onInterim,
        Action<string, string>? onFinal,
        Action<string>? onError,
        CancellationToken token)
    {
        try
        {
            var (audioConfig, resources) = CreateLoopbackAudioConfig();
            using var _ = resources;
            await RunDirectionAsync(fromLocale, toLangCode, null,
                audioConfig, onInterim, onFinal, onError, token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { onError?.Invoke($"Loopback fout: {ex.Message}"); }
    }

    private (AudioConfig, IDisposable) CreateLoopbackAudioConfig()
    {
        var streamFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
        var pushStream = AudioInputStream.CreatePushStream(streamFormat);

        var enumerator = new MMDeviceEnumerator();
        var device = string.IsNullOrEmpty(_settings.LoopbackDeviceId)
            ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
            : enumerator.GetDevice(_settings.LoopbackDeviceId);

        var capture = new WasapiLoopbackCapture(device);

        var sourceBuffer = new BufferedWaveProvider(capture.WaveFormat)
        {
            DiscardOnBufferOverflow = true,
            BufferLength = capture.WaveFormat.AverageBytesPerSecond * 3
        };

        ISampleProvider pipeline = sourceBuffer.ToSampleProvider();
        if (capture.WaveFormat.Channels > 1)
            pipeline = new StereoToMonoSampleProvider(pipeline);
        pipeline = new WdlResamplingSampleProvider(pipeline, 16000);
        var pcmProvider = pipeline.ToWaveProvider16();

        capture.DataAvailable += (_, e) =>
        {
            sourceBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            var buf = new byte[8192];
            int read;
            while ((read = pcmProvider.Read(buf, 0, buf.Length)) > 0)
                pushStream.Write(buf, read);
        };

        capture.StartRecording();

        var audioConfig = AudioConfig.FromStreamInput(pushStream);
        var resources = new ActionDisposable(() =>
        {
            capture.StopRecording();
            capture.Dispose();
            device.Dispose();
            enumerator.Dispose();
            pushStream.Close();
        });

        return (audioConfig, resources);
    }

    private async Task SynthesizeAsync(string text, string voice)
    {
        var config = SpeechConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);
        config.SpeechSynthesisVoiceName = voice;

        AudioConfig audioConfig = string.IsNullOrEmpty(_settings.OutputDeviceId)
            ? AudioConfig.FromDefaultSpeakerOutput()
            : AudioConfig.FromSpeakerOutput(_settings.OutputDeviceId);

        using var synthesizer = new SpeechSynthesizer(config, audioConfig);
        await synthesizer.SpeakTextAsync(text);
    }

    private sealed class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        public ActionDisposable(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
