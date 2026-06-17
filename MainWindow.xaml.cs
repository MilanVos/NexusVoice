using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NexusVoice.Models;
using NexusVoice.Services;

namespace NexusVoice;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ConversationMessage> _messages = new();
    private AppSettings _settings;
    private SpeechTranslationService? _service;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private DateTime _sessionStart;

    public MainWindow()
    {
        InitializeComponent();
        ConversationList.ItemsSource = _messages;

        _messages.CollectionChanged += (_, _) =>
        {
            EmptyState.Visibility = _messages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            Dispatcher.BeginInvoke(() => ConversationScroller.ScrollToEnd());
        };

        _timer.Tick += (_, _) =>
            SessionTimer.Text = (DateTime.Now - _sessionStart).ToString(@"mm\:ss");

        _settings = AppSettings.Load();
        RefreshService();
        UpdateSetupHint();
    }

    private void RefreshService()
    {
        _service = _settings.IsConfigured ? new SpeechTranslationService(_settings) : null;
    }

    private void UpdateSetupHint()
    {
        SetupHint.Visibility = _settings.IsConfigured ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
            StopSession();
        else
            await StartSession();
    }

    private async Task StartSession()
    {
        if (_service == null)
        {
            SetStatus("⚠ Configureer eerst je Azure API-sleutel", "#FFC107");
            SettingsWindow.ShowDialog(this, _settings, OnSettingsSaved);
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _sessionStart = DateTime.Now;

        ToggleButton.Content = "⏹  Stop Vertaling";
        ToggleButton.Background = (SolidColorBrush)FindResource("ErrorBrush");
        ToggleButton.IsEnabled = true;

        SetLivePanelActive(nl: true, de: true);
        SetStatus("🔴 Live — vertaling actief", "#4CAF50");
        _timer.Start();

        NLInterimText.Text = "Luistert naar Nederlands...";
        DERecogText.Text = "Luistert naar Discord...";
        DEInterimText.Text = "";
        NLTranslatedText.Text = "";

        var callbacks = new SessionCallbacks
        {
            OnNLInterim = text => Dispatcher.BeginInvoke(() =>
            {
                NLInterimText.Text = text;
                SetDotActive(NLDot, true);
            }),

            OnNLFinal = (nl, de) => Dispatcher.BeginInvoke(() =>
            {
                NLInterimText.Text = nl;
                DEInterimText.Text = $"🇩🇪 {de}";
                SetDotActive(NLDot, false);
                _messages.Add(new ConversationMessage
                {
                    Speaker = Speaker.Dutch,
                    OriginalText = nl,
                    TranslatedText = de
                });
            }),

            OnDEInterim = text => Dispatcher.BeginInvoke(() =>
            {
                DERecogText.Text = text;
                SetDotActive(DEDot, true);
            }),

            OnDEFinal = (de, nl) => Dispatcher.BeginInvoke(() =>
            {
                DERecogText.Text = de;
                NLTranslatedText.Text = $"🇳🇱 {nl}";
                SetDotActive(DEDot, false);
                _messages.Add(new ConversationMessage
                {
                    Speaker = Speaker.German,
                    OriginalText = de,
                    TranslatedText = nl
                });
            }),

            OnError = msg => Dispatcher.BeginInvoke(() =>
                SetStatus($"✗ {msg}", "#F44336"))
        };

        try
        {
            await _service.RunSessionAsync(callbacks, _cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            SetStatus($"✗ {ex.Message}", "#F44336");
        }
        finally
        {
            FinishStop();
        }
    }

    private void StopSession()
    {
        _cts?.Cancel();
    }

    private void FinishStop()
    {
        Dispatcher.Invoke(() =>
        {
            _isRunning = false;
            _timer.Stop();

            ToggleButton.Content = "🎙  Start Vertaling";
            ToggleButton.Background = (SolidColorBrush)FindResource("NLBrush");
            ToggleButton.IsEnabled = true;

            SetLivePanelActive(nl: false, de: false);
            NLInterimText.Text = "Spreek Nederlands...";
            DEInterimText.Text = "";
            DERecogText.Text = "Luistert via Discord...";
            NLTranslatedText.Text = "";
            SessionTimer.Text = "";

            SetStatus("Gestopt", null);
        });
    }

    private void SetLivePanelActive(bool nl, bool de)
    {
        SetDotActive(NLDot, nl);
        SetDotActive(DEDot, de);
    }

    private void SetDotActive(Ellipse dot, bool active)
    {
        dot.Fill = active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
            : (SolidColorBrush)FindResource("SubTextBrush");
    }

    private void SetStatus(string message, string? colorHex)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
            StatusDot.Fill = colorHex != null
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
                : (SolidColorBrush)FindResource("SubTextBrush");
        });
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (_messages.Count == 0) return;
        if (MessageBox.Show("Wil je het gespreklog wissen?", "Bevestigen",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            _messages.Clear();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow.ShowDialog(this, _settings, OnSettingsSaved);
    }

    private void OnSettingsSaved(AppSettings newSettings)
    {
        _settings = newSettings;
        RefreshService();
        UpdateSetupHint();
        SetStatus("Instellingen opgeslagen", "#4CAF50");
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        base.OnClosed(e);
    }
}
