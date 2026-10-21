using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace OpenSoundPad.Audio;

public sealed record OspDevice(string Id, string Name, DataFlow Flow);

public sealed class OspEngine : IDisposable
{
    private static readonly string[] JunkKeywords =
        ["первичный", "переназначение", "стерео микшер", "stereo mix", "mapper", "primary sound", "microsoft sound mapper"];

    public OspDsp Dsp { get; private set; } = new OspDsp(48000);
    public SoundPadBank Pads { get; } = new();

    public float InputLevel { get; private set; }
    public float OutputLevel { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsMonitoring { get; set; }

    public string? InputDeviceId { get; private set; }
    public string? MonitorDeviceId { get; private set; }

    private WasapiCapture? _capture;
    private WasapiOut? _monitorOut;
    private BufferedWaveProvider? _monitorBuffer;
    private readonly OspApoClient _apoClient = new();

    private float[] _mono = Array.Empty<float>();
    private float[] _proc = Array.Empty<float>();
    private readonly object _sync = new();

    public static List<OspDevice> GetInputMicrophones() => GetDevices(DataFlow.Capture);
    public static List<OspDevice> GetOutputDevices() => GetDevices(DataFlow.Render);

    private static List<OspDevice> GetDevices(DataFlow flow)
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
                string baseName = name.Split('[')[0].Trim();
                if (!seen.Add(baseName)) continue;
                result.Add(new OspDevice(d.ID, baseName, flow));
            }
        }
        catch { }
        return result;
    }

    public void SetDevices(string? inputId, string? monitorId)
    {
        InputDeviceId = inputId;
        MonitorDeviceId = monitorId;
    }

    public void SetDevices(string? inputId, string? outputId, string? monitorId)
    {
        SetDevices(inputId, monitorId);
    }

    public void Start()
    {
        lock (_sync)
        {
            StopLocked();
            if (InputDeviceId == null)
                throw new InvalidOperationException("Не выбран микрофон.");

            using var e = new MMDeviceEnumerator();
            var inDev = e.GetDevice(InputDeviceId);

            int rate = inDev.AudioClient?.MixFormat?.SampleRate ?? 48000;
            if (rate is < 8000 or > 192000) rate = 48000;
            Dsp = new OspDsp(rate);

            int ch = Math.Max(1, inDev.AudioClient?.MixFormat?.Channels ?? 1);

            _capture = new WasapiCapture(inDev, false, 20);

            if (IsMonitoring && MonitorDeviceId != null)
                StartMonitorLocked(rate);

            _capture.DataAvailable += OnCaptureData;
            _capture.RecordingStopped += (_, _) => { };
            _capture.StartRecording();
            _apoClient.SetActive(true);
            IsRunning = true;
        }
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

            uint mode = Dsp.Muted ? 3u : 2u;
            _apoClient.WriteSamples(_proc, 0, frames, mode, 1.0f);

            if (IsMonitoring && _monitorBuffer != null)
            {
                byte[] stereo = new byte[frames * 2 * 4];
                for (int i = 0; i < frames; i++)
                {
                    float s = Math.Clamp(_proc[i], -0.98f, 0.98f);
                    byte[] b = BitConverter.GetBytes(s);
                    Buffer.BlockCopy(b, 0, stereo, i * 8, 4);
                    Buffer.BlockCopy(b, 0, stereo, i * 8 + 4, 4);
                }
                _monitorBuffer.AddSamples(stereo, 0, stereo.Length);
            }
        }
        catch { }
    }

    public void Stop()
    {
        lock (_sync) StopLocked();
    }

    private void StopLocked()
    {
        IsRunning = false;
        _apoClient.SetActive(false);
        try { _capture?.StopRecording(); } catch { }
        if (_capture != null) { _capture.DataAvailable -= OnCaptureData; _capture.Dispose(); _capture = null; }
        try { _monitorOut?.Stop(); } catch { }
        _monitorOut?.Dispose(); _monitorOut = null; _monitorBuffer = null;
        InputLevel = OutputLevel = 0;
    }

    public void Dispose()
    {
        Stop();
        _apoClient.Dispose();
    }
}
