using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace OpenSoundPad.Audio;

public sealed class OspApoClient : IDisposable
{
    private const uint Magic = 0x4F535041;
    private const uint Version = 1;
    private const int RingCapacity = 65536;
    private const string ShmNameGlobal = @"Global\OpenSoundPadApoShm";
    private const string ShmNameLocal = @"Local\OpenSoundPadApoShm";

    private const int OffsetMagic = 0;
    private const int OffsetVersion = 4;
    private const int OffsetActive = 8;
    private const int OffsetMode = 12;
    private const int OffsetSampleRate = 16;
    private const int OffsetChannels = 20;
    private const int OffsetWriteIndex = 24;
    private const int OffsetReadIndex = 28;
    private const int OffsetVolume = 32;
    private const int OffsetRingBuffer = 36;
    private const int TotalShmSize = OffsetRingBuffer + (RingCapacity * sizeof(float));

    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;
    private readonly object _lock = new();
    private bool _disposed;

    public bool IsConnected => _accessor != null;

    public OspApoClient()
    {
        InitializeMemory();
    }

    private void InitializeMemory()
    {
        try
        {
            try
            {
                _mmf = MemoryMappedFile.CreateOrOpen(ShmNameGlobal, TotalShmSize, MemoryMappedFileAccess.ReadWrite);
            }
            catch
            {
                _mmf = MemoryMappedFile.CreateOrOpen(ShmNameLocal, TotalShmSize, MemoryMappedFileAccess.ReadWrite);
            }

            _accessor = _mmf.CreateViewAccessor(0, TotalShmSize, MemoryMappedFileAccess.ReadWrite);

            _accessor.Write(OffsetMagic, Magic);
            _accessor.Write(OffsetVersion, Version);
            _accessor.Write(OffsetActive, 1u);
            _accessor.Write(OffsetMode, 1u);
            _accessor.Write(OffsetSampleRate, 48000u);
            _accessor.Write(OffsetChannels, 1u);
            _accessor.Write(OffsetWriteIndex, 0u);
            _accessor.Write(OffsetReadIndex, 0u);
            _accessor.Write(OffsetVolume, 1.0f);
        }
        catch
        {
            _accessor?.Dispose();
            _accessor = null;
            _mmf?.Dispose();
            _mmf = null;
        }
    }

    public void WriteSamples(float[] samples, int offset, int count, uint mode, float volume)
    {
        if (_accessor == null || count <= 0) return;

        lock (_lock)
        {
            try
            {
                _accessor.Write(OffsetActive, 1u);
                _accessor.Write(OffsetMode, mode);
                _accessor.Write(OffsetVolume, volume);

                uint wp = _accessor.ReadUInt32(OffsetWriteIndex);
                uint rp = _accessor.ReadUInt32(OffsetReadIndex);

                uint lag = (wp >= rp) ? (wp - rp) : ((uint)RingCapacity - rp + wp);
                if (lag > 16384)
                {
                    wp = (rp + 480) % RingCapacity;
                }

                for (int i = 0; i < count; i++)
                {
                    int pos = (int)((wp + i) % RingCapacity);
                    int byteOffset = OffsetRingBuffer + (pos * sizeof(float));
                    _accessor.Write(byteOffset, samples[offset + i]);
                }

                uint newWp = (wp + (uint)count) % RingCapacity;
                _accessor.Write(OffsetWriteIndex, newWp);
            }
            catch
            {
            }
        }
    }

    public void SetActive(bool active)
    {
        if (_accessor == null) return;
        try
        {
            _accessor.Write(OffsetActive, active ? 1u : 0u);
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_accessor != null)
            {
                _accessor.Write(OffsetActive, 0u);
                _accessor.Dispose();
                _accessor = null;
            }
            _mmf?.Dispose();
            _mmf = null;
        }
        catch { }
    }
}
