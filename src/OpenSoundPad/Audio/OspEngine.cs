using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace OpenSoundPad.Audio;

public sealed record OspDevice(string Id, string Name, DataFlow Flow);

/// <summary>
/// Звуковой движок: WASAPI захват микрофона -> OspDsp + SoundPad -> WASAPI вывод
/// в виртуальный кабель + мониторинг в наушники. Только визуальный режим.
/// </summary>
public sealed class OspEngine : IDisposable
{
    private static readonly string[] JunkKeywords =
        ["первичный", "переназначение", "стерео микшер", "stereo mix", "mapper", "primary sound", "microsoft sound mapper"];
    private static readonly string[] VirtualKeywords =
        ["animaze", "cable", "voicemod", "virtual", "line in"];

    public OspDsp Dsp { get; private set; } = new OspDsp(48000);
    public SoundPadBank Pads { get; } = new();

    public float InputLevel { get; private set; }
    public float OutputLevel { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsMonitoring { get; set; }

    public string? InputDeviceId { get; private set; }
    public string? OutputDeviceId { get; private set; }
    public string? MonitorDeviceId { get; private set; }

    private WasapiCapture? _capture;
    private WasapiOut? _cableOut;
    private WasapiOut? _monitorOut;
    private BufferedWaveProvider? _cableBuffer;
    private BufferedWaveProvider? _monitorBuffer;

    private float[] _mono = Array.Empty<float>();
    private float[] _proc = Array.Empty<float>();
    private readonly object _sync = new();

    public static List<OspDevice> GetInputMicrophones() => GetDevices(DataFlow.Capture, physicalOnly: true);
    public static List<OspDevice> GetVirtualCables() => GetDevices(DataFlow.Render, virtualOnly: true);
    public static List<OspDevice> GetOutputDevices() => GetDevices(DataFlow.Render, physicalOnly: true);

    private static List<OspDevice> GetDevices(DataFlow flow, bool physicalOnly = false, bool virtualOnly = false)
    {
        var result = new List<OspDevice>();
        try
        {
            using var e = new MMDeviceEnumerator();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in e.EnumerateAudioEndPoints(flow, DeviceState.Active))
            {
                string name = d.FriendlyName;
                string lower = name.ToLowerInvariant();
                if (JunkKeywords.Any(k => lower.Contains(k))) continue;
                bool isVirt = VirtualKeywords.Any(k => lower.Contains(k));
                if (physicalOnly && isVirt) continue;
                if (virtualOnly && !isVirt) continue;
                string baseName = name.Split('[')[0].Trim();
                if (!seen.Add(baseName)) continue;
                result.Add(new OspDevice(d.ID, baseName, flow));
            }
        }
        catch { }
        return result;
    }

    public static string FindVirtualMicName(string outName)
    {
        string l = outName.ToLowerInvariant();
        if (l.Contains("animaze")) return "Microphone (Animaze Virtual Audio)";
        if (l.Contains("cable") || l.Contains("vb-audio")) return "CABLE Output (VB-Audio Virtual Cable)";
        if (l.Contains("voicemod")) return "Voicemod Virtual Audio Device";
        return $"{outName} (виртуальный микрофон)";
    }

    public void SetDevices(string? inputId, string? outputId, string? monitorId)
    {
        InputDeviceId = inputId;
        OutputDeviceId = outputId;
        MonitorDeviceId = monitorId;
    }

    public void Start()
    {
        lock (_sync)
        {
            StopLocked();
            if (InputDeviceId == null || OutputDeviceId == null)
                throw new InvalidOperationException("Не выбраны устройства ввода/вывода.");

            using var e = new MMDeviceEnumerator();
            var inDev = e.GetDevice(InputDeviceId);
            var outDev = e.GetDevice(OutputDeviceId);

            int rate = inDev.AudioClient?.MixFormat?.SampleRate ?? 48000;
            if (rate is < 8000 or > 192000) rate = 48000;
            Dsp = new OspDsp(rate);
            ApplySavedVoice();

            int ch = Math.Max(1, inDev.AudioClient?.MixFormat?.Channels ?? 1);

            _capture = new WasapiCapture(inDev, false, 20);
            var cableFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, 2);
            _cableBuffer = new BufferedWaveProvider(cableFormat)
            {
                DiscardOnBufferOverflow = true,
                BufferDuration = TimeSpan.FromMilliseconds(500),
            };
            _cableOut = new WasapiOut(outDev, AudioClientShareMode.Shared, false, 40);
            _cableOut.Init(_cableBuffer);
            _cableOut.PlaybackStopped += (_, _) => { };

            if (IsMonitoring && MonitorDeviceId != null)
                StartMonitorLocked(rate);

            _capture.DataAvailable += OnCaptureData;
            _capture.RecordingStopped += (_, _) => { };
            _capture.StartRecording();
            _cableOut.Play();
            IsRunning = true;
        }
    }

    private void ApplySavedVoice()
    {
        // голос/параметры подхватит MainWindow из OspConfig после Start
    }

    private void StartMonitorLocked(int rate)
    {
        try
        {
            if (MonitorDeviceId == null) return;
            using var e = new MMDeviceEnumerator();
            var monDev = e.GetDevice(MonitorDeviceId);
            var fmt = WaveFormat.CreateIeeeFloatWaveFormat(rate, 2);
            _monitorBuffer = new BufferedWaveProvider(fmt)
            {
                DiscardOnBufferOverflow = true,
                BufferDuration = TimeSpan.FromMilliseconds(500),
            };
            _monitorOut?.Dispose();
            _monitorOut = new WasapiOut(monDev, AudioClientShareMode.Shared, false, 40);
            _monitorOut.Init(_monitorBuffer);
            _monitorOut.Play();
        }
        catch { _monitorOut = null; }
    }

    public void SetMonitoring(bool on)
    {
        lock (_sync)
        {
            IsMonitoring = on;
            if (!IsRunning) return;
            if (on && _monitorOut == null)
            {
                int rate = Dsp.SampleRate;
                StartMonitorLocked(rate);
            }
            else if (!on)
            {
                try { _monitorOut?.Stop(); } catch { }
                _monitorOut?.Dispose();
                _monitorOut = null;
                _monitorBuffer = null;
            }
        }
    }

    private void OnCaptureData(object? sender, WaveInEventArgs a)
    {
        try
        {
            var fmt = _capture?.WaveFormat;
            if (fmt == null) return;
            int bytesPerSample = fmt.BitsPerSample / 8;
            int channels = Math.Max(1, fmt.Channels);
            int frames = a.BytesRecorded / (bytesPerSample * channels);
            if (frames <= 0) return;

            if (_mono.Length < frames)
            {
                _mono = new float[frames];
                _proc = new float[frames];
            }

            // Берём первый канал, IEEE float или 16-bit PCM
            if (fmt.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                for (int i = 0; i < frames; i++)
                {
                    float s = BitConverter.ToSingle(a.Buffer, i * channels * 4);
                    _mono[i] = s;
                }
            }
            else if (fmt.BitsPerSample == 16)
            {
                for (int i = 0; i < frames; i++)
                {
                    short v = BitConverter.ToInt16(a.Buffer, i * channels * 2);
                    _mono[i] = v / 32768f;
                }
            }
            else return;

            double inSum = 0;
            for (int i = 0; i < frames; i++) inSum += _mono[i] * _mono[i];
            InputLevel = (float)Math.Min(1.0, Math.Sqrt(inSum / frames) * 4.0);

            Dsp.ProcessBlock(_mono, _proc, frames);
            Pads.MixInto(_proc, frames);

            double outSum = 0;
            for (int i = 0; i < frames; i++) outSum += _proc[i] * _proc[i];
            OutputLevel = (float)Math.Min(1.0, Math.Sqrt(outSum / frames) * 4.0);

            // Стерео interleaved для кабеля/монитора
            byte[] stereo = new byte[frames * 2 * 4];
            for (int i = 0; i < frames; i++)
            {
                float s = Math.Clamp(_proc[i], -0.98f, 0.98f);
                byte[] b = BitConverter.GetBytes(s);
                Buffer.BlockCopy(b, 0, stereo, i * 8, 4);
                Buffer.BlockCopy(b, 0, stereo, i * 8 + 4, 4);
            }
            _cableBuffer?.AddSamples(stereo, 0, stereo.Length);
            if (IsMonitoring) _monitorBuffer?.AddSamples(stereo, 0, stereo.Length);
        }
        catch { /* аудиоколбэк не должен кидать */ }
    }

    public void Stop()
    {
        lock (_sync) StopLocked();
    }

    private void StopLocked()
    {
        IsRunning = false;
        try { _capture?.StopRecording(); } catch { }
        if (_capture != null) { _capture.DataAvailable -= OnCaptureData; _capture.Dispose(); _capture = null; }
        try { _cableOut?.Stop(); } catch { }
        _cableOut?.Dispose(); _cableOut = null; _cableBuffer = null;
        try { _monitorOut?.Stop(); } catch { }
        _monitorOut?.Dispose(); _monitorOut = null; _monitorBuffer = null;
        InputLevel = OutputLevel = 0;
    }

    public void Dispose() => Stop();
}
