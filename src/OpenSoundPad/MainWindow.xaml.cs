using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using OpenSoundPad.Audio;

namespace OpenSoundPad;

public partial class MainWindow : Window
{
    private readonly OspEngine _engine = new();
    private readonly OspConfig _config;
    private readonly DispatcherTimer _vuTimer;
    private readonly ObservableCollection<PadRow> _padRows = new();
    private bool _updatingUi = true;
    private bool _initialized;

    public MainWindow() : this(OspConfig.Load())
    {
    }

    public MainWindow(OspConfig config)
    {
        InitializeComponent();
        StateChanged += MainWindow_StateChanged;
        _config = config;
        Loc.Lang = _config.Language;

        ApplyLanguage();
        RefreshVoicesList();
        RefreshPadList();
        UpdateDeviceDisplay();
        ApplyConfigToUi();

        // Восстановление режима (войсмод или саундпад)
        SetMode(_config.Mode == "soundpad" ? "soundpad" : "voicemod", saveConfig: false);

        _vuTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(45) };
        _vuTimer.Tick += (_, _) =>
        {
            float inLevel = _engine.InputLevel * 100;
            float outLevel = _engine.OutputLevel * 100;

            InputVu.Value = inLevel;
            OutputVu.Value = outLevel;
            PadOutputVu.Value = outLevel;

            int activeCount = 0;
            for (int i = 0; i < _padRows.Count; i++)
            {
                bool isPlaying = _engine.Pads.IsPadPlaying(i);
                if (isPlaying) activeCount++;
                if (_padRows[i].IsPlaying != isPlaying)
                    _padRows[i].IsPlaying = isPlaying;
            }

            ActivePadsCountText.Text = Loc.PlayingCount(activeCount);
        };
        _vuTimer.Start();

        KeyDown += OnKeyDown;
        Closed += (_, _) => { _vuTimer.Stop(); _engine.Stop(); _config.Save(); };
        _initialized = true;
        _updatingUi = false;
    }

    // ---------- переключение режимов (Войсмод / Саундпад) ----------

    private void ModeVoice_Click(object sender, RoutedEventArgs e) => SetMode("voicemod");
    private void ModePad_Click(object sender, RoutedEventArgs e) => SetMode("soundpad");

    private void SetMode(string mode, bool saveConfig = true)
    {
        bool isPad = mode == "soundpad";
        _config.Mode = isPad ? "soundpad" : "voicemod";
        if (saveConfig) _config.Save();

        ModeVoiceRadio.IsChecked = !isPad;
        ModePadRadio.IsChecked = isPad;

        VoiceModeGrid.Visibility = isPad ? Visibility.Collapsed : Visibility.Visible;
        SoundpadModeGrid.Visibility = isPad ? Visibility.Visible : Visibility.Collapsed;

        TbEffect.Visibility = isPad ? Visibility.Collapsed : Visibility.Visible;
    }

    // ---------- язык и интерфейс ----------

    private void ApplyLanguage()
    {
        FileMenu.Header = Loc.MenuFile;
        MiOpenFolder.Header = Loc.MenuOpenFolder;
        MiExit.Header = Loc.MenuExit;
        SettingsMenu.Header = Loc.MenuSettings;

        BtnMinimize.ToolTip = Loc.WinMinimize;
        BtnMaximize.ToolTip = WindowState == WindowState.Maximized ? Loc.WinRestore : Loc.WinMaximize;
        BtnClose.ToolTip = Loc.WinClose;

        ModeVoiceText.Text = Loc.ModeVoice;
        ModePadText.Text = Loc.ModePad;

        TbStartText.Text = Loc.TbStart;
        TbStopText.Text = Loc.TbStop;
        TbSettingsText.Text = Loc.MenuSettings + "…";
        UpdateToggleButtons();
        UpdateStatusBadge();

        // Войсмод панель
        VoicesGroup.Header = Loc.VoicePresets + " (1–5)";
        VoiceStudioGroup.Header = Loc.VoiceStudio + " (5)";
        LevelsGroup.Header = Loc.GrLevels;
        EffectLabel.Text = Loc.Effect;
        EffectState.Text = _engine.Dsp.Bypass ? Loc.EffectBypass : Loc.EffectActive;
        CustomHeader.Text = Loc.CustomVoice;
        CustomHintText.Text = Loc.CustomVoiceHint;
        VoiceHotkeyHint.Text = Loc.VoiceHotkeysHint;
        ResetVoiceText.Text = Loc.ResetVoiceParams;
        PitchKnob.Title = Loc.PitchTitle;
        DriveKnob.Title = Loc.DriveTitle;
        BassKnob.Title = Loc.BassTitle;
        RobotKnob.Title = Loc.RobotTitle;
        VolumeKnob.Title = Loc.VolumeTitle;

        // Саундпад панель
        PadsGroup.Header = $"{Loc.GrPads} ({_padRows.Count})";
        PadVolumeKnob.Title = Loc.PadVolumeTitle;
        StopPadsBtnText.Text = Loc.StopPads + " (Esc)";
        PadDropHint.Text = Loc.SoundpadHint;
        PadHotkeyHint.Text = Loc.PadHotkeysHint;
        AddPadBtnText.Text = Loc.AddSound;
        EmptyPadsText.Text = Loc.NoSoundsYet;

        ColNum.Header = Loc.ColNum;
        ColName.Header = Loc.ColName;
        ColDur.Header = Loc.ColDur;
        ColKey.Header = Loc.ColKey;
        CtxPlay.Header = Loc.PlayPad;
        CtxStop.Header = Loc.StopPad;
        CtxRepeat.Header = Loc.T("Зациклить / Повтор (R)", "Loop / Repeat (R)", "Wiederholen (R)", "Bucle / Repetir (R)", "Boucle / Répéter (R)");
        CtxRename.Header = Loc.RenameSound;
        CtxDelete.Header = Loc.RemoveSound + " (Del)";
        CtxClearAll.Header = Loc.ClearAllPads;
        PadsView.ToolTip = Loc.PlayTip;
        UpdateRepeatButtonUi();

        LevelInLabel.Text = Loc.LevelIn;
        LevelOutLabel.Text = Loc.LevelOut;
        ActiveDevHeader.Text = Loc.ActiveDevices + ":";
        DevMicTitle.Text = Loc.SetMic;
        DevCableTitle.Text = Loc.SetCable;
        DevMonTitle.Text = Loc.SetMonitor;
        OpenSettingsBtnText.Text = Loc.SetDevicesBtn;
        VirtMicHeader.Text = Loc.VirtMic;
        CableHint.Text = Loc.CableHint;

        if (!_engine.IsRunning) StatusText.Text = Loc.Stopped;
        UpdateLabels();
        RefreshVoicesList();
        RefreshPadList();
        UpdateDeviceDisplay();
    }

    private void UpdateStatusBadge()
    {
        if (_engine.IsRunning)
        {
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
            StatusBadgeText.Text = Loc.BadgeLive;
            StatusBadgeText.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(0x0C, 0x22, 0x17));
            StatusBadge.BorderBrush = new SolidColorBrush(Color.FromRgb(0x12, 0x4B, 0x32));
        }
        else
        {
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
            StatusBadgeText.Text = Loc.BadgeStopped;
            StatusBadgeText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(0x0A, 0x0A, 0x0A));
            StatusBadge.BorderBrush = new SolidColorBrush(Color.FromRgb(0x1C, 0x1C, 0x1C));
        }
    }

    private void UpdateToggleButtons()
    {
        TbEffectText.Text = _engine.Dsp.Bypass ? Loc.TbEffectOn : Loc.TbEffectOff;
        TbMuteText.Text = _engine.Dsp.Muted ? Loc.TbUnmute : Loc.TbMute;
        TbMonitorText.Text = _engine.IsMonitoring ? Loc.TbMonitorOn : Loc.TbMonitor;

        // Визуальная подсветка активных состояний (True Black стиль)
        TbMute.BorderBrush = _engine.Dsp.Muted
            ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
            : (Brush)FindResource("BorderDark");

        var monBrush = _engine.IsMonitoring
            ? new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81))
            : (Brush)FindResource("BorderDark");
        TbMonitor.BorderBrush = monBrush;
    }

    // ---------- голоса ----------

    private void RefreshVoicesList()
    {
        int sel = SelectedVoice() - 1;
        VoicesBox.Items.Clear();
        for (int i = 1; i <= 5; i++)
            VoicesBox.Items.Add($"{i} — {Loc.VoiceName(i)}");
        VoicesBox.SelectedIndex = Math.Clamp(sel, 0, 4);
        UpdateVoiceDescription();
    }

    private int SelectedVoice() => VoicesBox.SelectedIndex is >= 0 and <= 4 ? VoicesBox.SelectedIndex + 1 : 1;

    private void VoicesBox_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized || _updatingUi || VoicesBox.SelectedIndex < 0) return;
        int v = VoicesBox.SelectedIndex + 1;
        _engine.Dsp.SetVoice(v);
        _config.VoiceId = v;
        _config.Save();
        UpdateVoiceDescription();
        if (_engine.IsRunning && StatusText.Text.Contains('•'))
            StatusText.Text = StatusText.Text.Split('•')[0].TrimEnd() + $" • {Loc.VoiceName(v)}";
    }

    private void UpdateVoiceDescription()
    {
        int v = SelectedVoice();
        VoiceDescText.Text = Loc.VoiceDesc(v);
    }

    private void ResetCustomVoice_Click(object sender, RoutedEventArgs e)
    {
        _updatingUi = true;
        try
        {
            PitchKnob.Value = -5.0;
            DriveKnob.Value = 25.0;
            BassKnob.Value = 6.0;
            RobotKnob.Value = 0.0;
        }
        finally { _updatingUi = false; }

        PushParamsToDsp();
        VoicesBox.SelectedIndex = 4;
        _config.VoiceId = 5;
        _config.Save();
        UpdateVoiceDescription();
    }

    // ---------- пады (Soundpad) ----------

    public sealed class PadRow : INotifyPropertyChanged
    {
        private bool _isPlaying;
        private string _name = "";
        private string _duration = "—";

        public int Num { get; init; }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayingVisibility));
                }
            }
        }

        public Visibility PlayingVisibility => IsPlaying ? Visibility.Visible : Visibility.Collapsed;

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } }
        }

        public string Duration
        {
            get => _duration;
            set { if (_duration != value) { _duration = value; OnPropertyChanged(nameof(Duration)); } }
        }

        public string Key { get; init; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private void RefreshPadList()
    {
        int sel = PadsView.SelectedIndex;
        _padRows.Clear();
        for (int i = 0; i < _engine.Pads.PadCount; i++)
        {
            string name = _engine.Pads.GetPadName(i);
            var dur = _engine.Pads.GetPadDuration(i);
            string hotkey = i < 12 ? $"F{i + 1}" : $"№{i + 1}";
            _padRows.Add(new PadRow
            {
                Num = i + 1,
                IsPlaying = _engine.Pads.IsPadPlaying(i),
                Name = name,
                Duration = dur.HasValue ? $"{(int)dur.Value.TotalMinutes}:{dur.Value.Seconds:00}" : "—",
                Key = hotkey
            });
        }
        if (PadsView.ItemsSource != _padRows)
            PadsView.ItemsSource = _padRows;

        EmptyPadsPlaceholder.Visibility = _padRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        PadsGroup.Header = $"{Loc.GrPads} ({_padRows.Count})";

        if (sel >= 0 && sel < _padRows.Count) PadsView.SelectedIndex = sel;
        else if (_padRows.Count > 0 && sel >= _padRows.Count) PadsView.SelectedIndex = _padRows.Count - 1;
    }

    private int SelectedPad() => PadsView.SelectedIndex is >= 0 && PadsView.SelectedIndex < _engine.Pads.PadCount
        ? PadsView.SelectedIndex : -1;

    private void PadsView_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        int idx = SelectedPad();
        if (idx >= 0)
            _engine.Pads.Trigger(idx);
        else
            AddPadBtn_Click(sender, e);
    }

    private void PadsView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            int idx = SelectedPad();
            if (idx >= 0)
                _engine.Pads.Trigger(idx);
            e.Handled = true;
        }
        else if (e.Key is Key.Delete)
        {
            CtxClear_Click(sender, e);
            e.Handled = true;
        }
    }

    private void PadsView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var dep = e.OriginalSource as DependencyObject;
        while (dep != null && dep != PadsView)
        {
            if (dep is ListViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
                break;
            }
            dep = VisualTreeHelper.GetParent(dep);
        }
    }

    private void PadsView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        bool hasSelection = PadsView.SelectedIndex >= 0;
        bool hasPads = _padRows.Count > 0;

        CtxPlay.IsEnabled = hasSelection;
        CtxStop.IsEnabled = hasSelection && PadsView.SelectedIndex < _padRows.Count && _padRows[PadsView.SelectedIndex].IsPlaying;
        CtxRepeat.IsEnabled = hasSelection;
        CtxDelete.IsEnabled = hasSelection;
        CtxClearAll.IsEnabled = hasPads;
    }

    private void CtxPlay_Click(object sender, RoutedEventArgs e)
    {
        int idx = SelectedPad();
        if (idx >= 0) _engine.Pads.Trigger(idx);
    }

    private void CtxStop_Click(object sender, RoutedEventArgs e)
    {
        int idx = SelectedPad();
        if (idx >= 0)
        {
            _engine.Pads.StopPad(idx);
            if (!_engine.Pads.IsAnyLooping())
                SetRepeatState(false);
        }
    }

    // ---------- кнопка и логика повтора (Repeat / Loop) ----------

    private bool _isRepeatActive;
    private DateTime _repeatMouseDownTime;
    private bool _wasActiveBeforeMouseDown;

    private void RepeatPadBtn_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _repeatMouseDownTime = DateTime.UtcNow;
        _wasActiveBeforeMouseDown = _isRepeatActive;
        if (!_isRepeatActive)
        {
            SetRepeatState(true);
        }
    }

    private void RepeatPadBtn_MouseUp(object sender, MouseButtonEventArgs e)
    {
        double holdDuration = (DateTime.UtcNow - _repeatMouseDownTime).TotalMilliseconds;
        if (holdDuration > 350)
        {
            // Если кнопку зажали и удерживали более 350мс — отпускание выключает повтор
            SetRepeatState(false);
            e.Handled = true;
        }
    }

    private void RepeatPadBtn_Click(object sender, RoutedEventArgs e)
    {
        // Короткий клик: если до клика кнопка уже была активна, выключаем её
        if (_wasActiveBeforeMouseDown)
        {
            SetRepeatState(false);
        }
    }

    private void ToggleRepeat()
    {
        SetRepeatState(!_isRepeatActive);
    }

    private void CtxRepeat_Click(object sender, RoutedEventArgs e)
    {
        ToggleRepeat();
    }

    private void SetRepeatState(bool active)
    {
        _isRepeatActive = active;

        int sel = SelectedPad();
        if (_isRepeatActive)
        {
            if (sel >= 0 && sel < _engine.Pads.PadCount)
            {
                _engine.Pads.Trigger(sel, loop: true);
            }
            else if (_engine.Pads.PadCount > 0)
            {
                PadsView.SelectedIndex = 0;
                _engine.Pads.Trigger(0, loop: true);
            }
        }
        else
        {
            _engine.Pads.StopLooping();
        }

        UpdateRepeatButtonUi();
    }

    private void UpdateRepeatButtonUi()
    {
        if (RepeatPadBtn == null || RepeatPadBtnText == null || RepeatIcon == null) return;

        if (_isRepeatActive)
        {
            RepeatPadBtn.Background = (Brush)FindResource("AccentGreenDim");
            RepeatPadBtn.BorderBrush = (Brush)FindResource("AccentGreen");
            RepeatIcon.Style = (Style)FindResource("GreenIcon");
            RepeatPadBtnText.Foreground = (Brush)FindResource("AccentGreen");
            RepeatPadBtnText.Text = Loc.Lang == "en" ? "Loop: ON (R)" : "Повтор: ВКЛ (R)";
        }
        else
        {
            RepeatPadBtn.Background = (Brush)FindResource("BgInput");
            RepeatPadBtn.BorderBrush = (Brush)FindResource("BorderDark");
            RepeatIcon.Style = (Style)FindResource("WhiteIcon");
            RepeatPadBtnText.Foreground = (Brush)FindResource("TextPrimary");
            RepeatPadBtnText.Text = Loc.Lang == "en" ? "Loop (R)" : "Повтор (R)";
        }
    }

    private void AddPadBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Audio files|*.wav;*.mp3;*.aiff;*.wma;*.m4a;*.ogg;*.flac|All files|*.*",
            Title = Loc.AddSound,
            Multiselect = true,
        };
        if (dlg.ShowDialog() == true)
        {
            int added = 0;
            foreach (string file in dlg.FileNames)
            {
                if (File.Exists(file))
                {
                    string persistentFile = EnsurePersistentSoundFile(file);
                    if (_engine.Pads.AddPad(persistentFile) >= 0)
                    {
                        _config.PadList.Add(persistentFile);
                        added++;
                    }
                }
            }
            if (added > 0)
            {
                _config.Save();
                RefreshPadList();
                PadsView.SelectedIndex = _padRows.Count - 1;
            }
            else MessageBox.Show(Loc.LoadFailed, "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CtxRename_Click(object sender, RoutedEventArgs e)
    {
        int idx = SelectedPad();
        if (idx < 0 || idx >= _engine.Pads.PadCount) return;

        string currentName = _engine.Pads.GetPadName(idx);
        var renameWin = new RenameWindow(currentName) { Owner = this };
        if (renameWin.ShowDialog() == true)
        {
            string newName = renameWin.ResultName;
            _engine.Pads.SetPadName(idx, newName);

            string? file = _engine.Pads.GetPadFile(idx);
            if (!string.IsNullOrEmpty(file))
            {
                _config.PadTitles[file] = newName;
                _config.PadTitles[Path.GetFileName(file)] = newName;
            }
            _config.Save();
            RefreshPadList();
        }
    }

    private void CtxClear_Click(object sender, RoutedEventArgs e)
    {
        int idx = SelectedPad();
        if (idx >= 0)
        {
            _engine.Pads.RemovePad(idx);
            if (idx < _config.PadList.Count)
                _config.PadList.RemoveAt(idx);
            _config.Save();
            RefreshPadList();
        }
    }

    private void ClearAllPads_Click(object sender, RoutedEventArgs e)
    {
        if (_engine.Pads.PadCount == 0) return;
        if (MessageBox.Show(Loc.ClearAllAsk, "OSP", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _engine.Pads.ClearAll();
        _config.PadList.Clear();
        _config.Save();
        RefreshPadList();
    }

    private void PadsView_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void PadsView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                int added = 0;
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string persistentFile = EnsurePersistentSoundFile(file);
                        if (_engine.Pads.AddPad(persistentFile) >= 0)
                        {
                            _config.PadList.Add(persistentFile);
                            added++;
                        }
                    }
                }
                if (added > 0)
                {
                    _config.Save();
                    RefreshPadList();
                    PadsView.SelectedIndex = _padRows.Count - 1;
                }
            }
        }
    }

    private static string EnsurePersistentSoundFile(string sourceFile)
    {
        try
        {
            string appDir = Path.GetDirectoryName(OspConfig.GetFilePath()) ?? "";
            string soundsDir = Path.Combine(appDir, "sounds");
            Directory.CreateDirectory(soundsDir);

            string fullSource = Path.GetFullPath(sourceFile);
            if (fullSource.StartsWith(soundsDir, StringComparison.OrdinalIgnoreCase))
                return fullSource;

            string fileName = Path.GetFileName(sourceFile);
            string destFile = Path.Combine(soundsDir, fileName);

            if (File.Exists(destFile))
            {
                var fiSrc = new FileInfo(sourceFile);
                var fiDst = new FileInfo(destFile);
                if (fiSrc.Length != fiDst.Length)
                {
                    string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    destFile = Path.Combine(soundsDir, $"{nameNoExt}_{Guid.NewGuid():N}{ext}");
                }
            }

            if (!File.Exists(destFile))
            {
                File.Copy(sourceFile, destFile, true);
            }

            return destFile;
        }
        catch
        {
            return sourceFile;
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    // ---------- устройства ----------

    private static string? FindDeviceName(IEnumerable<OspDevice> list, string? id)
    {
        foreach (var d in list)
            if (d.Id == id) return d.Name;
        foreach (var d in list) return d.Name;
        return null;
    }

    private void UpdateDeviceDisplay()
    {
        string? inDev = FindDeviceName(OspEngine.GetInputMicrophones(), _config.InputDeviceId);
        string? cableDev = FindDeviceName(OspEngine.GetVirtualCables(), _config.OutputDeviceId);
        string? monDev = FindDeviceName(OspEngine.GetOutputDevices(), _config.MonitorDeviceId);

        InputDevText.Text = inDev ?? Loc.NotSelected;
        CableDevText.Text = cableDev ?? Loc.NotSelected;
        CableDevText2.Text = cableDev ?? Loc.NotSelected;
        MonitorDevText.Text = monDev ?? Loc.MonitorOff;

        string virtText = cableDev != null
            ? OspEngine.FindVirtualMicName(cableDev)
            : Loc.NoCable;
        VirtMicText.Text = virtText;
        VirtMicText2.Text = virtText;
    }

    // ---------- меню и тулбар ----------

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

    // ---------- синхронизация конфига ----------

    private void ApplyConfigToUi()
    {
        _updatingUi = true;
        try
        {
            VolumeKnob.Value = Math.Clamp(_config.VolumeGain * 100, 50, 400);
            PitchKnob.Value = _config.PitchSemitones;
            DriveKnob.Value = _config.Drive * 100;
            BassKnob.Value = _config.BassBoostDb;
            RobotKnob.Value = _config.RobotMod * 100;
            PadVolumeKnob.Value = _config.PadGain * 100;
            _engine.Pads.MasterGain = _config.PadGain;
            _engine.Pads.SetSampleRate(48000);
            _engine.Pads.ClearAll();

            string appDir = Path.GetDirectoryName(OspConfig.GetFilePath()) ?? "";
            string soundsDir = Path.Combine(appDir, "sounds");
            Directory.CreateDirectory(soundsDir);

            for (int i = 0; i < _config.PadList.Count; i++)
            {
                string file = _config.PadList[i];
                if (!File.Exists(file))
                {
                    string candidate = Path.Combine(soundsDir, Path.GetFileName(file));
                    if (File.Exists(candidate))
                    {
                        _config.PadList[i] = candidate;
                        file = candidate;
                    }
                }
                else
                {
                    string persistent = EnsurePersistentSoundFile(file);
                    if (persistent != file)
                    {
                        _config.PadList[i] = persistent;
                        file = persistent;
                    }
                }

                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    int pIdx = _engine.Pads.AddPad(file);
                    if (pIdx >= 0)
                    {
                        if (_config.PadTitles.TryGetValue(file, out string? customTitle) ||
                            _config.PadTitles.TryGetValue(Path.GetFileName(file), out customTitle))
                        {
                            _engine.Pads.SetPadName(pIdx, customTitle);
                        }
                    }
                }
            }
            VoicesBox.SelectedIndex = Math.Clamp(_config.VoiceId - 1, 0, 4);
            RefreshPadList();
        }
        finally { _updatingUi = false; }
    }

    private void UpdateLabels()
    {
    }

    private void PushParamsToDsp()
    {
        _engine.Dsp.SetCustomParams(
            (float)PitchKnob.Value,
            (float)(DriveKnob.Value / 100.0),
            (float)BassKnob.Value,
            (float)(RobotKnob.Value / 100.0));
        _config.PitchSemitones = (float)PitchKnob.Value;
        _config.Drive = (float)(DriveKnob.Value / 100.0);
        _config.BassBoostDb = (float)BassKnob.Value;
        _config.RobotMod = (float)(RobotKnob.Value / 100.0);
    }

    // ---------- звуковой движок ----------

    private void StartBtn_Click(object sender, RoutedEventArgs e) => StartEngine();
    private void StopBtn_Click(object sender, RoutedEventArgs e) => StopEngine();

    private void StartEngine()
    {
        try
        {
            if (_config.InputDeviceId == null || _config.OutputDeviceId == null)
            {
                var inList = OspEngine.GetInputMicrophones();
                var outList = OspEngine.GetVirtualCables();
                if (_config.InputDeviceId == null && inList.Count > 0)
                    _config.InputDeviceId = inList[0].Id;
                if (_config.OutputDeviceId == null && outList.Count > 0)
                    _config.OutputDeviceId = outList[0].Id;

                if (_config.InputDeviceId == null || _config.OutputDeviceId == null)
                {
                    MessageBox.Show(Loc.NeedDevices, "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
                    OpenSettings();
                    if (_config.InputDeviceId == null || _config.OutputDeviceId == null) return;
                }
            }
            _engine.SetDevices(_config.InputDeviceId, _config.OutputDeviceId, _config.MonitorDeviceId);

            _engine.Dsp.VolumeGain = (float)(VolumeKnob.Value / 100.0);
            _engine.Dsp.InputPregain = _config.InputPregain;
            _engine.Dsp.GateThreshold = _config.GateThreshold;
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Pads.SetSampleRate(_engine.Dsp.SampleRate);
            _engine.Pads.MasterGain = (float)(PadVolumeKnob.Value / 100.0);
            _config.Save();

            _engine.Start();
            _engine.Dsp.SetVoice(SelectedVoice());
            PushParamsToDsp();
            _engine.Dsp.VolumeGain = (float)(VolumeKnob.Value / 100.0);
            _engine.Dsp.InputPregain = _config.InputPregain;
            _engine.Dsp.GateThreshold = _config.GateThreshold;

            string inName = FindDeviceName(OspEngine.GetInputMicrophones(), _config.InputDeviceId) ?? "?";
            string outName = FindDeviceName(OspEngine.GetVirtualCables(), _config.OutputDeviceId) ?? "?";
            TbStart.IsEnabled = false;
            TbStop.IsEnabled = true;
            StatusText.Text = Loc.Live(inName, outName, Loc.VoiceName(SelectedVoice()));
            UpdateDeviceDisplay();
            UpdateStatusBadge();
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
        UpdateStatusBadge();
    }

    // ---------- обработка параметров ----------

    private void KnobParam_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        PushParamsToDsp();
        if (VoicesBox.SelectedIndex != 4) VoicesBox.SelectedIndex = 4;
        _config.VoiceId = 5;
        _config.Save();
        UpdateVoiceDescription();
    }

    private void VolumeKnob_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        _engine.Dsp.VolumeGain = (float)(VolumeKnob.Value / 100.0);
        _config.VolumeGain = _engine.Dsp.VolumeGain;
        _config.Save();
    }

    private void PadVolumeKnob_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialized || _updatingUi) return;
        UpdateLabels();
        _engine.Pads.MasterGain = (float)(PadVolumeKnob.Value / 100.0);
        _config.PadGain = _engine.Pads.MasterGain;
        _config.Save();
    }

    private void EffectBtn_Click(object sender, RoutedEventArgs e) => ToggleEffect();
    private void MuteBtn_Click(object sender, RoutedEventArgs e) => ToggleMute();
    private void MonitorBtn_Click(object sender, RoutedEventArgs e) => ToggleMonitor();
    private void StopPadsBtn_Click(object sender, RoutedEventArgs e)
    {
        _engine.Pads.StopAll();
        SetRepeatState(false);
    }

    private void ToggleEffect()
    {
        _engine.Dsp.Bypass = !_engine.Dsp.Bypass;
        EffectState.Text = _engine.Dsp.Bypass ? Loc.EffectBypass : Loc.EffectActive;
        EffectState.Foreground = _engine.Dsp.Bypass
            ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
            : new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
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

    // ---------- горячие клавиши ----------

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // В режиме саундпада Esc глушит все сэмплы
        if (e.Key == Key.Escape)
        {
            _engine.Pads.StopAll();
            SetRepeatState(false);
            e.Handled = true;
            return;
        }

        // F1-F12: запуск сэмплов саундпада
        if (e.Key is >= Key.F1 and <= Key.F12)
        {
            int pIdx = (int)e.Key - (int)Key.F1;
            if (pIdx >= 0 && pIdx < _engine.Pads.PadCount)
            {
                _engine.Pads.Trigger(pIdx);
                e.Handled = true;
                return;
            }
        }

        if (e.Key is >= Key.D1 and <= Key.D5 && VoiceModeGrid.Visibility == Visibility.Visible)
        {
            VoicesBox.SelectedIndex = (int)e.Key - (int)Key.D1;
        }
        else switch (e.Key)
        {
            case Key.T: ToggleEffect(); break;
            case Key.M: ToggleMute(); break;
            case Key.L: ToggleMonitor(); break;
            case Key.R: ToggleRepeat(); break;
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        try
        {
            var helper = new WindowInteropHelper(this);
            int darkMode = 1;
            DwmSetWindowAttribute(helper.Handle, 20, ref darkMode, sizeof(int));
            DwmSetWindowAttribute(helper.Handle, 19, ref darkMode, sizeof(int));
        }
        catch
        {
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            if (FindResource("GeoWinRestore") is Geometry geo)
                MaximizePath.Data = geo;
            BtnMaximize.ToolTip = Loc.IsRu ? "Восстановить" : "Restore";
            MainRootGrid.Margin = new Thickness(7);
        }
        else
        {
            if (FindResource("GeoWinMax") is Geometry geo)
                MaximizePath.Data = geo;
            BtnMaximize.ToolTip = Loc.IsRu ? "Развернуть" : "Maximize";
            MainRootGrid.Margin = new Thickness(0);
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
