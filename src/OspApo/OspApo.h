#pragma once
#include <windows.h>
#include <initguid.h>
#include <audioenginebaseapo.h>
#include <baseaudioprocessingobject.h>
#include "OspIpc.h"

// CLSID: {B1A0A001-44B9-4591-9B5F-0B8639059AD0}
DEFINE_GUID(CLSID_OpenSoundPadAPO, 
    0xb1a0a001, 0x44b9, 0x4591, 0x9b, 0x5f, 0x0b, 0x86, 0x39, 0x05, 0x9a, 0xd0);

class COspApo : public CBaseAudioProcessingObject, public IAudioSystemEffects2
{
private:
    LONG m_cRef;
    HANDLE m_hMapFile;
    OspApoSharedMemory* m_pShm;
    UINT32 m_u32ChannelCount;
    UINT32 m_u32SampleRate;

    static const CRegAPOProperties<1> sm_RegProperties;

    void EnsureShmConnected();

public:
    COspApo();
    virtual ~COspApo();

    // IUnknown
    STDMETHODIMP QueryInterface(REFIID riid, void** ppv) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;

    // IAudioProcessingObjectRT
    STDMETHODIMP_(void) APOProcess(
        UINT32 u32NumInputConnections,
        APO_CONNECTION_PROPERTY** ppInputConnections,
        UINT32 u32NumOutputConnections,
        APO_CONNECTION_PROPERTY** ppOutputConnections) override;

    // IAudioProcessingObjectConfiguration
    STDMETHODIMP LockForProcess(
        UINT32 u32NumInputConnections,
        APO_CONNECTION_DESCRIPTOR** ppInputConnections,
        UINT32 u32NumOutputConnections,
        APO_CONNECTION_DESCRIPTOR** ppOutputConnections) override;

    STDMETHODIMP UnlockForProcess() override;

    // IAudioProcessingObject
    STDMETHODIMP Reset() override;
    STDMETHODIMP GetLatency(HNSTIME* pTime) override;

    // IAudioSystemEffects2
    STDMETHODIMP GetEffectsList(
        LPGUID* ppEffectsIds,
        UINT* pcEffects,
        HANDLE Event) override;
};
