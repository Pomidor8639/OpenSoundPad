#include "OspApo.h"
#include <strsafe.h>

static LONG g_cObjects = 0;
static LONG g_cServerLocks = 0;
static HMODULE g_hModule = NULL;

class COspApoClassFactory : public IClassFactory
{
private:
    LONG m_cRef;

public:
    COspApoClassFactory() : m_cRef(1)
    {
        InterlockedIncrement(&g_cObjects);
    }

    ~COspApoClassFactory()
    {
        InterlockedDecrement(&g_cObjects);
    }

    STDMETHODIMP QueryInterface(REFIID riid, void** ppv) override
    {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IClassFactory)
        {
            *ppv = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }
        *ppv = NULL;
        return E_NOINTERFACE;
    }

    STDMETHODIMP_(ULONG) AddRef() override
    {
        return (ULONG)InterlockedIncrement(&m_cRef);
    }

    STDMETHODIMP_(ULONG) Release() override
    {
        ULONG count = (ULONG)InterlockedDecrement(&m_cRef);
        if (count == 0)
        {
            delete this;
        }
        return count;
    }

    STDMETHODIMP CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppv) override
    {
        if (pUnkOuter != NULL)
        {
            return CLASS_E_NOAGGREGATION;
        }

        COspApo* pApo = new (std::nothrow) COspApo();
        if (!pApo)
        {
            return E_OUTOFMEMORY;
        }

        HRESULT hr = pApo->QueryInterface(riid, ppv);
        pApo->Release();
        return hr;
    }

    STDMETHODIMP LockServer(BOOL fLock) override
    {
        if (fLock)
        {
            InterlockedIncrement(&g_cServerLocks);
        }
        else
        {
            InterlockedDecrement(&g_cServerLocks);
        }
        return S_OK;
    }
};

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    UNREFERENCED_PARAMETER(lpReserved);
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hModule = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

STDAPI DllCanUnloadNow()
{
    return (g_cObjects == 0 && g_cServerLocks == 0) ? S_OK : S_FALSE;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = NULL;

    if (!IsEqualCLSID(rclsid, CLSID_OpenSoundPadAPO))
    {
        return CLASS_E_CLASSNOTAVAILABLE;
    }

    COspApoClassFactory* pFactory = new (std::nothrow) COspApoClassFactory();
    if (!pFactory)
    {
        return E_OUTOFMEMORY;
    }

    HRESULT hr = pFactory->QueryInterface(riid, ppv);
    pFactory->Release();
    return hr;
}

static const WCHAR g_szClsidKey[] = L"SOFTWARE\\Classes\\CLSID\\{B1A0A001-44B9-4591-9B5F-0B8639059AD0}";

STDAPI DllRegisterServer()
{
    WCHAR szModulePath[MAX_PATH];
    if (GetModuleFileNameW(g_hModule, szModulePath, MAX_PATH) == 0)
    {
        return HRESULT_FROM_WIN32(GetLastError());
    }

    HKEY hKey = NULL;
    LONG lRes = RegCreateKeyExW(HKEY_LOCAL_MACHINE, g_szClsidKey, 0, NULL, 0, KEY_WRITE, NULL, &hKey, NULL);
    if (lRes != ERROR_SUCCESS)
    {
        return HRESULT_FROM_WIN32(lRes);
    }

    const WCHAR szFriendlyName[] = L"OpenSoundPad Microphone APO";
    RegSetValueExW(hKey, NULL, 0, REG_SZ, (const BYTE*)szFriendlyName, (DWORD)(sizeof(szFriendlyName)));

    HKEY hInprocKey = NULL;
    lRes = RegCreateKeyExW(hKey, L"InProcServer32", 0, NULL, 0, KEY_WRITE, NULL, &hInprocKey, NULL);
    if (lRes == ERROR_SUCCESS)
    {
        RegSetValueExW(hInprocKey, NULL, 0, REG_SZ, (const BYTE*)szModulePath, (DWORD)((wcslen(szModulePath) + 1) * sizeof(WCHAR)));
        const WCHAR szThreadingModel[] = L"Both";
        RegSetValueExW(hInprocKey, L"ThreadingModel", 0, REG_SZ, (const BYTE*)szThreadingModel, (DWORD)(sizeof(szThreadingModel)));
        RegCloseKey(hInprocKey);
    }

    RegCloseKey(hKey);
    return S_OK;
}

STDAPI DllUnregisterServer()
{
    RegDeleteTreeW(HKEY_LOCAL_MACHINE, g_szClsidKey);
    return S_OK;
}
