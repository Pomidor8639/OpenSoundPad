using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using OpenSoundPad.Audio;

namespace OpenSoundPad;

public partial class MainWindow : Window
{
    private readonly OspEngine _engine = new();
    private readonly OspConfig _config;
    private readonly DispatcherTimer _vuTimer;
    private bool _updatingUi = true;
    private bool _initialized;

    public MainWindow()
    {
        InitializeComponent();
        _config = OspConfig.Load();
        Loc.Lang = _config.Language == "en" ? "en" : "ru";
        ConfigPathText.Text = OspConfig.GetFilePath();

        ApplyLanguage();
        RefreshVoicesList();
        RefreshPadList();
        UpdateDeviceDisplay();
        ApplyConfigToUi();

        _vuTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _vuTimer.Tick += (_, _) =>
        {
            InputVu.Value = _engine.InputLevel * 100;
            OutputVu.Value = _engine.OutputLevel * 100;
        };
        _vuTimer.Start();

        KeyDown += OnKeyDown;
        Closed += (_, _) => { _vuTimer.Stop(); _engine.Stop(); _config.Save(); };
        _initialized = true;
        _updatingUi = false;
    }

    // ---------- язык ----------

    private void ApplyLanguage()
    {
        FileMenu.Header = Loc.MenuFile;
        MiOpenFolder.Header = Loc.MenuOpenFolder;
        MiExit.Header = Loc.MenuExit;
        SettingsMenu.Header = Loc.MenuSettings;
        MiSettings.Header = Loc.MenuOpenSettings;
        HelpMenu.Header = Loc.MenuHelp;
        MiAbout.Header = Loc.MenuAbout;

        TbStart.Content = "▶ " + Loc.TbStart;
        TbStop.Content = "■ " + Loc.TbStop;
        UpdateToggleButtons();

        VoicesGroup.Header = Loc.GrVoices;
        PadsGroup.Header = Loc.GrPads;
        LevelsGroup.Header = Loc.GrLevels;
        EffectLabel.Text = Loc.Effect + " ";
        EffectState.Text = _engine.Dsp.Bypass ? Loc.EffectBypass : Loc.EffectActive;
        CustomHeader.Text = Loc.CustomVoice;
        HotkeyHint.Text = Loc.HotkeysHint;

        ColNum.Header = Loc.ColNum;
        ColName.Header = Loc.ColName;
        ColDur.Header = Loc.ColDur;
        ColKey.Header = Loc.ColKey;
        PadGainLabel.Text = Loc.PadGain;
        StopPadsBtn.Content = Loc.StopPads;
        CtxLoad.Header = Loc.LoadSound;
        CtxClear.Header = Loc.Clear;
        PadsView.ToolTip = Loc.PlayTip;

        LevelInLabel.Text = Loc.LevelIn;
        LevelOutLabel.Text = Loc.LevelOut;
        VirtMicHeader.Text = Loc.VirtMic;
        CableHint.Text = Loc.CableHint;

        if (!_engine.IsRunning) StatusText.Text = Loc.Stopped;
        UpdateLabels();
        RefreshVoicesList();
        RefreshPadList();
    }

    private void UpdateToggleButtons()
    {
        TbEffect.Content = _engine.Dsp.Bypass ? Loc.TbEffectOn : Loc.TbEffectOff;
        TbMute.Content = _engine.Dsp.Muted ? Loc.TbUnmute : Loc.TbMute;
        TbMonitor.Content = _engine.IsMonitoring ? Loc.TbMonitorOn : Loc.TbMonitor;
    }

    // ---------- голоса ----------

    private void RefreshVoicesList()
    {
        int sel = SelectedVoice() - 1;
        VoicesBox.Items.Clear();
        for (int i = 1; i <= 5; i++)
            VoicesBox.Items.Add($"{i} — {Loc.VoiceName(i)}");
        VoicesBox.SelectedIndex = Math.Clamp(sel, 0, 4);
    }

    private int SelectedVoice() => VoicesBox.SelectedIndex is >= 0 and <= 4 ? VoicesBox.SelectedIndex + 1 : 1;

    private void VoicesBox_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized || _updatingUi || VoicesBox.SelectedIndex < 0) return;
        int v = VoicesBox.SelectedIndex + 1;
        _engine.Dsp.SetVoice(v);
        _config.VoiceId = v;
        _config.Save();
        if (_engine.IsRunning && StatusText.Text.Contains('•'))
            StatusText.Text = StatusText.Text.Split('•')[0].TrimEnd() + $" • {Loc.VoiceName(v)}";
    }

    // ---------- пады ----------

    private sealed record PadRow(int Num, string Name, string Duration, string Key);

    private void RefreshPadList()
    {
        int sel = PadsView.SelectedIndex;
        PadsView.Items.Clear();
        for (int i = 0; i < SoundPadBank.PadCount; i++)
        {
            string name = _engine.Pads.GetPadName(i);
            if (name == $"Pad {i + 1}") name = Loc.Pad(i + 1);
            var dur = _engine.Pads.GetPadDuration(i);
            PadsView.Items.Add(new PadRow(
                i + 1, name,
                dur.HasValue ? $"{(int)dur.Value.TotalMinutes}:{dur.Value.Seconds:00}" : "—",
                $"F{i + 1}"));
        }
        if (sel >= 0 && sel < PadsView.Items.Count) PadsView.SelectedIndex = sel;
    }

    private int SelectedPad() => PadsView.SelectedIndex is >= 0 and < SoundPadBank.PadCount
        ? PadsView.SelectedIndex : 0;

    private void PadsView_DoubleClick(object sender, MouseButtonEventArgs e) =>
        _engine.Pads.Trigger(SelectedPad());

    private void PadsView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            _engine.Pads.Trigger(SelectedPad());
            e.Handled = true;
        }
    }

    private void CtxLoad_Click(object sender, RoutedEventArgs e) => LoadPadFile(SelectedPad());
    private void CtxClear_Click(object sender, RoutedEventArgs e)
    {
        int idx = SelectedPad();
        _engine.Pads.ClearPad(idx);
        _config.PadFiles.Remove(idx.ToString());
        _config.Save();
        RefreshPadList();
    }

    private void LoadPadFile(int idx)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Audio|*.wav;*.mp3;*.aiff;*.wma;*.m4a;*.ogg;*.flac|All|*.*",
            Title = $"{Loc.LoadSound} ({Loc.Pad(idx + 1)})",
        };
        if (dlg.ShowDialog() == true)
        {
            if (_engine.Pads.LoadPad(idx, dlg.FileName))
            {
                _config.PadFiles[idx.ToString()] = dlg.FileName;
                _config.Save();
                RefreshPadList();
            }
            else MessageBox.Show(Loc.LoadFailed, "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---------- устройства (только отображение; выбор — в настройках) ----------

    private static string? FindDeviceName(IEnumerable<OspDevice> list, string? id)
    {
        foreach (var d in list)
            if (d.Id == id) return d.Name;
        foreach (var d in list) return d.Name;
        return null;
    }

    private void UpdateDeviceDisplay()
    {
        string? cableName = FindDeviceName(OspEngine.GetVirtualCables(), _config.OutputDeviceId);
        VirtMicText.Text = cableName != null
            ? OspEngine.FindVirtualMicName(cableName)
            : Loc.NoCable;
    }

    // ---------- меню / тулбар ----------

    private void MiOpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? dir = Path.GetDirectoryName(OspConfig.GetFilePath());
            if (dir != null) Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
        }
        catch { }
    }

    private void MiExit_Click(object sender, RoutedEventArgs e) => Close();
    private void MiSettings_Click(object sender, RoutedEventArgs e) => OpenSettings();
    private void MiAbout_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(Loc.AboutText, "OSP", MessageBoxButton.OK, MessageBoxImage.Information);

    private void OpenSettings()
    {
        bool wasRunning = _engine.IsRunning;
        if (wasRunning) StopEngine();
        string oldLang = Loc.Lang;
        var dlg = new SettingsWindow(_config) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _engine.Dsp.InputPregain = _config.InputPregain;
            _engine.Dsp.GateThreshold = _config.GateThreshold;
            if (Loc.Lang != oldLang) ApplyLanguage();
            ApplyConfigToUi();
            UpdateDeviceDisplay();
        }
        else if (Loc.Lang != oldLang)
        {
            ApplyLanguage();
        }
    }

    // ---------- config ----------

    private void ApplyConfigToUi()
    {
        _updatingUi = true;
        try
        {
            VolumeSlider.Value = Math.Clamp(_config.VolumeGain * 100, 50, 400);
            PitchSlider.Value = _config.PitchSemitones;
            DriveSlider.Value = _config.Drive * 100;
            BassSlider.Value = _config.BassBoostDb;
            RobotSlider.Value = _config.RobotMod * 100;
            PadGainSlider.Value = _config.PadGain * 100;
            _engine.Pads.MasterGain = _config.PadGain;
            _engine.Pads.SetSampleRate(48000);
            foreach (var kv in _config.PadFiles)
                if (int.TryParse(kv.Key, out int idx) && idx is >= 0 and < SoundPadBank.PadCount && File.Exists(kv.Value))
                    _engine.Pads.LoadPad(idx, kv.Value);
            VoicesBox.SelectedIndex = Math.Clamp(_config.VoiceId - 1, 0, 4);
            UpdateLabels();
            RefreshPadList();
        }
        finally { _updatingUi = false; }
    }

    private void UpdateLabels()
    {
        VolumeLabel.Text = Loc.Volume((int)VolumeSlider.Value);
        PitchLabel.Text = Loc.Pitch(PitchSlider.Value);
        DriveLabel.Text = Loc.Drive((int)DriveSlider.Value);
        BassLabel.Text = Loc.Bass(BassSlider.Value);
        RobotLabel.Text = Loc.Robot((int)RobotSlider.Value);
    }

    private void PushParamsToDsp()
    {
        _engine.Dsp.SetCustomParams(
            (float)PitchSlider.Value,
            (float)(DriveSlider.Value / 100.0),
            (float)BassSlider.Value,
            (float)(RobotSlider.Value / 100.0));
        _config.PitchSemitones = (float)PitchSlider.Value;
        _config.Drive = (float)(DriveSlider.Value / 100.0);
        _config.BassBoostDb = (float)BassSlider.Value;
        _config.RobotMod = (float)(RobotSlider.Value / 100.0);
    }

    // ---------- engine ----------

    private void StartBtn_Click(object sender, RoutedEventArgs e) => StartEngine();
    private void StopBtn_Click(object sender, RoutedEventArgs e) => StopEngine();

    private void StartEngine()
    {
        try
        {
            if (_config.InputDeviceId == null || _config.OutputDeviceId == null)
            {
                MessageBox.Show(Loc.NeedDevices, "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
                OpenSettings();
                if (_config.InputDeviceId == null || _config.OutputDeviceId == null) return;
            }
            _engine.SetDevices(_config.InputDeviceId, _config.OutputDeviceId, _config.MonitorDeviceId);

            _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);
            _engine.Dsp.InputPregain = _config.InputPregain;
            _engine.Dsp.GateThreshold = _config.GateThreshold;
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Pads.SetSampleRate(_engine.Dsp.SampleRate);
            _engine.Pads.MasterGain = (float)(PadGainSlider.Value / 100.0);
            _config.Save();

            _engine.Start();
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);
            _engine.Dsp.InputPregain = _config.InputPregain;
            _engine.Dsp.GateThreshold = _config.GateThreshold;

            string inName = FindDeviceName(OspEngine.GetInputMicrophones(), _config.InputDeviceId) ?? "?";
            string outName = FindDeviceName(OspEngine.GetVirtualCables(), _config.OutputDeviceId) ?? "?";
            TbStart.IsEnabled = false;
            TbStop.IsEnabled = true;
            StatusText.Text = Loc.Live(inName, outName, Loc.VoiceName(SelectedVoice()));
            UpdateDeviceDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Loc.StartFailed + ex.Message, "OSP", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopEngine()
    {
        _engine.Stop();
        TbStart.IsEnabled = true;
        TbStop.IsEnabled = false;
        StatusText.Text = Loc.Stopped;
    }

    // ---------- voice UI ----------

    private void CustomParam_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        UpdateLabels();
        PushParamsToDsp();
        if (VoicesBox.SelectedIndex != 4) VoicesBox.SelectedIndex = 4;
        _config.VoiceId = 5;
        _config.Save();
    }

    private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        UpdateLabels();
        _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);
        _config.VolumeGain = _engine.Dsp.VolumeGain;
        _config.Save();
    }

    private void PadGain_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        _engine.Pads.MasterGain = (float)(PadGainSlider.Value / 100.0);
        _config.PadGain = _engine.Pads.MasterGain;
        _config.Save();
    }

    private void EffectBtn_Click(object sender, RoutedEventArgs e) => ToggleEffect();
    private void MuteBtn_Click(object sender, RoutedEventArgs e) => ToggleMute();
    private void MonitorBtn_Click(object sender, RoutedEventArgs e) => ToggleMonitor();
    private void StopPadsBtn_Click(object sender, RoutedEventArgs e) => _engine.Pads.StopAll();

    private void ToggleEffect()
    {
        _engine.Dsp.Bypass = !_engine.Dsp.Bypass;
        EffectState.Text = _engine.Dsp.Bypass ? Loc.EffectBypass : Loc.EffectActive;
        UpdateToggleButtons();
    }

    private void ToggleMute()
    {
        _engine.Dsp.Muted = !_engine.Dsp.Muted;
        UpdateToggleButtons();
    }

    private void ToggleMonitor()
    {
        _engine.SetMonitoring(!_engine.IsMonitoring);
        UpdateToggleButtons();
    }

    // ---------- hotkeys ----------

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is >= Key.D1 and <= Key.D5)
        {
            VoicesBox.SelectedIndex = (int)e.Key - (int)Key.D1;
        }
        else switch (e.Key)
        {
            case Key.T: ToggleEffect(); break;
            case Key.M: ToggleMute(); break;
            case Key.L: ToggleMonitor(); break;
            case Key.F1: case Key.F2: case Key.F3: case Key.F4:
            case Key.F5: case Key.F6: case Key.F7: case Key.F8:
            case Key.F9: case Key.F10: case Key.F11: case Key.F12:
                _engine.Pads.Trigger((int)e.Key - (int)Key.F1);
                break;
        }
    }
}
