using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly List<Button> _padButtons = new();
    private bool _updatingUi;

    public MainWindow()
    {
        InitializeComponent();
        _config = OspConfig.Load();
        ConfigPathText.Text = OspConfig.GetFilePath();

        BuildPadGrid();
        RefreshDevices(selectSaved: true);
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
    }

    // ---------- pads ----------

    private void BuildPadGrid()
    {
        PadGrid.Children.Clear();
        _padButtons.Clear();
        for (int i = 0; i < SoundPadBank.PadCount; i++)
        {
            int idx = i;
            var btn = new Button
            {
                Content = _engine.Pads.GetPadName(idx),
                FontSize = 13,
                MinHeight = 64,
                Tag = idx,
                ToolTip = "ЛКМ — играть (или F1..F12). ПКМ — загрузить/очистить.",
            };
            btn.Click += (_, _) => _engine.Pads.Trigger(idx);
            var ctx = new ContextMenu();
            var load = new MenuItem { Header = "Загрузить звук…" };
            load.Click += (_, _) => LoadPadFile(idx);
            var clear = new MenuItem { Header = "Очистить" };
            clear.Click += (_, _) =>
            {
                _engine.Pads.ClearPad(idx);
                _config.PadFiles.Remove(idx.ToString());
                _config.Save();
                RefreshPadNames();
            };
            ctx.Items.Add(load);
            ctx.Items.Add(clear);
            btn.ContextMenu = ctx;
            _padButtons.Add(btn);
            PadGrid.Children.Add(btn);
        }
        RefreshPadNames();
    }

    private void RefreshPadNames()
    {
        for (int i = 0; i < _padButtons.Count; i++)
        {
            string name = _engine.Pads.GetPadName(i);
            string key = i < 12 ? $"F{i + 1}" : "";
            _padButtons[i].Content = $"{i + 1}. {name}\n[{key}]";
        }
    }

    private void LoadPadFile(int idx)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Audio|*.wav;*.mp3;*.aiff;*.wma;*.m4a;*.ogg;*.flac|All|*.*",
            Title = $"Звук для пада {idx + 1}",
        };
        if (dlg.ShowDialog() == true)
        {
            if (_engine.Pads.LoadPad(idx, dlg.FileName))
            {
                _config.PadFiles[idx.ToString()] = dlg.FileName;
                _config.Save();
                RefreshPadNames();
            }
            else MessageBox.Show("Не удалось загрузить файл.", "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---------- devices ----------

    private void RefreshDevices(bool selectSaved = false)
    {
        var inputs = OspEngine.GetInputMicrophones();
        var cables = OspEngine.GetVirtualCables();
        var monitors = OspEngine.GetOutputDevices();

        InputBox.ItemsSource = inputs;
        CableBox.ItemsSource = cables;
        MonitorBox.ItemsSource = monitors;

        if (inputs.Count > 0) InputBox.SelectedIndex = 0;
        if (cables.Count > 0) CableBox.SelectedIndex = 0;
        if (monitors.Count > 0) MonitorBox.SelectedIndex = 0;

        if (selectSaved)
        {
            SelectById(InputBox, inputs, _config.InputDeviceId);
            SelectById(CableBox, cables, _config.OutputDeviceId);
            SelectById(MonitorBox, monitors, _config.MonitorDeviceId);
        }
        UpdateVirtMic();
    }

    private static void SelectById(ComboBox box, List<OspDevice> list, string? id)
    {
        if (id == null) return;
        for (int i = 0; i < list.Count; i++)
            if (list[i].Id == id) { box.SelectedIndex = i; return; }
    }

    private void UpdateVirtMic()
    {
        if (CableBox.SelectedItem is OspDevice dev)
            VirtMicText.Text = OspEngine.FindVirtualMicName(dev.Name);
        else
            VirtMicText.Text = "Кабель не выбран — в Discord нечего выводить";
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
            RefreshPadNames();
            CheckVoiceRadio(_config.VoiceId);
            UpdateLabels();
        }
        finally { _updatingUi = false; }
    }

    private void CheckVoiceRadio(int id)
    {
        Voice1.IsChecked = id == 1; Voice2.IsChecked = id == 2;
        Voice3.IsChecked = id == 3; Voice4.IsChecked = id == 4;
        Voice5.IsChecked = id == 5;
    }

    private int SelectedVoice() =>
        Voice2.IsChecked == true ? 2 :
        Voice3.IsChecked == true ? 3 :
        Voice4.IsChecked == true ? 4 :
        Voice5.IsChecked == true ? 5 : 1;

    private void UpdateLabels()
    {
        VolumeLabel.Text = $"Громкость: {(int)VolumeSlider.Value}%";
        PitchLabel.Text = $"Питч: {PitchSlider.Value:+0.0;-0.0} st";
        DriveLabel.Text = $"Дисторшн: {(int)DriveSlider.Value}%";
        BassLabel.Text = $"Бас: +{BassSlider.Value:0.0} дБ";
        RobotLabel.Text = $"Робот: {(int)RobotSlider.Value}%";
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
            if (InputBox.SelectedItem is not OspDevice inp || CableBox.SelectedItem is not OspDevice cable)
            {
                MessageBox.Show("Выберите микрофон и виртуальный кабель.", "OSP",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var mon = MonitorBox.SelectedItem as OspDevice;
            _engine.SetDevices(inp.Id, cable.Id, mon?.Id);
            _config.InputDeviceId = inp.Id;
            _config.OutputDeviceId = cable.Id;
            _config.MonitorDeviceId = mon?.Id;

            _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Pads.SetSampleRate(_engine.Dsp.SampleRate);
            _engine.Pads.MasterGain = (float)(PadGainSlider.Value / 100.0);
            _config.Save();

            _engine.Start();
            // Применяем сохранённый голос уже на частоте устройства
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);

            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            StatusText.Text = $"В эфире: {inp.Name} → {cable.Name}" +
                              (OspDsp.Voices.TryGetValue(SelectedVoice(), out var vn) ? $" • {vn}" : "");
            UpdateVirtMic();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось запустить звук: {ex.Message}", "OSP",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopEngine()
    {
        _engine.Stop();
        StartBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
        StatusText.Text = "Остановлен";
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e) => RefreshDevices();

    // ---------- voice UI ----------

    private void Voice_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingUi) return;
        int v = SelectedVoice();
        _engine.Dsp.SetVoice(v);
        _config.VoiceId = v;
        _config.Save();
        if (OspDsp.Voices.TryGetValue(v, out var name) && _engine.IsRunning)
            StatusText.Text = StatusText.Text.Split('•')[0].TrimEnd() + $" • {name}";
    }

    private void CustomParam_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateLabels();
        if (_updatingUi) return;
        PushParamsToDsp();
        if (Voice5.IsChecked != true) Voice5.IsChecked = true;
        _config.VoiceId = 5;
        _config.Save();
    }

    private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateLabels();
        if (_updatingUi) return;
        _engine.Dsp.VolumeGain = (float)(VolumeSlider.Value / 100.0);
        _config.VolumeGain = _engine.Dsp.VolumeGain;
        _config.Save();
    }

    private void PadGain_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingUi) return;
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
        EffectState.Text = _engine.Dsp.Bypass ? "ОРИГИНАЛ" : "АКТИВЕН";
        EffectBtn.Content = _engine.Dsp.Bypass ? "Вкл. эффект (T)" : "Выкл. эффект (T)";
    }

    private void ToggleMute()
    {
        _engine.Dsp.Muted = !_engine.Dsp.Muted;
        MuteBtn.Content = _engine.Dsp.Muted ? "Анмут (M)" : "Мут (M)";
    }

    private void ToggleMonitor()
    {
        _engine.SetMonitoring(!_engine.IsMonitoring);
        MonitorBtn.Content = _engine.IsMonitoring ? "Монитор: ВКЛ (L)" : "Монитор (L)";
    }

    // ---------- hotkeys ----------

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is >= Key.D1 and <= Key.D5)
        {
            int v = (int)e.Key - (int)Key.D1 + 1;
            CheckVoiceRadio(v);
            Voice_Checked(this, new RoutedEventArgs());
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
