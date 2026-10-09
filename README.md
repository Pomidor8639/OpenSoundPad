<div align="center">

# OSP — OpenSoundPad

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Release](https://img.shields.io/github/v/release/Pomidor8639/OpenSoundPad?style=for-the-badge&color=blue)](https://github.com/Pomidor8639/OpenSoundPad/releases)
[![NAudio](https://img.shields.io/badge/NAudio-WASAPI-FF6F00?style=for-the-badge)](https://github.com/naudio/NAudio)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**Визуальное приложение для изменения голоса в реальном времени + саундпад, с выводом в виртуальный микрофон Windows.**

</div>

---

## Возможности

- 5 голосовых пресетов:
  - 1. Аноним: многослойный сдвиг тона (-7.0 и -13.5 полутонов), глубокий резонанс (135 Гц), срез верхов (3900 Гц) и ламповый перегруз. Полная маскировка тембра.
  - 2. Женский: формантный сдвиг (+3.8 полутона), фильтр баса (170 Гц), осветление гармоник (2800 Гц).
  - 3. Ребенок: высокий звонкий голос (+6.0 полутонов), обрезной фильтр 230 Гц, акцент 3600 Гц.
  - 4. Демон: экстремально низкий рык (-9.0 и -15.5 полутонов) с массивным овердрайвом.
  - 5. Пользовательский: ручная настройка — питч от -18 до +18 полутонов, дисторшн 0-100%, бас 0..+15 дБ, робо-модуляция 0-100%.

- Саундпад 12 слотов:
  - Загрузка wav/mp3/ogg/flac (drag через меню пада), полифония, громкость на пад + мастер-гейн.
  - Горячие клавиши F1–F12, кнопка «Стоп все пады».

- Мягкий кроссфейд при переключении голосов — без щелчков.
- Встроенный предусилитель (x2.2), регулировка громкости 50–400%, адаптивный Noise Gate.
- Маршрутизация в виртуальный кабель (VB-CABLE, Animaze, Voicemod) → Discord, Telegram, CS2, Dota 2, OBS.
- Мониторинг себя в наушниках с отдельной цепочкой вывода.
- Живые VU-метры входа/выхода, выбор устройств из GUI.
- Настройки в `%APPDATA%\OpenSoundPad\config.json`, автомиграция со старого VoicehackTool.

---

## Горячие клавиши

| Клавиша | Действие |
|---|---|
| `1`–`5` | Пресеты голоса |
| `F1`–`F12` | Пады саундпада |
| `T` | Вкл/выкл эффект (Bypass) |
| `M` | Мут |
| `L` | Мониторинг в наушниках |

---

## Требования

- Windows 10 / 11, .NET 9 Runtime (для self-contained сборки не нужен)
- Виртуальный аудиодрайвер: VB-Audio Virtual Cable (базовый — можно встроить в setup), Animaze, Voicemod

---

## Запуск из исходников

```bash
cd src/OpenSoundPad
dotnet run
```

## Сборка релиза (один exe)

```bash
cd src/OpenSoundPad
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

---

## Настройка виртуального микрофона

1. В правой панели выберите микрофон, виртуальный кабель и наушники, нажмите **▶ Старт**.
2. В Discord / Telegram / играх в поле **Устройство ввода** выберите соответствующий виртуальный микрофон (`CABLE Output`, `Microphone (Animaze Virtual Audio)`).

---

## Структура проекта

- `src/OpenSoundPad/` — WPF-приложение (.NET 9, `WinExe`, консоли больше нет).
- `src/OpenSoundPad/Audio/OspDsp.cs` — DSP-ядро (порт `dsp.py`: гранулярный питч-шифтинг, биквады, овердрайв, гейт, кроссфейд).
- `src/OpenSoundPad/Audio/OspEngine.cs` — WASAPI-движок на NAudio (захват → DSP+пады → кабель + монитор).
- `src/OpenSoundPad/Audio/SoundPadBank.cs` — банк из 12 падов с полифонией.
- `src/OpenSoundPad/Audio/OspConfig.cs` — конфиг `%APPDATA%\OpenSoundPad\config.json` + миграция.
- `src/OpenSoundPad/MainWindow.xaml(.cs)` — визуал: голоса, саундпад 3×4, микшер, устройства, VU.
- `driver/OspVirtualMic.inf` — каркас INF для будущего собственного драйвера (phase 2, нужен WDK + подпись).

---

## Лицензия

MIT License
