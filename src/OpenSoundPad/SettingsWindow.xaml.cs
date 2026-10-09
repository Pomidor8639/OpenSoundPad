using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        SelectById(CableBox, config.OutputDeviceId);
        SelectById(MonitorBox, config.MonitorDeviceId);
        CableBox.SelectionChanged += (_, _) => UpdateVirtMic();
        UpdateVirtMic();

        PregainSlider.Value = config.InputPregain * 100;
        GateSlider.Value = config.GateThreshold * 10000;
        UpdateParamLabels();
        PregainSlider.ValueChanged += (_, _) => UpdateParamLabels();
        GateSlider.ValueChanged += (_, _) => UpdateParamLabels();

        LangBox.Items.Add("Русский");
        LangBox.Items.Add("English");
        LangBox.SelectedIndex = config.Language == "en" ? 1 : 0;

        ConfigPathText.Text = OspConfig.GetFilePath();
        _ready = true;
    }

    private void ApplyLang()
    {
        Title = Loc.SettingsTitle;
        DevGroup.Header = Loc.SetDevices;
        MicLabel.Text = Loc.SetMic;
        bool hasCable = OspEngine.HasVirtualCable();
        CableLabel.Text = hasCable ? Loc.SetCable : Loc.SetCableFallback;
        MonLabel.Text = Loc.SetMonitor;
        VirtLabel.Text = hasCable ? Loc.SetVirtInSystem : Loc.SetVirtFallback;
        InstallCableBtnText.Text = Loc.Lang == "en"
            ? "Install VB-CABLE driver (from vbcable folder)"
            : "Установить драйвер VB-CABLE (из папки vbcable)";
        RefreshBtnText.Text = Loc.Refresh;
        ParGroup.Header = Loc.SetParams;
        LangGroup.Header = Loc.SetLang == "Язык:" ? "Язык / Language" : "Language / Язык";
        CfgLabel.Text = Loc.SetConfigFile;
        OpenFolderBtnText.Text = Loc.OpenFolder;
        ResetBtnText.Text = Loc.ResetAll;
        AboutGroup.Header = Loc.SetAbout;
        AppDescText.Text = Loc.SetAboutDesc;
        CancelBtn.Content = Loc.Cancel;
        OkBtn.Content = Loc.Ok;
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
        Loc.Lang = LangBox.SelectedIndex == 1 ? "en" : "ru";
        ApplyLang();
        UpdateParamLabels();
        UpdateVirtMic();
    }

    private void UpdateParamLabels()
    {
        PregainLabel.Text = Loc.Pregain(PregainSlider.Value / 100);
        GateLabel.Text = Loc.Gate(GateSlider.Value / 10000);
    }

    private void RefreshLists()
    {
        string? i = (InputBox.SelectedItem as OspDevice)?.Id;
        string? c = (CableBox.SelectedItem as OspDevice)?.Id;
        string? m = (MonitorBox.SelectedItem as OspDevice)?.Id;
        InputBox.ItemsSource = OspEngine.GetInputMicrophones();
        CableBox.ItemsSource = OspEngine.GetVirtualCables();
        MonitorBox.ItemsSource = OspEngine.GetOutputDevices();
        SelectById(InputBox, i);
        SelectById(CableBox, c);
        SelectById(MonitorBox, m);
    }

    private static void SelectById(ComboBox box, string? id)
    {
        if (id != null)
            foreach (var item in box.Items)
                if (item is OspDevice d && d.Id == id) { box.SelectedItem = item; return; }
        if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
    }

    private void UpdateVirtMic()
    {
        bool hasCable = OspEngine.HasVirtualCable();
        InstallCableBtn.Visibility = !hasCable ? Visibility.Visible : Visibility.Collapsed;
        VirtMicText.Text = CableBox.SelectedItem is OspDevice dev
            ? OspEngine.FindVirtualMicName(dev.Name)
            : Loc.NoCable;
    }

    private void InstallCableBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string cableDir = Path.Combine(baseDir, "vbcable");
            string setupExe = Path.Combine(cableDir, "VBCABLE_Setup_x64.exe");
            if (File.Exists(setupExe))
            {
                Process.Start(new ProcessStartInfo(setupExe) { UseShellExecute = true, Verb = "runas" });
            }
            else if (Directory.Exists(cableDir))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", cableDir) { UseShellExecute = true });
            }
            else
            {
                Process.Start(new ProcessStartInfo("https://vb-audio.com/Cable/") { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        RefreshLists();
        ApplyLang();
        UpdateVirtMic();
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
        SelectById(CableBox, null);
        SelectById(MonitorBox, null);
        UpdateVirtMic();
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
        SelectedOutputId = (CableBox.SelectedItem as OspDevice)?.Id;
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
}
