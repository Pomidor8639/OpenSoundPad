<div align="center">

# OSP — OpenSoundPad

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Version](https://img.shields.io/badge/Version-v1.11.0-10B981?style=for-the-badge)](https://github.com/Pomidor8639/OpenSoundPad/releases)
[![NAudio](https://img.shields.io/badge/NAudio-WASAPI-FF6F00?style=for-the-badge)](https://github.com/naudio/NAudio)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg?style=for-the-badge)](LICENSE)
[![AlternativeTo](https://img.shields.io/badge/AlternativeTo-OpenSoundPad-0078D4?style=for-the-badge&logo=alternativeto&logoColor=white)](https://alternativeto.net/software/opensoundpad/)

**OSP (OpenSoundPad) is a lightweight, low-latency, real-time voice changer and full-featured soundpad for Windows with direct output to virtual microphones.**

[Download Release](https://github.com/Pomidor8639/OpenSoundPad/releases) • [Features](#features) • [Hotkeys & Controls](#hotkeys--controls) • [Quick Start](#quick-start) • [Build from Source](#building-from-source) • [🇷🇺 Русский язык](#-документация-на-русском-языке-russian-documentation)

</div>

---

## What's New in v1.11.0

- **First-Run Language Setup Screen**:
  - On the very first launch, users are greeted with a clean, focused language selection screen.
  - Nothing else is shown until a language is chosen.
  - Your selection is remembered, and OpenSoundPad opens immediately in your chosen language on every subsequent launch.
- **Support for 5 Languages**:
  - Full native translations for: 🇬🇧 **English**, 🇷🇺 **Русский**, 🇩🇪 **Deutsch**, 🇪🇸 **Español**, and 🇫🇷 **Français**.
  - All UI elements, dials, dialogs, presets, and tooltips are localized.
  - Language can be switched anytime in Settings.
- **Persistent Sound Storage**:
  - Sounds added via button or drag-and-drop are automatically copied to internal storage (`%APPDATA%\OpenSoundPad\sounds\`).
  - Audio files never disappear even if moved or deleted from Downloads or Desktop.
- **Sample Renaming**:
  - Right-click any track in the Soundpad and select "Rename…" to customize track titles.
- **Soundpad Rotary Volume Knob**:
  - Sleek rotary knob (0–200%) in the soundpad toolbar matching the Voice Studio styling.

---

## Features

### 1. Voice Changer Studio (Real-Time DSP)
- **5 Built-In Voice Presets:**
  - **1. Anonymous:** Multi-layer pitch shift (-7.0 & -13.5 st), resonance filter, and soft overdrive for complete vocal anonymity.
  - **2. Female:** Natural formant shift (+3.8 st), high-pass sub-bass cut, harmonic enhancement.
  - **3. Kid:** High pitch (+6.0 st), bright tone.
  - **4. Demon:** Deep guttural growl (-9.0 & -15.5 st) with aggressive analog distortion.
  - **5. Custom:** Free manual tuning using 4 rotary knobs.
- **Analog Rotary Knobs (True Angular + Linear Drag):**
  - **Pitch:** -18 to +18 semitones (bipolar scale).
  - **Distortion:** 0 to 100% overdrive.
  - **Bass Boost:** 0 to +15 dB low-end boost.
  - **Robotizer:** 0 to 100% ring modulation.
  - **Mic Volume:** 50% to 400% preamplification.
- **Knob Physics:** Circular radial drag, vertical DAW-style drag, direct click on scale, mouse wheel adjustment, Shift for micro-precision, double-click to reset.

### 2. Soundpad Bank
- **Dynamic Unlimited Track List:** Add as many audio files as you want without hard caps.
- **Fast Importing:** Click «+ Add sound…» or simply Drag & Drop audio files from Windows Explorer.
- **Supported Formats:** `.mp3`, `.wav`, `.ogg`, `.flac`, `.aiff`, `.m4a`, `.wma`.
- **Right-Click Context Menu:** Play, Stop, Loop/Repeat (R), Rename, Remove from list (Del), Clear all.
- **Polyphonic Engine:** Play multiple sounds simultaneously with zero stutter or buffer clipping.
- **Loop / Repeat Mode:** Single click or press `R` to toggle loop, or hold the button for continuous repeat while pressed.

### 3. Audio Pipeline & Routing
- **Universal Virtual Audio Cable Support:** Works with VB-Audio Virtual Cable, Voicemod Virtual Audio, Animaze, etc.
- **Zero-Latency WASAPI Streaming:** Stream processed voice and soundpad effects directly to Discord, Steam, CS2, Dota 2, Telegram, and OBS.
- **Smart Fallback:** If VB-CABLE is not installed, the app allows selecting any standard microphone or output device (like original Soundpad).
- **Monitoring:** Listen to your own processed voice and sound effects in your headphones simultaneously.
- **Built-in Noise Gate:** Adaptive threshold gate to eliminate background hiss and keyboard noise.

### 4. True Black OLED Design
- Pitch-black background with emerald green accents.
- Custom dark title bar with window snapping and caption controls.
- Fast, standalone single-file binary with zero external dependencies.

---

## Hotkeys & Controls

### Keyboard:
| Key | Action |
|---|---|
| `1` – `5` | Switch voice preset (1–5) |
| `F1` – `F12` | Trigger soundpad pads 1–12 |
| `Space` / `Enter` | Play selected track |
| `R` | Toggle loop / repeat for selected track |
| `Delete` | Remove selected track from soundpad |
| `Esc` | Stop all playing sounds and reset loop |
| `T` | Toggle voice effect bypass |
| `M` | Mute / unmute microphone |
| `L` | Toggle headphones monitoring (Listen) |

### Mouse:
| Action | Description |
|---|---|
| **Hold Repeat Button** | Continuous loop while mouse button is held (>350ms) |
| **Click Repeat Button** | Toggle persistent loop ON / OFF |
| **Right-Click Track** | Context menu (Play, Loop, Rename, Delete, Clear) |
| **Double-Click Track** | Play track |
| **Drag & Drop** | Add files from Windows Explorer |
| **Drag Knob Up/Down** | Linear parameter change |
| **Drag Knob in Circle** | Radial dial rotation |
| **Shift + Drag Knob** | Fine-grained precision adjustment |
| **Double-Click Knob** | Reset to default value |

---

## Quick Start

1. Download the release package `OpenSoundPad-v1.11.0-win-x64.zip` (or standalone `OpenSoundPad.exe`) from [Releases](https://github.com/Pomidor8639/OpenSoundPad/releases).
2. Extract the archive to any folder.
3. Run `OpenSoundPad.exe`.
4. On first launch, pick your preferred language (English, Russian, German, Spanish, French).
5. Open **Settings** in the top bar to select your microphone and output device:
   - **Microphone:** your physical mic.
   - **Output:** `CABLE Input (VB-Audio Virtual Cable)` to route into Discord/games, or any audio output if VB-CABLE is absent.
   - **Headphones:** your listening device for monitoring.
6. Click **«▶ Start»** to activate the audio engine.
7. In Discord / game settings, select the input device: `CABLE Output (VB-Audio Virtual Cable)`.

---

## Building from Source

Requires [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) on Windows.

```bash
git clone https://github.com/Pomidor8639/OpenSoundPad.git
cd OpenSoundPad

# Run development build
dotnet run --project src/OpenSoundPad/OpenSoundPad.csproj

# Publish standalone single-file executable
dotnet publish src/OpenSoundPad/OpenSoundPad.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

---

## 🇷🇺 Документация на русском языке (Russian Documentation)

### Описание
**OSP (OpenSoundPad)** — бесплатный, легковесный и производительный войсчейнджер реального времени и полнофункциональный саундпад на C# (.NET 9) для Windows с прямым выводом в виртуальный микрофон.

### Что нового в версии 1.11.0:
- **Экран выбора языка при первом запуске**: при самом первом запуске открывается чистое окно выбора языка, без лишних элементов. Выбор сохраняется, и программа сразу работает на выбранном языке.
- **Поддержка 5 языков**: английский (English), русский (Русский), немецкий (Deutsch), испанский (Español) и французский (Français). Язык можно сменить в любой момент в «Настройках».
- **Постоянное хранилище звуков**: все добавленные треки копируются в `%APPDATA%\OpenSoundPad\sounds\`. Музыка не пропадает, даже если удалить файлы из Загрузок.
- **Переименование треков**: правый клик по треку -> «Переименовать…» для задания любого пользовательского названия.
- **Крутилка громкости в саундпаде**: горизонтальный ползунок заменен на стильную крутилку громкости (0–200%).
- **Автономный запуск**: полностью независимый exe-файл, не требующий установки сторонних библиотек .NET.

### Горячие клавиши:
- `1` – `5`: переключение пресетов голоса (Аноним, Женский, Ребенок, Демон, Свой).
- `F1` – `F12`: запуск треков саундпада.
- `Space` / `Enter`: играть выбранный трек.
- `R`: зацикливание / повтор выбранного звука (Loop).
- `Del`: удалить трек из списка.
- `Esc`: стоп всех звуков и сброс зацикливания.
- `T`: байпас эффекта голоса.
- `M`: заглушить микрофон (Мут).
- `L`: мониторинг в наушниках.

---

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
