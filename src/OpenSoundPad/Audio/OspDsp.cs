using System;
using System.Collections.Generic;

namespace OpenSoundPad.Audio;

/// <summary>
/// OSP DSP core — порт dsp.py (OpenSoundPadDSP) 1:1.
/// Гранулярный питч-шифтинг, биквадные EQ, ламповый овердрайв (tanh),
/// нойз-гейт, кроссфейд при смене голоса. Ноль аллокаций в горячем цикле
/// сверх переиспользуемых буферов блока.
/// </summary>
public sealed class OspDsp
{
    public static readonly IReadOnlyDictionary<int, string> Voices = new Dictionary<int, string>
    {
        [1] = "Аноним",
        [2] = "Женский",
        [3] = "Ребенок",
        [4] = "Демон",
        [5] = "Пользовательский",
    };

    public int SampleRate { get; }

    public int CurrentVoice { get; private set; } = 1;
    private int _prevVoice = 1;
    private int _crossfadeRemaining;

    public float VolumeGain = 2.0f;
    public float InputPregain = 2.2f;

    public bool Bypass;
    public bool Muted;

    // Custom voice params (voice 5)
    public float CustomPitchSemitones = -5.0f;
    public float CustomDrive = 0.25f;
    public float CustomBassBoostDb = 6.0f;
    public float CustomRobotMod;

    // Delay line for granular pitch shifting
    private readonly int _maxGrain;
    private readonly int _bufferLen;
    private readonly float[] _delayBuffer;
    private int _writePos;

    // phase per voice: 1->[0,0], 2->[0], 3->[0], 4->[0,0], 5->[0,0]
    private readonly Dictionary<int, double[]> _phases = new()
    {
        [1] = new double[2],
        [2] = new double[1],
        [3] = new double[1],
        [4] = new double[2],
        [5] = new double[2],
    };

    // Filters (DF1 biquads + 1st-order section for 3rd-order LP)
    private Biquad _anonLp2;          // 2nd-order part of 3900 Hz LP
    private FirstOrderLp _anonLp1;    // 1st-order part of 3900 Hz LP
    private Biquad _bass;             // 135 Hz peaking +8 dB Q1.1
    private Biquad _throat;           // 1750 Hz peaking +4 dB Q1.3
    private Biquad _femHp;            // 170 Hz HP
    private Biquad _femEq;            // 2800 Hz peaking +3.5 dB Q1.2
    private Biquad _childHp;          // 230 Hz HP
    private Biquad _childEq;          // 3600 Hz peaking +4 dB Q1.2
    private Biquad _customBass;       // 140 Hz peaking, adjustable

    private double _growlPhase;

    // Noise gate
    public float GateThreshold = 0.003f;
    private float _gateGain;
    private const float GateAttack = 0.45f;
    private const float GateRelease = 0.04f;

    // Scratch buffers (reused, grown on demand)
    private float[] _scratchA = Array.Empty<float>();
    private float[] _scratchB = Array.Empty<float>();
    private float[] _scratchC = Array.Empty<float>();
    private float[] _scratchD = Array.Empty<float>();

    private readonly object _lock = new();

    public OspDsp(int sampleRate = 48000)
    {
        SampleRate = sampleRate;
        _maxGrain = (int)(0.080 * sampleRate);
        _bufferLen = _maxGrain * 8;
        _delayBuffer = new float[_bufferLen];

        _anonLp2 = Biquad.LowPass(3900.0, sampleRate, 1.0);
        _anonLp1 = FirstOrderLp.LowPass(3900.0, sampleRate);
        _bass = Biquad.Peaking(135.0, 8.0, 1.1, sampleRate);
        _throat = Biquad.Peaking(1750.0, 4.0, 1.3, sampleRate);
        _femHp = Biquad.HighPass(170.0, sampleRate);
        _femEq = Biquad.Peaking(2800.0, 3.5, 1.2, sampleRate);
        _childHp = Biquad.HighPass(230.0, sampleRate);
        _childEq = Biquad.Peaking(3600.0, 4.0, 1.2, sampleRate);
        UpdateCustomFilters();
    }

    public void SetCustomParams(float pitchSemitones, float drive, float bassBoostDb, float robotMod)
    {
        lock (_lock)
        {
            CustomPitchSemitones = Math.Clamp(pitchSemitones, -18f, 18f);
            CustomDrive = Math.Clamp(drive, 0f, 1f);
            CustomBassBoostDb = Math.Clamp(bassBoostDb, 0f, 15f);
            CustomRobotMod = Math.Clamp(robotMod, 0f, 1f);
            UpdateCustomFilters();
        }
    }

    private void UpdateCustomFilters()
    {
        float db = Math.Clamp(CustomBassBoostDb, 0f, 16f);
        _customBass = Biquad.Peaking(140.0, db, 1.1, SampleRate);
    }

    public void SetVoice(int voiceId)
    {
        lock (_lock)
        {
            if (Voices.ContainsKey(voiceId) && voiceId != CurrentVoice)
            {
                _prevVoice = CurrentVoice;
                CurrentVoice = voiceId;
                _crossfadeRemaining = 2;
            }
        }
    }

    public float AdjustVolume(float delta)
    {
        lock (_lock)
        {
            VolumeGain = Math.Clamp(VolumeGain + delta, 0.5f, 4.0f);
            return VolumeGain;
        }
    }

    private void EnsureScratch(int n)
    {
        if (_scratchA.Length < n)
        {
            _scratchA = new float[n];
            _scratchB = new float[n];
            _scratchC = new float[n];
            _scratchD = new float[n];
        }
    }

    // Гранулярный питч-шифтинг — точный порт _read_pitch_shifted
    private void ReadPitchShifted(float[] output, int n, double rate, int phaseIdx, int voiceId, double grainMs)
    {
        int bufLen = _bufferLen;
        int nGrain = Math.Max(64, (int)(grainMs / 1000.0 * SampleRate));
        double phase = _phases[voiceId][phaseIdx];
        double step = (1.0 - rate) / nGrain;

        for (int i = 0; i < n; i++)
        {
            int wPos = (_writePos + i) % bufLen;
            double p1 = (phase + i * step) % 1.0; if (p1 < 0) p1 += 1.0;
            double p2 = p1 + 0.5; if (p2 >= 1.0) p2 -= 1.0;

            double pos1 = wPos - p1 * nGrain;
            pos1 %= bufLen; if (pos1 < 0) pos1 += bufLen;
            double pos2 = wPos - p2 * nGrain;
            pos2 %= bufLen; if (pos2 < 0) pos2 += bufLen;

            double w1 = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * p1));
            double w2 = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * p2));

            int i1 = (int)pos1; double f1 = pos1 - i1;
            int i1n = i1 + 1; if (i1n >= bufLen) i1n -= bufLen;
            double s1 = (1.0 - f1) * _delayBuffer[i1] + f1 * _delayBuffer[i1n];

            int i2 = (int)pos2; double f2 = pos2 - i2;
            int i2n = i2 + 1; if (i2n >= bufLen) i2n -= bufLen;
            double s2 = (1.0 - f2) * _delayBuffer[i2] + f2 * _delayBuffer[i2n];

            output[i] = (float)(s1 * w1 + s2 * w2);
        }

        double np = phase + n * step;
        np %= 1.0; if (np < 0) np += 1.0;
        _phases[voiceId][phaseIdx] = np;
    }

    private static double SemitoneRate(double st) => Math.Pow(2.0, st / 12.0);

    private void RenderVoice(int voiceId, float[] output, int n)
    {
        switch (voiceId)
        {
            case 1: // Аноним
            {
                ReadPitchShifted(_scratchA, n, SemitoneRate(-7.0), 0, 1, 62.0);
                ReadPitchShifted(_scratchB, n, SemitoneRate(-13.5), 1, 1, 75.0);
                for (int i = 0; i < n; i++) _scratchC[i] = 0.65f * _scratchA[i] + 0.45f * _scratchB[i];
                _bass.ProcessInPlace(_scratchC, n);
                _throat.ProcessInPlace(_scratchC, n);
                _anonLp1.ProcessInPlace(_scratchC, n);
                _anonLp2.ProcessInPlace(_scratchC, n);
                double dt = 1.0 / SampleRate;
                for (int i = 0; i < n; i++)
                {
                    double t = i * dt;
                    double rasp = 0.88 + 0.12 * Math.Sin(2.0 * Math.PI * 32.0 * t + _growlPhase);
                    double x = _scratchC[i] * rasp;
                    output[i] = (float)(Math.Tanh(x * 2.0) + 0.15 * Math.Tanh(x * 4.0));
                }
                _growlPhase = (_growlPhase + 2.0 * Math.PI * 32.0 * n / SampleRate) % (2.0 * Math.PI);
                break;
            }
            case 2: // Женский
            {
                ReadPitchShifted(_scratchC, n, SemitoneRate(3.8), 0, 2, 52.0);
                _femHp.ProcessInPlace(_scratchC, n);
                _femEq.ProcessInPlace(_scratchC, n);
                for (int i = 0; i < n; i++) output[i] = (float)(Math.Tanh(_scratchC[i] * 1.8) * 1.45);
                break;
            }
            case 3: // Ребенок
            {
                ReadPitchShifted(_scratchC, n, SemitoneRate(6.0), 0, 3, 44.0);
                _childHp.ProcessInPlace(_scratchC, n);
                _childEq.ProcessInPlace(_scratchC, n);
                for (int i = 0; i < n; i++) output[i] = (float)(Math.Tanh(_scratchC[i] * 1.6) * 1.40);
                break;
            }
            case 4: // Демон
            {
                ReadPitchShifted(_scratchA, n, SemitoneRate(-9.0), 0, 4, 68.0);
                ReadPitchShifted(_scratchB, n, SemitoneRate(-15.5), 1, 4, 80.0);
                for (int i = 0; i < n; i++) _scratchC[i] = 0.58f * _scratchA[i] + 0.55f * _scratchB[i];
                _bass.ProcessInPlace(_scratchC, n);
                for (int i = 0; i < n; i++)
                {
                    double x = _scratchC[i];
                    output[i] = (float)(Math.Tanh(x * 2.6) + 0.22 * Math.Tanh(x * 5.5));
                }
                break;
            }
            case 5: // Пользовательский
            {
                float pitch, drive, robot, bassDb;
                lock (_lock)
                {
                    pitch = CustomPitchSemitones; drive = CustomDrive;
                    robot = CustomRobotMod; bassDb = CustomBassBoostDb;
                }
                double rate = SemitoneRate(pitch);
                double grainMs = pitch > 0 ? 46.0 : 60.0 + Math.Min(20.0, Math.Abs(pitch) * 1.5);
                ReadPitchShifted(_scratchC, n, rate, 0, 5, grainMs);

                if (pitch < -6.0)
                {
                    double subRate = SemitoneRate(pitch - 6.0);
                    ReadPitchShifted(_scratchD, n, subRate, 1, 5, 76.0);
                    for (int i = 0; i < n; i++) _scratchC[i] = 0.72f * _scratchC[i] + 0.38f * _scratchD[i];
                }
                if (bassDb > 0.5f)
                {
                    Biquad cb;
                    lock (_lock) { cb = _customBass; }
                    cb.ProcessInPlace(_scratchC, n);
                    lock (_lock) { _customBass = cb; }
                }
                if (robot > 0.02f)
                {
                    double dt = 1.0 / SampleRate;
                    for (int i = 0; i < n; i++)
                    {
                        double t = i * dt;
                        double mod = (1.0 - robot * 0.45) + robot * 0.45 * Math.Sin(2.0 * Math.PI * 40.0 * t);
                        _scratchC[i] = (float)(_scratchC[i] * mod);
                    }
                }
                if (drive > 0.01f)
                {
                    double mult = 1.0 + drive * 3.5;
                    double post = 1.0 + drive * 0.35;
                    for (int i = 0; i < n; i++) _scratchC[i] = (float)(Math.Tanh(_scratchC[i] * mult) * post);
                }
                Array.Copy(_scratchC, output, n);
                break;
            }
            default:
                Array.Clear(output, 0, n);
                break;
        }
    }

    /// <summary>Обработка блока. input и output могут совпадать. Возвращает output.</summary>
    public float[] ProcessBlock(float[] input, float[] output, int n)
    {
        bool muted, bypass;
        float vol;
        lock (_lock) { muted = Muted; bypass = Bypass; vol = VolumeGain; }

        if (muted)
        {
            Array.Clear(output, 0, n);
            return output;
        }
        if (bypass)
        {
            for (int i = 0; i < n; i++)
            {
                float v = input[i] * vol;
                output[i] = Math.Clamp(v, -0.98f, 0.98f);
            }
            return output;
        }

        // Pregain + gate
        double sum = 0;
        for (int i = 0; i < n; i++) { double v = input[i] * InputPregain; sum += v * v; }
        double rms = Math.Sqrt(sum / Math.Max(1, n)) + 1e-9;
        float target = rms > GateThreshold ? 1f : 0f;
        float alpha = target > _gateGain ? GateAttack : GateRelease;
        _gateGain += alpha * (target - _gateGain);

        if (_gateGain < 0.01f)
        {
            Array.Clear(output, 0, n);
            return output;
        }

        EnsureScratch(n);

        // Push boosted into delay line
        for (int i = 0; i < n; i++)
        {
            int idx = _writePos + i;
            if (idx >= _bufferLen) idx -= _bufferLen;
            _delayBuffer[idx] = input[i] * InputPregain;
        }

        int cur, prev, xf;
        lock (_lock) { cur = CurrentVoice; prev = _prevVoice; xf = _crossfadeRemaining; }

        RenderVoice(cur, output, n);

        if (xf > 0)
        {
            RenderVoice(prev, _scratchA, n);
            for (int i = 0; i < n; i++)
            {
                float fade = n == 1 ? 1f : (float)i / (n - 1);
                output[i] = (1f - fade) * _scratchA[i] + fade * output[i];
            }
            lock (_lock) { _crossfadeRemaining--; }
        }

        _writePos += n;
        if (_writePos >= _bufferLen) _writePos -= _bufferLen;

        float g = vol * _gateGain;
        for (int i = 0; i < n; i++) output[i] = Math.Clamp(output[i] * g, -0.98f, 0.98f);
        return output;
    }

    // ---- filter primitives ----

    private struct Biquad
    {
        public double B0, B1, B2, A1, A2;
        public double Z1, Z2; // DF1 state

        public void ProcessInPlace(float[] data, int n)
        {
            double z1 = Z1, z2 = Z2;
            double b0 = B0, b1 = B1, b2 = B2, a1 = A1, a2 = A2;
            for (int i = 0; i < n; i++)
            {
                double x = data[i];
                double y = b0 * x + z1;
                z1 = b1 * x - a1 * y + z2;
                z2 = b2 * x - a2 * y;
                data[i] = (float)y;
            }
            Z1 = z1; Z2 = z2;
        }

        public static Biquad Peaking(double freq, double gainDb, double q, double fs)
        {
            double w0 = 2.0 * Math.PI * freq / fs;
            double alpha = Math.Sin(w0) / (2.0 * q);
            double ag = Math.Pow(10.0, gainDb / 40.0);
            double b0 = 1.0 + alpha * ag, b1 = -2.0 * Math.Cos(w0), b2 = 1.0 - alpha * ag;
            double a0 = 1.0 + alpha / ag, a1 = -2.0 * Math.Cos(w0), a2 = 1.0 - alpha / ag;
            return new Biquad { B0 = b0 / a0, B1 = b1 / a0, B2 = b2 / a0, A1 = a1 / a0, A2 = a2 / a0 };
        }

        public static Biquad LowPass(double freq, double fs, double q)
        {
            double w0 = 2.0 * Math.PI * freq / fs;
            double alpha = Math.Sin(w0) / (2.0 * q);
            double cw = Math.Cos(w0);
            double b0 = (1.0 - cw) / 2.0, b1 = 1.0 - cw, b2 = (1.0 - cw) / 2.0;
            double a0 = 1.0 + alpha, a1 = -2.0 * cw, a2 = 1.0 - alpha;
            return new Biquad { B0 = b0 / a0, B1 = b1 / a0, B2 = b2 / a0, A1 = a1 / a0, A2 = a2 / a0 };
        }

        public static Biquad HighPass(double freq, double fs)
        {
            double q = 1.0 / Math.Sqrt(2.0);
            double w0 = 2.0 * Math.PI * freq / fs;
            double alpha = Math.Sin(w0) / (2.0 * q);
            double cw = Math.Cos(w0);
            double b0 = (1.0 + cw) / 2.0, b1 = -(1.0 + cw), b2 = (1.0 + cw) / 2.0;
            double a0 = 1.0 + alpha, a1 = -2.0 * cw, a2 = 1.0 - alpha;
            return new Biquad { B0 = b0 / a0, B1 = b1 / a0, B2 = b2 / a0, A1 = a1 / a0, A2 = a2 / a0 };
        }
    }

    private struct FirstOrderLp
    {
        public double B0, B1, A1;
        public double Z;

        public void ProcessInPlace(float[] data, int n)
        {
            double z = Z;
            for (int i = 0; i < n; i++)
            {
                double x = data[i];
                // DF1 первого порядка: y = b0*x + z; z = b1*x - a1*y
                double y = B0 * x + z;
                z = B1 * x - A1 * y;
                data[i] = (float)y;
            }
            Z = z;
        }

        public static FirstOrderLp LowPass(double freq, double fs)
        {
            // Билинейное преобразование аналогового LP первого порядка
            double k = Math.Tan(Math.PI * freq / fs);
            double a0 = 1.0 + k;
            return new FirstOrderLp { B0 = k / a0, B1 = k / a0, A1 = (k - 1.0) / a0 };
        }
    }
}
