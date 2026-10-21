#include "OspApo.h"
#include <strsafe.h>

const AVRT_DATA CRegAPOProperties<1> COspApo::sm_RegProperties(
    CLSID_OpenSoundPadAPO,
    L"OpenSoundPad Microphone APO",
    L"Copyright (c) 2026 OpenSoundPad Community",
    1,
    0,
    __uuidof(IAudioProcessingObjectRT),
    DEFAULT_APOREG_FLAGS,
    1, 1,
    1, 1,
    DEFAULT_APOREG_MAXINSTANCES
);

COspApo::COspApo()
    : CBaseAudioProcessingObject(&sm_RegProperties.m_Properties),
      m_cRef(1),
      m_hMapFile(NULL),
      m_pShm(NULL),
      m_u32ChannelCount(1),
      m_u32SampleRate(48000)
{
}

COspApo::~COspApo()
{
    if (m_pShm)
    {
        UnmapViewOfFile(m_pShm);
        m_pShm = NULL;
    }
    if (m_hMapFile)
    {
        CloseHandle(m_hMapFile);
        m_hMapFile = NULL;
    }
}

void COspApo::EnsureShmConnected()
{
    if (m_pShm)
    {
        return;
    }

    if (!m_hMapFile)
    {
        m_hMapFile = OpenFileMappingW(FILE_MAP_READ | FILE_MAP_WRITE, FALSE, OSP_SHM_NAME);
        if (!m_hMapFile)
        {
            m_hMapFile = OpenFileMappingW(FILE_MAP_READ | FILE_MAP_WRITE, FALSE, OSP_SHM_NAME_LOCAL);
        }
    }

    if (m_hMapFile && !m_pShm)
    {
        m_pShm = (OspApoSharedMemory*)MapViewOfFile(m_hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, sizeof(OspApoSharedMemory));
    }
}

STDMETHODIMP COspApo::QueryInterface(REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;

    if (riid == IID_IUnknown || riid == __uuidof(IAudioProcessingObject))
    {
        *ppv = static_cast<IAudioProcessingObject*>(this);
    }
    else if (riid == __uuidof(IAudioProcessingObjectRT))
    {
        *ppv = static_cast<IAudioProcessingObjectRT*>(this);
    }
    else if (riid == __uuidof(IAudioProcessingObjectConfiguration))
    {
        *ppv = static_cast<IAudioProcessingObjectConfiguration*>(this);
    }
    else if (riid == __uuidof(IAudioSystemEffects))
    {
        *ppv = static_cast<IAudioSystemEffects*>(this);
    }
    else if (riid == __uuidof(IAudioSystemEffects2))
    {
        *ppv = static_cast<IAudioSystemEffects2*>(this);
    }
    else
    {
        *ppv = NULL;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

STDMETHODIMP_(ULONG) COspApo::AddRef()
{
    return (ULONG)InterlockedIncrement(&m_cRef);
}

STDMETHODIMP_(ULONG) COspApo::Release()
{
    ULONG count = (ULONG)InterlockedDecrement(&m_cRef);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

STDMETHODIMP COspApo::Reset()
{
    return CBaseAudioProcessingObject::Reset();
}

STDMETHODIMP COspApo::GetLatency(HNSTIME* pTime)
{
    if (!pTime) return E_POINTER;
    *pTime = 0; // Zero added latency
    return S_OK;
}

STDMETHODIMP COspApo::LockForProcess(
    UINT32 u32NumInputConnections,
    APO_CONNECTION_DESCRIPTOR** ppInputConnections,
    UINT32 u32NumOutputConnections,
    APO_CONNECTION_DESCRIPTOR** ppOutputConnections)
{
    HRESULT hr = CBaseAudioProcessingObject::LockForProcess(
        u32NumInputConnections, ppInputConnections,
        u32NumOutputConnections, ppOutputConnections);

    if (FAILED(hr))
    {
        return hr;
    }

    if (u32NumInputConnections > 0 && ppInputConnections && ppInputConnections[0] && ppInputConnections[0]->pFormat)
    {
        const WAVEFORMATEX* pWfx = ppInputConnections[0]->pFormat->GetAudioFormat();
        if (pWfx)
        {
            m_u32ChannelCount = pWfx->nChannels;
            m_u32SampleRate = pWfx->nSamplesPerSec;
        }
    }

    EnsureShmConnected();
    return S_OK;
}

STDMETHODIMP COspApo::UnlockForProcess()
{
    if (m_pShm)
    {
        UnmapViewOfFile(m_pShm);
        m_pShm = NULL;
    }
    if (m_hMapFile)
    {
        CloseHandle(m_hMapFile);
        m_hMapFile = NULL;
    }
    return CBaseAudioProcessingObject::UnlockForProcess();
}

STDMETHODIMP_(void) COspApo::APOProcess(
    UINT32 u32NumInputConnections,
    APO_CONNECTION_PROPERTY** ppInputConnections,
    UINT32 u32NumOutputConnections,
    APO_CONNECTION_PROPERTY** ppOutputConnections)
{
    if (u32NumInputConnections == 0 || !ppInputConnections || !ppInputConnections[0])
    {
        return;
    }
    if (u32NumOutputConnections == 0 || !ppOutputConnections || !ppOutputConnections[0])
    {
        return;
    }

    UINT32 frames = ppInputConnections[0]->u32ValidFrameCount;
    UINT32 totalSamples = frames * m_u32ChannelCount;
    FLOAT32* pIn = (FLOAT32*)ppInputConnections[0]->pBuffer;
    FLOAT32* pOut = (FLOAT32*)ppOutputConnections[0]->pBuffer;

    if (!pIn || !pOut || frames == 0)
    {
        return;
    }

    // Default pass-through
    if (pOut != pIn)
    {
        memcpy(pOut, pIn, totalSamples * sizeof(FLOAT32));
    }

    EnsureShmConnected();

    if (m_pShm && m_pShm->magic == OSP_IPC_MAGIC && m_pShm->active == 1)
    {
        uint32_t wp = m_pShm->writeIndex;
        uint32_t rp = m_pShm->readIndex;

        if (wp != rp)
        {
            uint32_t available = (wp >= rp) ? (wp - rp) : (OSP_RING_CAPACITY - rp + wp);
            uint32_t toProcess = (available < totalSamples) ? available : totalSamples;
            float vol = m_pShm->volume;
            if (vol <= 0.0f) vol = 1.0f;
            uint32_t mode = m_pShm->mode;

            if (mode == 1)
            {
                // Mix sound into microphone
                for (UINT32 i = 0; i < toProcess; ++i)
                {
                    float padSample = m_pShm->ringBuffer[rp % OSP_RING_CAPACITY] * vol;
                    rp++;
                    float mixed = pOut[i] + padSample;
                    if (mixed > 1.0f) mixed = 1.0f;
                    else if (mixed < -1.0f) mixed = -1.0f;
                    pOut[i] = mixed;
                }
                m_pShm->readIndex = rp % OSP_RING_CAPACITY;
            }
            else if (mode == 2)
            {
                // Replace microphone with voice changer modulated output
                for (UINT32 i = 0; i < toProcess; ++i)
                {
                    float voiceSample = m_pShm->ringBuffer[rp % OSP_RING_CAPACITY] * vol;
                    rp++;
                    if (voiceSample > 1.0f) voiceSample = 1.0f;
                    else if (voiceSample < -1.0f) voiceSample = -1.0f;
                    pOut[i] = voiceSample;
                }
                m_pShm->readIndex = rp % OSP_RING_CAPACITY;
            }
            else if (mode == 3)
            {
                // Mute microphone, play sound only
                for (UINT32 i = 0; i < toProcess; ++i)
                {
                    float padSample = m_pShm->ringBuffer[rp % OSP_RING_CAPACITY] * vol;
                    rp++;
                    if (padSample > 1.0f) padSample = 1.0f;
                    else if (padSample < -1.0f) padSample = -1.0f;
                    pOut[i] = padSample;
                }
                for (UINT32 i = toProcess; i < totalSamples; ++i)
                {
                    pOut[i] = 0.0f;
                }
                m_pShm->readIndex = rp % OSP_RING_CAPACITY;
            }
        }
    }

    ppOutputConnections[0]->u32ValidFrameCount = frames;
    ppOutputConnections[0]->u32BufferFlags = ppInputConnections[0]->u32BufferFlags;
}

STDMETHODIMP COspApo::GetEffectsList(
    LPGUID* ppEffectsIds,
    UINT* pcEffects,
    HANDLE Event)
{
    UNREFERENCED_PARAMETER(Event);
    if (!pcEffects || !ppEffectsIds)
    {
        return E_POINTER;
    }
    *pcEffects = 0;
    *ppEffectsIds = NULL;
    return S_OK;
}
