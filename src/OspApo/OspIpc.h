#pragma once
#include <windows.h>
#include <stdint.h>

#define OSP_IPC_MAGIC 0x4F535041 // 'OSPA'
#define OSP_IPC_VERSION 1
#define OSP_RING_CAPACITY 65536 // 64K float samples

// Shared memory layout
struct OspApoSharedMemory
{
    uint32_t magic;
    uint32_t version;
    uint32_t active;       // 1 if OpenSoundPad is running and active
    uint32_t mode;         // 1 = Mix (Soundpad into mic), 2 = Replace (Voice Changer replaces mic), 3 = Mute mic
    uint32_t sampleRate;   // 44100 or 48000
    uint32_t channels;     // 1 or 2
    volatile uint32_t writeIndex;
    volatile uint32_t readIndex;
    float volume;          // 0.0 - 2.0
    float ringBuffer[OSP_RING_CAPACITY];
};

#define OSP_SHM_NAME L"Global\\OpenSoundPadApoShm"
#define OSP_SHM_NAME_LOCAL L"Local\\OpenSoundPadApoShm"
