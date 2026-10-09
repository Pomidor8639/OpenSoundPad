using System.Diagnostics;
using System.IO;
using System.Windows;
using OpenSoundPad.Audio;

namespace OpenSoundPad;

public partial class SettingsWindow : Window
{
    private readonly OspConfig _config;

    public SettingsWindow(OspConfig config)
    {
        _config = config;
        InitializeComponent();
        PregainSlider.Value = _config.InputPregain * 100;
        GateSlider.Value = _config.GateThreshold * 10000;
        ConfigPathText.Text = OspConfig.GetFilePath();
        PregainSlider.ValueChanged += (_, _) =>
            PregainLabel.Text = $"Предусиление входа: x{(PregainSlider.Value / 100):0.0}";
        GateSlider.ValueChanged += (_, _) =>
            GateLabel.Text = $"Порог шумоподавителя: {(GateSlider.Value / 10000):0.000}";
        PregainLabel.Text = $"Предусиление входа: x{(PregainSlider.Value / 100):0.0}";
        GateLabel.Text = $"Порог шумоподавителя: {(GateSlider.Value / 10000):0.000}";
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
        if (MessageBox.Show("Сбросить все настройки (голоса, громкости, устройства)?\nФайлы падов сохранятся.",
                "OSP", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        var pads = _config.PadFiles;
        var fresh = new OspConfig { PadFiles = pads };
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
        PregainSlider.Value = _config.InputPregain * 100;
        GateSlider.Value = _config.GateThreshold * 10000;
        _config.Save();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        _config.InputPregain = (float)(PregainSlider.Value / 100.0);
        _config.GateThreshold = (float)(GateSlider.Value / 10000.0);
        _config.Save();
        DialogResult = true;
    }
}
