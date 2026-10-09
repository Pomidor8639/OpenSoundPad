using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace OpenSoundPad.Audio;

/// <summary>
/// Саундпад: банк сэмплов с полифонией. Сэмплы заранее декодируются во float-mono 48k,
/// в аудиоколбэке только MixInto без аллокаций/IO.
/// </summary>
public sealed class SoundPadBank
{
    public const int PadCount = 12;
    public float MasterGain = 1.0f;

    private readonly object _sync = new();
    private readonly Pad[] _pads = new Pad[PadCount];
    private int _sampleRate = 48000;

    public SoundPadBank()
    {
        for (int i = 0; i < PadCount; i++) _pads[i] = new Pad();
    }

    public void SetSampleRate(int rate) => _sampleRate = rate;

    public string? GetPadFile(int index) { lock (_sync) return _pads[index].FilePath; }
    public string GetPadName(int index)
    {
        lock (_sync)
        {
            var p = _pads[index];
            if (p.Samples == null) return $"Pad {index + 1}";
            return Path.GetFileNameWithoutExtension(p.FilePath ?? $"Pad {index + 1}");
        }
    }

    public float GetPadGain(int index) { lock (_sync) return _pads[index].Gain; }
    public void SetPadGain(int index, float gain) { lock (_sync) _pads[index].Gain = Math.Clamp(gain, 0f, 2f); }

    public TimeSpan? GetPadDuration(int index)
    {
        lock (_sync)
        {
            var s = _pads[index].Samples;
            if (s == null || _sampleRate <= 0) return null;
            return TimeSpan.FromSeconds((double)s.Length / _sampleRate);
        }
    }

    public bool LoadPad(int index, string filePath)
    {
        try
        {
            using var reader = new AudioFileReader(filePath);
            var resampler = new WdlResamplingSampleProvider(reader, _sampleRate);
            var mono = resampler.ToMono();
            var data = new List<float>(1 << 20);
            float[] buf = new float[8192];
            int read;
            while ((read = mono.Read(buf, 0, buf.Length)) > 0)
                for (int i = 0; i < read; i++) data.Add(Math.Clamp(buf[i], -1f, 1f));
            if (data.Count == 0) return false;
            lock (_sync)
            {
                _pads[index].Samples = data.ToArray();
                _pads[index].FilePath = filePath;
                _pads[index].Voices.Clear();
            }
            return true;
        }
        catch { return false; }
    }

    public void ClearPad(int index)
    {
        lock (_sync)
        {
            _pads[index].Samples = null;
            _pads[index].FilePath = null;
            _pads[index].Voices.Clear();
        }
    }

    public void Trigger(int index)
    {
        lock (_sync)
        {
            var p = _pads[index];
            if (p.Samples == null) return;
            // retrigger: максимум 4 войса на пад
            if (p.Voices.Count >= 4) p.Voices.RemoveAt(0);
            p.Voices.Add(new Voice { Pos = 0 });
        }
    }

    public void StopAll()
    {
        lock (_sync) foreach (var p in _pads) p.Voices.Clear();
    }

    public void MixInto(float[] buffer, int n)
    {
        if (MasterGain <= 0.001f) return;
        lock (_sync)
        {
            foreach (var p in _pads)
            {
                if (p.Samples == null || p.Voices.Count == 0 || p.Gain <= 0.001f) continue;
                float g = p.Gain * MasterGain;
                for (int v = p.Voices.Count - 1; v >= 0; v--)
                {
                    var voice = p.Voices[v];
                    float[] s = p.Samples;
                    int pos = voice.Pos;
                    int left = s.Length - pos;
                    int take = Math.Min(left, n);
                    for (int i = 0; i < take; i++) buffer[i] += s[pos + i] * g;
                    voice.Pos += take;
                    if (voice.Pos >= s.Length) p.Voices.RemoveAt(v);
                }
            }
        }
        for (int i = 0; i < n; i++) buffer[i] = Math.Clamp(buffer[i], -0.98f, 0.98f);
    }

    private sealed class Pad
    {
        public float[]? Samples;
        public string? FilePath;
        public float Gain = 1.0f;
        public readonly List<Voice> Voices = new();
    }

    private sealed class Voice
    {
        public int Pos;
    }
}
