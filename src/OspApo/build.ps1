$ErrorActionPreference = "Stop"

$cl = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.44.35207\bin\Hostx64\x64\cl.exe"
$inc1 = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.44.35207\include"
$inc2 = "C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\ucrt"
$inc3 = "C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\shared"
$inc4 = "C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um"
$lib1 = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.44.35207\lib\x64"
$lib2 = "C:\Program Files (x86)\Windows Kits\10\Lib\10.0.26100.0\ucrt\x64"
$lib3 = "C:\Program Files (x86)\Windows Kits\10\Lib\10.0.26100.0\um\x64"
$lib4 = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.44.35207\atlmfc\lib\x64"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

& $cl /nologo /LD /O2 /EHsc /std:c++17 /I "$inc1" /I "$inc2" /I "$inc3" /I "$inc4" "$scriptDir\OspApo.cpp" "$scriptDir\dllmain.cpp" /link /LIBPATH:"$lib1" /LIBPATH:"$lib2" /LIBPATH:"$lib3" /LIBPATH:"$lib4" /DEF:"$scriptDir\OspApo.def" AudioBaseProcessingObjectV140.lib audiomediatypecrt.lib audioeng.lib atls.lib ole32.lib advapi32.lib /OUT:"$scriptDir\OspApo.dll"

if ($LASTEXITCODE -eq 0) {
    Copy-Item "$scriptDir\OspApo.dll" "$scriptDir\..\OpenSoundPad\OspApo.dll" -Force
    Write-Output "Built OspApo.dll successfully and copied to OpenSoundPad"
} else {
    Write-Error "Failed to build OspApo.dll"
}
