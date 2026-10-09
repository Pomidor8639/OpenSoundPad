using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace OpenSoundPad.Audio;

/// <summary>
/// Саундпад: динамический банк сэмплов с полифонией.
/// Сэмплы декодируются во float-mono 48k при добавлении.
/// Воспроизведение полифоническое без блокировок ввода/вывода.
/// </summary>
public sealed class SoundPadBank
{
    public float MasterGain = 1.0f;

    private readonly object _sync = new();
    private readonly List<Pad> _pads = new();
    private int _sampleRate = 48000;

    public int PadCount
    {
        get { lock (_sync) return _pads.Count; }
    }

    public void SetSampleRate(int rate) => _sampleRate = rate;

    public string? GetPadFile(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return null;
            return _pads[index].FilePath;
        }
    }

    public string GetPadName(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return $"Pad {index + 1}";
            var p = _pads[index];
            if (p.Samples == null) return $"Pad {index + 1}";
            return Path.GetFileNameWithoutExtension(p.FilePath ?? $"Pad {index + 1}");
        }
    }

    public float GetPadGain(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return 1.0f;
            return _pads[index].Gain;
        }
    }

    public void SetPadGain(int index, float gain)
    {
        lock (_sync)
        {
            if (index >= 0 && index < _pads.Count)
                _pads[index].Gain = Math.Clamp(gain, 0f, 2f);
        }
    }

    public TimeSpan? GetPadDuration(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return null;
            var s = _pads[index].Samples;
            if (s == null || _sampleRate <= 0) return null;
            return TimeSpan.FromSeconds((double)s.Length / _sampleRate);
        }
    }

    public int AddPad(string filePath)
    {
        var pad = DecodeFile(filePath);
        if (pad == null) return -1;
        lock (_sync)
        {
            _pads.Add(pad);
            return _pads.Count - 1;
        }
    }

    public bool LoadPad(int index, string filePath)
    {
        var pad = DecodeFile(filePath);
        if (pad == null) return false;
        lock (_sync)
        {
            if (index >= 0 && index < _pads.Count)
            {
                _pads[index] = pad;
                return true;
            }
            else if (index == _pads.Count)
            {
                _pads.Add(pad);
                return true;
            }
            return false;
        }
    }

    public bool RemovePad(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return false;
            _pads.RemoveAt(index);
            return true;
        }
    }

    public void ClearPad(int index)
    {
        RemovePad(index);
    }

    public void ClearAll()
    {
        lock (_sync)
        {
            _pads.Clear();
        }
    }

    public void Trigger(int index, bool loop = false)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return;
            var p = _pads[index];
            if (p.Samples == null) return;
            if (loop)
            {
                p.Voices.Clear();
                p.Voices.Add(new Voice { Pos = 0, IsLooping = true });
            }
            else
            {
                if (p.Voices.Count >= 4) p.Voices.RemoveAt(0);
                p.Voices.Add(new Voice { Pos = 0, IsLooping = false });
            }
        }
    }

    public void StopLooping()
    {
        lock (_sync)
        {
            foreach (var p in _pads)
            {
                foreach (var v in p.Voices)
                    v.IsLooping = false;
            }
        }
    }

    public bool IsAnyLooping()
    {
        lock (_sync)
        {
            foreach (var p in _pads)
            {
                if (p.Voices.Exists(v => v.IsLooping)) return true;
            }
            return false;
        }
    }

    public bool IsPadLooping(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return false;
            return _pads[index].Voices.Exists(v => v.IsLooping);
        }
    }

    public bool HasPad(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return false;
            return _pads[index].Samples != null;
        }
    }

    public bool IsPadPlaying(int index)
    {
        lock (_sync)
        {
            if (index < 0 || index >= _pads.Count) return false;
            return _pads[index].Voices.Count > 0;
        }
    }

    public void StopPad(int index)
    {
        lock (_sync)
        {
            if (index >= 0 && index < _pads.Count)
                _pads[index].Voices.Clear();
        }
    }

    public void StopAll()
    {
        lock (_sync)
        {
            foreach (var p in _pads) p.Voices.Clear();
        }
    }

    public void MixInto(float[] buffer, int n)
    {
        if (MasterGain <= 0.001f) return;
        lock (_sync)
        {
            for (int pIdx = 0; pIdx < _pads.Count; pIdx++)
            {
                var p = _pads[pIdx];
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
                    if (voice.Pos >= s.Length)
                    {
                        if (voice.IsLooping)
                        {
                            voice.Pos = 0;
                        }
                        else
                        {
                            p.Voices.RemoveAt(v);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < n; i++) buffer[i] = Math.Clamp(buffer[i], -0.98f, 0.98f);
    }

    private Pad? DecodeFile(string filePath)
    {
        try
        {
            using var reader = new AudioFileReader(filePath);
            var resampler = new WdlResamplingSampleProvider(reader, _sampleRate);
            var mono = resampler.ToMono();
            var data = new List<float>(1 << 18);
            float[] buf = new float[8192];
            int read;
            while ((read = mono.Read(buf, 0, buf.Length)) > 0)
                for (int i = 0; i < read; i++) data.Add(Math.Clamp(buf[i], -1f, 1f));
            if (data.Count == 0) return null;
            return new Pad
            {
                Samples = data.ToArray(),
                FilePath = filePath
            };
        }
        catch
        {
            return null;
        }
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
        public bool IsLooping;
    }
}
