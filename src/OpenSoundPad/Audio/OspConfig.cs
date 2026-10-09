using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenSoundPad.Audio;

/// <summary>
/// Настройки OSP. Новый путь %APPDATA%\OpenSoundPad\config.json,
/// автоматическая миграция со старого VoicehackTool.
/// </summary>
public sealed class OspConfig
{
    public string? InputDeviceId { get; set; }
    public string? OutputDeviceId { get; set; }
    public string? MonitorDeviceId { get; set; }
    public int VoiceId { get; set; } = 1;
    public float VolumeGain { get; set; } = 2.0f;
    public float PitchSemitones { get; set; } = -5.0f;
    public float Drive { get; set; } = 0.25f;
    public float BassBoostDb { get; set; } = 6.0f;
    public float RobotMod { get; set; } = 0.0f;
    public float PadGain { get; set; } = 1.0f;
    public float InputPregain { get; set; } = 2.2f;
    public float GateThreshold { get; set; } = 0.003f;
    public Dictionary<string, string> PadFiles { get; set; } = new();

    public static string GetFilePath()
    {
        string? appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) is string ad && ad.Length > 0
            ? ad : null;
        string dir = appdata != null
            ? Path.Combine(appdata, "OpenSoundPad")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".opensoundpad");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "config.json");
    }

    private static IEnumerable<string> LegacyPaths()
    {
        string? appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) is string ad && ad.Length > 0
            ? ad : null;
        if (appdata != null)
            yield return Path.Combine(appdata, "VoicehackTool", "config.json");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".voicehacktool", "config.json");
    }

    public static OspConfig Load()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            try
            {
                var cfg = JsonSerializer.Deserialize<OspConfig>(File.ReadAllText(path));
                if (cfg != null) return cfg;
            }
            catch { /* битый конфиг — пробуем legacy */ }
        }
        foreach (string legacy in LegacyPaths())
        {
            try
            {
                if (!File.Exists(legacy)) continue;
                string json = File.ReadAllText(legacy);
                // Старый формат python-конфига: маппим известные поля
                var cfg = new OspConfig();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("voice_id", out var v)) cfg.VoiceId = v.GetInt32();
                if (root.TryGetProperty("volume_gain", out var g)) cfg.VolumeGain = (float)g.GetDouble();
                if (root.TryGetProperty("custom_voice_params", out var cp))
                {
                    if (cp.TryGetProperty("pitch_semitones", out var p)) cfg.PitchSemitones = (float)p.GetDouble();
                    if (cp.TryGetProperty("drive", out var d)) cfg.Drive = (float)d.GetDouble();
                    if (cp.TryGetProperty("bass_boost_db", out var b)) cfg.BassBoostDb = (float)b.GetDouble();
                    if (cp.TryGetProperty("robot_mod", out var r)) cfg.RobotMod = (float)r.GetDouble();
                }
                cfg.Save();
                return cfg;
            }
            catch { }
        }
        return new OspConfig();
    }

    public void Save()
    {
        try
        {
            string path = GetFilePath();
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(this, opts));
        }
        catch { }
    }
}
