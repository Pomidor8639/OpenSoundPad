using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSoundPad.Audio;

namespace OpenSoundPad;

public partial class SettingsWindow : Window
{
    private readonly OspConfig _config;
    private readonly string _initialLang;
    private bool _ready;

    public string? SelectedInputId { get; private set; }
    public string? SelectedOutputId { get; private set; }
    public string? SelectedMonitorId { get; private set; }

    public SettingsWindow(OspConfig config)
    {
        _config = config;
        _initialLang = config.Language ?? "ru";
        InitializeComponent();
        ApplyLang();
        RefreshLists();
        SelectById(InputBox, config.InputDeviceId);
        SelectById(MonitorBox, config.MonitorDeviceId);
        InputBox.SelectionChanged += (_, _) => UpdateApoStatus();
        UpdateApoStatus();

        PregainSlider.Value = config.InputPregain * 100;
        GateSlider.Value = config.GateThreshold * 10000;
        UpdateParamLabels();
        PregainSlider.ValueChanged += (_, _) => UpdateParamLabels();
        GateSlider.ValueChanged += (_, _) => UpdateParamLabels();

        LangBox.ItemsSource = new List<LanguageOption>
        {
            new() { Code = "en", Name = "English", FlagUri = "pack://application:,,,/Flags/gb.png" },
            new() { Code = "ru", Name = "Русский", FlagUri = "pack://application:,,,/Flags/ru.png" },
            new() { Code = "de", Name = "Deutsch", FlagUri = "pack://application:,,,/Flags/de.png" },
            new() { Code = "es", Name = "Español", FlagUri = "pack://application:,,,/Flags/es.png" },
            new() { Code = "fr", Name = "Français", FlagUri = "pack://application:,,,/Flags/fr.png" },
            new() { Code = "zh", Name = "简体中文", FlagUri = "pack://application:,,,/Flags/cn.png" },
            new() { Code = "ja", Name = "日本語", FlagUri = "pack://application:,,,/Flags/jp.png" },
            new() { Code = "ko", Name = "한국어", FlagUri = "pack://application:,,,/Flags/kr.png" },
        };
        LangBox.SelectedIndex = config.Language switch
        {
            "ru" => 1,
            "de" => 2,
            "es" => 3,
            "fr" => 4,
            "zh" => 5,
            "ja" => 6,
            "ko" => 7,
            _ => 0
        };

        ConfigPathText.Text = OspConfig.GetFilePath();
        _ready = true;
    }

    private void ApplyLang()
    {
        Title = Loc.SettingsTitle;
        DevGroup.Header = Loc.SetDevices;
        MicLabel.Text = Loc.SetMic;
        MonLabel.Text = Loc.SetMonitor;
        ApoLabel.Text = Loc.ApoInterception;
        ApoDescText.Text = Loc.ApoDesc;
        InstallApoBtnText.Text = Loc.InstallApo;
        UninstallApoBtnText.Text = Loc.UninstallApo;
        RefreshBtnText.Text = Loc.Refresh;
        ParGroup.Header = Loc.SetParams;
        LangGroup.Header = Loc.SetLang;
        CfgLabel.Text = Loc.SetConfigFile;
        OpenFolderBtnText.Text = Loc.OpenFolder;
        ResetBtnText.Text = Loc.ResetAll;
        AboutGroup.Header = Loc.SetAbout;
        AppDescText.Text = Loc.SetAboutDesc;
        CancelBtn.Content = Loc.Cancel;
        OkBtn.Content = Loc.Ok;
        UpdateApoStatus();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        catch { }
    }

    private void LangBox_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        if (LangBox.SelectedItem is LanguageOption opt)
        {
            Loc.Lang = opt.Code;
        }
        else
        {
            Loc.Lang = LangBox.SelectedIndex switch
            {
                1 => "ru",
                2 => "de",
                3 => "es",
                4 => "fr",
                5 => "zh",
                6 => "ja",
                7 => "ko",
                _ => "en"
            };
        }
        ApplyLang();
        UpdateParamLabels();
    }

    private void UpdateParamLabels()
    {
        PregainLabel.Text = Loc.Pregain(PregainSlider.Value / 100);
        GateLabel.Text = Loc.Gate(GateSlider.Value / 10000);
    }

    private void RefreshLists()
    {
        string? i = (InputBox.SelectedItem as OspDevice)?.Id;
        string? m = (MonitorBox.SelectedItem as OspDevice)?.Id;
        InputBox.ItemsSource = OspEngine.GetInputMicrophones();
        MonitorBox.ItemsSource = OspEngine.GetOutputDevices();
        SelectById(InputBox, i);
        SelectById(MonitorBox, m);
    }

    private static void SelectById(ComboBox box, string? id)
    {
        if (id != null)
            foreach (var item in box.Items)
                if (item is OspDevice d && d.Id == id) { box.SelectedItem = item; return; }
        if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
    }

    private void UpdateApoStatus()
    {
        string? micId = (InputBox.SelectedItem as OspDevice)?.Id;
        bool isInstalled = !string.IsNullOrEmpty(micId) && OspApoManager.IsApoInstalledOnEndpoint(micId);

        if (isInstalled)
        {
            ApoStatusBadge.Text = Loc.ApoActive;
            ApoStatusBadge.Foreground = (Brush)FindResource("AccentGreen");
            InstallApoBtn.Visibility = Visibility.Collapsed;
            UninstallApoBtn.Visibility = Visibility.Visible;
        }
        else
        {
            ApoStatusBadge.Text = Loc.ApoNotInstalled;
            ApoStatusBadge.Foreground = (Brush)FindResource("TextMuted");
            InstallApoBtn.Visibility = Visibility.Visible;
            UninstallApoBtn.Visibility = Visibility.Collapsed;
        }
    }

    private void InstallApoBtn_Click(object sender, RoutedEventArgs e)
    {
        string? micId = (InputBox.SelectedItem as OspDevice)?.Id;
        if (string.IsNullOrWhiteSpace(micId)) return;

        bool ok = OspApoManager.InstallApoOnEndpoint(micId);
        UpdateApoStatus();
        if (ok)
        {
            MessageBox.Show(Loc.ApoStatusSuccess, "OpenSoundPad", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(Loc.ApoStatusFailed, "OpenSoundPad", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void UninstallApoBtn_Click(object sender, RoutedEventArgs e)
    {
        string? micId = (InputBox.SelectedItem as OspDevice)?.Id;
        if (string.IsNullOrWhiteSpace(micId)) return;

        bool ok = OspApoManager.UninstallApoFromEndpoint(micId);
        UpdateApoStatus();
        if (ok)
        {
            MessageBox.Show(Loc.ApoStatusSuccess, "OpenSoundPad", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        RefreshLists();
        ApplyLang();
        UpdateApoStatus();
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? dir = Path.GetDirectoryName(OspConfig.GetFilePath());
            if (dir != null) Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
        }
        catch { }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(Loc.ResetAsk, "OSP", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        var pads = _config.PadFiles;
        var lang = _config.Language;
        var fresh = new OspConfig { PadFiles = pads, Language = lang };
        _config.InputDeviceId = fresh.InputDeviceId;
        _config.OutputDeviceId = fresh.OutputDeviceId;
        _config.MonitorDeviceId = fresh.MonitorDeviceId;
        _config.VoiceId = fresh.VoiceId;
        _config.VolumeGain = fresh.VolumeGain;
        _config.PitchSemitones = fresh.PitchSemitones;
        _config.Drive = fresh.Drive;
        _config.BassBoostDb = fresh.BassBoostDb;
        _config.RobotMod = fresh.RobotMod;
        _config.PadGain = fresh.PadGain;
        _config.InputPregain = fresh.InputPregain;
        _config.GateThreshold = fresh.GateThreshold;
        _config.Save();
        RefreshLists();
        SelectById(InputBox, null);
        SelectById(MonitorBox, null);
        UpdateApoStatus();
        PregainSlider.Value = _config.InputPregain * 100;
        GateSlider.Value = _config.GateThreshold * 10000;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Loc.Lang = _initialLang;
        DialogResult = false;
        Close();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        SelectedInputId = (InputBox.SelectedItem as OspDevice)?.Id;
        SelectedOutputId = null;
        SelectedMonitorId = (MonitorBox.SelectedItem as OspDevice)?.Id;
        _config.InputDeviceId = SelectedInputId;
        _config.OutputDeviceId = SelectedOutputId;
        _config.MonitorDeviceId = SelectedMonitorId;
        _config.InputPregain = (float)(PregainSlider.Value / 100.0);
        _config.GateThreshold = (float)(GateSlider.Value / 10000.0);
        _config.Language = Loc.Lang;
        _config.Save();
        DialogResult = true;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    [System.Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(System.IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    protected override void OnSourceInitialized(System.EventArgs e)
    {
        base.OnSourceInitialized(e);
        try
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            int darkMode = 1;
            DwmSetWindowAttribute(helper.Handle, 20, ref darkMode, sizeof(int));
            DwmSetWindowAttribute(helper.Handle, 19, ref darkMode, sizeof(int));
        }
        catch
        {
        }
    }
}

public sealed class LanguageOption
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string FlagUri { get; init; }
}
