using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using Microsoft.Win32;

namespace OpenSoundPad.Audio;

public static class OspApoManager
{
    public const string ApoClsid = "{B1A0A001-44B9-4591-9B5F-0B8639059AD0}";
    public const string UniteFxClsid = "{27384E53-9860-0AC1-9519-C60EBCAA2C71}";
    public const string ClownfishClsid = "{27384E53-9860-0AC3-9519-C60EBCAA2C71}";

    private const string PkeyFxEndpointEffect = "{d04e05a6-594b-4fb6-a80d-01af5eed7d1d},2";
    private const string PkeyFxPreMix = "{d04e05a6-594b-4fb6-a80d-01af5eed7d1d},10";
    private const string PkeyFxStream = "{d04e05a6-594b-4fb6-a80d-01af5eed7d1d},5";

    public static bool IsAdministrator()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static string ExtractGuid(string endpointId)
    {
        if (string.IsNullOrWhiteSpace(endpointId)) return string.Empty;
        int start = endpointId.IndexOf('{');
        int end = endpointId.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return endpointId.Substring(start, end - start + 1);
        }
        return endpointId;
    }

    public static bool IsApoInstalledOnEndpoint(string endpointId)
    {
        string guid = ExtractGuid(endpointId);
        if (string.IsNullOrWhiteSpace(guid)) return false;

        try
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture\" + guid + @"\FxProperties";
            using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
            if (key == null) return false;

            string[] propNames = { PkeyFxEndpointEffect, PkeyFxPreMix, PkeyFxStream };
            foreach (var prop in propNames)
            {
                object? val = key.GetValue(prop);
                if (val is string str)
                {
                    if (str.Equals(ApoClsid, StringComparison.OrdinalIgnoreCase) ||
                        str.Equals(UniteFxClsid, StringComparison.OrdinalIgnoreCase) ||
                        str.Equals(ClownfishClsid, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
        }
        return false;
    }

    public static bool InstallApoOnEndpoint(string endpointId)
    {
        if (!IsAdministrator())
        {
            return RunElevatedAction("--install-apo", endpointId);
        }

        string guid = ExtractGuid(endpointId);
        if (string.IsNullOrWhiteSpace(guid)) return false;

        try
        {
            string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "OpenSoundPad");
            Directory.CreateDirectory(targetDir);
            string targetDll = Path.Combine(targetDir, "OspApo.dll");

            byte[]? dllBytes = LoadApoDllBytes();
            if (dllBytes != null && dllBytes.Length > 0)
            {
                try
                {
                    File.WriteAllBytes(targetDll, dllBytes);
                }
                catch
                {
                }
            }

            if (!File.Exists(targetDll))
            {
                string localDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OspApo.dll");
                if (File.Exists(localDll))
                {
                    try
                    {
                        File.Copy(localDll, targetDll, true);
                    }
                    catch
                    {
                        targetDll = localDll;
                    }
                }
            }

            using (var audioKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Audio", true))
            {
                audioKey?.SetValue("DisableProtectedAudioDG", 1, RegistryValueKind.DWord);
            }

            string clsidKeyPath = @"SOFTWARE\Classes\CLSID\" + ApoClsid;
            using (var clsidKey = Registry.LocalMachine.CreateSubKey(clsidKeyPath, true))
            {
                clsidKey?.SetValue(string.Empty, "OpenSoundPad Microphone APO");
                using var inprocKey = clsidKey?.CreateSubKey("InProcServer32", true);
                inprocKey?.SetValue(string.Empty, targetDll);
                inprocKey?.SetValue("ThreadingModel", "Both");
            }

            string fxKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture\" + guid + @"\FxProperties";
            using (var fxKey = Registry.LocalMachine.CreateSubKey(fxKeyPath, true))
            {
                fxKey?.SetValue(PkeyFxEndpointEffect, ApoClsid, RegistryValueKind.String);
                fxKey?.SetValue(PkeyFxPreMix, ApoClsid, RegistryValueKind.String);
            }

            RestartAudioService();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool UninstallApoFromEndpoint(string endpointId)
    {
        if (!IsAdministrator())
        {
            return RunElevatedAction("--uninstall-apo", endpointId);
        }

        string guid = ExtractGuid(endpointId);
        if (string.IsNullOrWhiteSpace(guid)) return false;

        try
        {
            string fxKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture\" + guid + @"\FxProperties";
            using (var fxKey = Registry.LocalMachine.OpenSubKey(fxKeyPath, true))
            {
                if (fxKey != null)
                {
                    string[] propNames = { PkeyFxEndpointEffect, PkeyFxPreMix, PkeyFxStream };
                    foreach (var prop in propNames)
                    {
                        object? val = fxKey.GetValue(prop);
                        if (val is string str && str.Equals(ApoClsid, StringComparison.OrdinalIgnoreCase))
                        {
                            fxKey.DeleteValue(prop, false);
                        }
                    }
                }
            }

            RestartAudioService();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void RestartAudioService()
    {
        try
        {
            var psi = new ProcessStartInfo("net.exe", "stop audiosrv /y")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            p?.WaitForExit(3000);

            psi.Arguments = "start audiosrv";
            p = Process.Start(psi);
            p?.WaitForExit(3000);
        }
        catch
        {
        }
    }

    private static bool RunElevatedAction(string flag, string endpointId)
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "OpenSoundPad.exe";
            var psi = new ProcessStartInfo(exePath, flag + " \"" + endpointId + "\"")
            {
                Verb = "runas",
                UseShellExecute = true
            };
            var p = Process.Start(psi);
            p?.WaitForExit(10000);
            return p?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static byte[]? LoadApoDllBytes()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            string resourceName = "OpenSoundPad.OspApo.dll";
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        catch
        {
        }

        string diskPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OspApo.dll");
        if (File.Exists(diskPath))
        {
            try
            {
                return File.ReadAllBytes(diskPath);
            }
            catch
            {
            }
        }
        return null;
    }
}
