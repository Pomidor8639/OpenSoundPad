using System;
using System.Runtime.InteropServices;

namespace OpenSoundPad.Audio;

public sealed class OspApoClient : IDisposable
{
    private const uint Magic = 0x4F535041;
    private const uint Version = 1;
    private const int RingCapacity = 65536;
    private const uint FileMapAllAccess = 0x000F001F;
    private const uint PageReadWrite = 0x04;
    private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

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

    private const string ShmNameGlobal = @"Global\OpenSoundPadApoShm";
    private const string ShmNameLocal = @"Local\OpenSoundPadApoShm";

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private IntPtr _hMap = IntPtr.Zero;
    private IntPtr _pView = IntPtr.Zero;
    private readonly object _lock = new();
    private bool _disposed;

    public bool IsConnected => _pView != IntPtr.Zero;

    public OspApoClient()
    {
        InitializeMemory();
    }

    public void EnsureConnected()
    {
        if (_pView == IntPtr.Zero)
        {
            InitializeMemory();
        }
    }

    private void InitializeMemory()
    {
        lock (_lock)
        {
            try
            {
                _hMap = OpenFileMapping(FileMapAllAccess, false, ShmNameGlobal);

                if (_hMap == IntPtr.Zero)
                {
                    _hMap = OpenFileMapping(FileMapAllAccess, false, ShmNameLocal);
                }

                if (_hMap == IntPtr.Zero)
                {
                    _hMap = CreateFileMapping(InvalidHandleValue, IntPtr.Zero, PageReadWrite, 0, (uint)TotalShmSize, ShmNameLocal);
                }

                if (_hMap != IntPtr.Zero)
                {
                    _pView = MapViewOfFile(_hMap, FileMapAllAccess, 0, 0, new UIntPtr((uint)TotalShmSize));
                    if (_pView != IntPtr.Zero)
                    {
                        Marshal.WriteInt32(_pView, OffsetMagic, (int)Magic);
                        Marshal.WriteInt32(_pView, OffsetVersion, (int)Version);
                        Marshal.WriteInt32(_pView, OffsetActive, 1);
                        Marshal.WriteInt32(_pView, OffsetMode, 1);
                        Marshal.WriteInt32(_pView, OffsetSampleRate, 48000);
                        Marshal.WriteInt32(_pView, OffsetChannels, 1);
                        Marshal.WriteInt32(_pView, OffsetWriteIndex, 0);
                        Marshal.WriteInt32(_pView, OffsetReadIndex, 0);
                        Marshal.Copy(new[] { 1.0f }, 0, IntPtr.Add(_pView, OffsetVolume), 1);
                    }
                }
            }
            catch
            {
                Cleanup();
            }
        }
    }

    public void SetActive(bool active)
    {
        EnsureConnected();
        if (_pView == IntPtr.Zero) return;
        lock (_lock)
        {
            try
            {
                Marshal.WriteInt32(_pView, OffsetActive, active ? 1 : 0);
            }
            catch { }
        }
    }

    public void WriteSamples(float[] samples, int offset, int count, uint mode, float volume)
    {
        EnsureConnected();
        if (_pView == IntPtr.Zero || count <= 0) return;

        lock (_lock)
        {
            try
            {
                Marshal.WriteInt32(_pView, OffsetActive, 1);
                Marshal.WriteInt32(_pView, OffsetMode, (int)mode);
                Marshal.Copy(new[] { volume }, 0, IntPtr.Add(_pView, OffsetVolume), 1);

                uint wp = (uint)Marshal.ReadInt32(_pView, OffsetWriteIndex);
                uint rp = (uint)Marshal.ReadInt32(_pView, OffsetReadIndex);

                uint lag = (wp >= rp) ? (wp - rp) : ((uint)RingCapacity - rp + wp);
                if (lag > 16384)
                {
                    wp = (rp + 480) % (uint)RingCapacity;
                }

                IntPtr ringBase = IntPtr.Add(_pView, OffsetRingBuffer);

                int firstChunk = Math.Min(count, RingCapacity - (int)wp);
                Marshal.Copy(samples, offset, IntPtr.Add(ringBase, (int)wp * sizeof(float)), firstChunk);

                if (firstChunk < count)
                {
                    int secondChunk = count - firstChunk;
                    Marshal.Copy(samples, offset + firstChunk, ringBase, secondChunk);
                }

                uint newWp = (wp + (uint)count) % (uint)RingCapacity;
                Marshal.WriteInt32(_pView, OffsetWriteIndex, (int)newWp);
            }
            catch { }
        }
    }

    private void Cleanup()
    {
        if (_pView != IntPtr.Zero)
        {
            try { UnmapViewOfFile(_pView); } catch { }
            _pView = IntPtr.Zero;
        }
        if (_hMap != IntPtr.Zero)
        {
            try { CloseHandle(_hMap); } catch { }
            _hMap = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SetActive(false);
        lock (_lock)
        {
            Cleanup();
        }
    }
}
