namespace OpenSoundPad;

/// <summary>Строки интерфейса ru/en. Loc.Lang = "ru" | "en".</summary>
public static class Loc
{
    public static string Lang = "ru";
    private static bool En => Lang == "en";

    // Меню
    public static string MenuFile => En ? "File" : "Файл";
    public static string MenuOpenFolder => En ? "Open config folder" : "Открыть папку конфига";
    public static string MenuExit => En ? "Exit" : "Выход";
    public static string MenuSettings => En ? "Settings" : "Настройки";
    public static string MenuOpenSettings => En ? "Settings…" : "Настройки…";
    public static string MenuHelp => En ? "Help" : "Справка";
    public static string MenuAbout => En ? "About" : "О программе";
    public static string AboutText => En
        ? "OSP OpenSoundPad\nVoice changer + soundpad.\nOutput: virtual cable (VB-CABLE / Animaze / Voicemod)."
        : "OSP OpenSoundPad\nВойсчейнджер + саундпад.\nВывод: виртуальный кабель (VB-CABLE / Animaze / Voicemod).";

    // Режимы
    public static string ModeVoice => En ? "Voice Changer" : "Войсмод";
    public static string ModePad => En ? "Soundpad" : "Саундпад";
    public static string VoiceStudio => En ? "Custom Voice Studio" : "Студия кастомного голоса";
    public static string VoicePresets => En ? "Voice Presets" : "Голосовые пресеты";
    public static string ResetVoiceParams => En ? "Reset sliders" : "Сбросить параметры";
    public static string ClearAllPads => En ? "Clear all" : "Очистить всё";
    public static string ClearAllAsk => En ? "Remove all sounds from the list?" : "Удалить все звуки из списка?";
    public static string AddSound => En ? "Add sound…" : "Добавить звук…";
    public static string RemoveSound => En ? "Remove" : "Удалить";
    public static string PlayingCount(int count) => En ? $"Active: {count}" : $"Играет: {count}";
    public static string SoundpadRouting => En ? "Routing to Cable:" : "Трансляция в кабель:";
    public static string SoundpadHint => En
        ? "Right-click track to remove / clear list • Drag files here or click «+ Add sound»"
        : "Правый клик по треку: удалить / очистить список • Перетащите файлы сюда или нажмите «+ Добавить звук»";
    public static string NoSoundsYet => En
        ? "No sounds yet. Click «+ Add sound» or drag & drop audio files here."
        : "Звуки пока не добавлены. Нажмите «+ Добавить звук» или перетащите аудиофайлы сюда.";
    public static string VoiceHotkeysHint => En
        ? "Keys: 1-5 voice presets, T effect bypass, M mute mic, L monitor"
        : "Клавиши: 1-5 пресеты, T эффект вкл/выкл, M мут микрофона, L монитор";
    public static string PadHotkeysHint => En
        ? "Keys: F1-F12 play pads, Space/Enter play selected, Del remove, Esc stop all"
        : "Клавиши: F1-F12 запуск, Space/Enter играть выбранный, Del удалить, Esc стоп всех";

    public static string VoiceDesc(int id) => (id, En) switch
    {
        (1, false) => "Маскировка тембра, сдвиг -7/-13.5 st, перегруз",
        (1, true) => "Timbre disguise, multi-layer pitch shift, overdrive",
        (2, false) => "Формантный сдвиг +3.8 st, осветление",
        (2, true) => "Formant shift +3.8 st, bright harmonics",
        (3, false) => "Высокий голос +6.0 st, звонкий",
        (3, true) => "High pitch +6.0 st, child tone",
        (4, false) => "Глубокий рык -9/-15.5 st, массивный перегруз",
        (4, true) => "Deep growl -9/-15.5 st, massive overdrive",
        (_, false) => "Индивидуальная ручная настройка параметров",
        (_, true) => "Fully custom parameters",
    };

    // Тулбар
    public static string TbStart => En ? "Start" : "Старт";
    public static string TbStop => En ? "Stop" : "Стоп";
    public static string TbEffectOff => En ? "Bypass (T)" : "Эффект выкл (T)";
    public static string TbEffectOn => En ? "Effect on (T)" : "Эффект вкл (T)";
    public static string TbMute => En ? "Mute (M)" : "Мут (M)";
    public static string TbUnmute => En ? "Unmute (M)" : "Анмут (M)";
    public static string TbMonitor => En ? "Monitor (L)" : "Монитор (L)";
    public static string TbMonitorOn => En ? "Monitor: ON (L)" : "Монитор: ВКЛ (L)";

    // Группы
    public static string GrVoices => En ? "Voices" : "Голоса";
    public static string GrPads => En ? "Soundpad" : "Саундпад";
    public static string GrLevels => En ? "Levels" : "Уровни";
    public static string Effect => En ? "Effect:" : "Эффект:";
    public static string EffectActive => En ? "ACTIVE" : "АКТИВЕН";
    public static string EffectBypass => En ? "ORIGINAL" : "ОРИГИНАЛ";
    public static string CustomVoice => En ? "Custom voice (preset 5):" : "Свой голос (пресет 5):";

    public static string VoiceName(int id) => (id, En) switch
    {
        (1, false) => "Аноним", (1, true) => "Anonymous",
        (2, false) => "Женский", (2, true) => "Female",
        (3, false) => "Ребенок", (3, true) => "Kid",
        (4, false) => "Демон", (4, true) => "Demon",
        (_, false) => "Свой", (_, true) => "Custom",
    };

    public static string Pitch(double v) => En ? $"Pitch: {v:+0.0;-0.0} st" : $"Питч: {v:+0.0;-0.0} st";
    public static string Drive(int v) => En ? $"Distortion: {v}%" : $"Дисторшн: {v}%";
    public static string Bass(double v) => En ? $"Bass: +{v:0.0} dB" : $"Бас: +{v:0.0} дБ";
    public static string Robot(int v) => En ? $"Robot: {v}%" : $"Робот: {v}%";
    public static string Volume(int v) => En ? $"Volume: {v}%" : $"Громкость: {v}%";
    public static string HotkeysHint => En
        ? "Keys: 1-5 voices, T effect, M mute, L monitor, F1-F12 pads"
        : "Клавиши: 1-5 голоса, T эффект, M мут, L монитор, F1-F12 пады";

    // Пады
    public static string Pad(int i) => En ? $"Pad {i}" : $"Пад {i}";
    public static string ColNum => "№";
    public static string ColName => En ? "Name" : "Название";
    public static string ColDur => En ? "Len" : "Длит.";
    public static string ColKey => En ? "Key" : "Клавиша";
    public static string PadGain => En ? "Pad volume:" : "Громкость падов:";
    public static string StopPads => En ? "Stop all" : "Стоп все";
    public static string LoadSound => En ? "Load sound…" : "Загрузить звук…";
    public static string Clear => En ? "Clear" : "Очистить";
    public static string LoadFailed => En ? "Could not load file." : "Не удалось загрузить файл.";
    public static string PlayTip => En ? "Double-click / Enter — play. Right-click — load/clear." : "Двойной клик / Enter — играть. Правый клик — загрузить/очистить.";

    // Уровни и устройства
    public static string LevelIn => En ? "INPUT (MIC)" : "ВХОД (MIC)";
    public static string LevelOut => En ? "OUTPUT (MOD + pads)" : "ВЫХОД (MOD + пады)";
    public static string VirtMic => En ? "System mic (for Discord/CS):" : "Микрофон в системе (для Discord/CS):";
    public static string NoCable => En ? "Cable not selected — nothing to output" : "Кабель не выбран — в Discord нечего выводить";
    public static string CableHint => En
        ? "If no cable in list — install VB-CABLE, then pick it in Settings."
        : "Если кабеля нет в списке — установите VB-CABLE и выберите его в настройках.";
    public static string ActiveDevices => En ? "Active Devices" : "Текущие устройства";
    public static string SetDevicesBtn => En ? "Configure Devices…" : "Настроить устройства…";
    public static string MasterVolume(int v) => En ? $"Master mic volume: {v}%" : $"Громкость микрофона: {v}%";
    public static string NotSelected => En ? "Not selected" : "Не выбрано";
    public static string MonitorOff => En ? "Disabled" : "Отключено";
    public static string PlayPad => En ? "Play" : "Играть";
    public static string StopPad => En ? "Stop" : "Стоп";
    public static string CustomVoiceHint => En
        ? "Sliders configure preset 5 (Custom)"
        : "Ползунки настраивают пресет 5 (Свой)";
    public static string BadgeLive => En ? "LIVE" : "В ЭФИРЕ";
    public static string BadgeStopped => En ? "STOPPED" : "ОСТАНОВЛЕН";

    // Статус
    public static string Stopped => En ? "Stopped" : "Остановлен";
    public static string Live(string inp, string cable, string voice) => En
        ? $"Live: {inp} → {cable} • {voice}" : $"В эфире: {inp} → {cable} • {voice}";
    public static string NeedDevices => En
        ? "Pick a microphone and a virtual cable (Settings)." : "Выберите микрофон и виртуальный кабель (Настройки).";
    public static string StartFailed => En ? "Could not start audio: " : "Не удалось запустить звук: ";

    // Настройки
    public static string SettingsTitle => En ? "Settings — OSP" : "Настройки — OSP";
    public static string SetDevices => En ? "Devices" : "Устройства";
    public static string SetMic => En ? "Microphone:" : "Микрофон:";
    public static string SetCable => En ? "Virtual cable (OSP output):" : "Виртуальный кабель (выход OSP):";
    public static string SetCableFallback => En
        ? "Output device (VB-CABLE not detected, pick any output):"
        : "Устройство вывода (VB-CABLE не обнаружен, выберите другой выход):";
    public static string SetMonitor => En ? "Headphones (monitor):" : "Наушники (монитор):";
    public static string SetVirtInSystem => En ? "System mic (pick in Discord):" : "Микрофон в системе (выбрать в Discord):";
    public static string SetVirtFallback => En ? "Output routing:" : "Маршрутизация звука:";
    public static string Refresh => En ? "Refresh" : "Обновить";
    public static string SetParams => En ? "Parameters" : "Параметры";
    public static string Pregain(double v) => En ? $"Input pregain: x{v:0.0}" : $"Предусиление входа: x{v:0.0}";
    public static string Gate(double v) => En ? $"Noise gate: {v:0.000}" : $"Шумодав: {v:0.000}";
    public static string SetLang => En ? "Language:" : "Язык:";
    public static string SetConfigFile => En ? "Config file:" : "Файл настроек:";
    public static string OpenFolder => En ? "Open folder" : "Открыть папку";
    public static string ResetAll => En ? "Reset all" : "Сбросить всё";
    public static string ResetAsk => En
        ? "Reset all settings (voices, volumes, devices)?\nPad files are kept."
        : "Сбросить все настройки (голоса, громкости, устройства)?\nФайлы падов сохранятся.";
    public static string SetAbout => En ? "About OpenSoundPad" : "О приложении";
    public static string SetAboutDesc => En
        ? "Real-time voice changer and soundpad with direct routing to Windows virtual microphone."
        : "Голосовой модулятор реального времени и саундпад с выводом в виртуальный микрофон Windows.";
    public static string Ok => "OK";
    public static string Cancel => En ? "Cancel" : "Отмена";
}
