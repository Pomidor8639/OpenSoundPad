namespace OpenSoundPad;

/// <summary>
/// Локализация приложения: ru, en, de, es, fr.
/// </summary>
public static class Loc
{
    public static string Lang = "en";

    public static bool IsRu => Lang == "ru";
    public static bool IsEn => Lang == "en";
    public static bool IsDe => Lang == "de";
    public static bool IsEs => Lang == "es";
    public static bool IsFr => Lang == "fr";

    public static string T(string ru, string en, string de, string es, string fr) => Lang switch
    {
        "en" => en,
        "de" => de,
        "es" => es,
        "fr" => fr,
        _ => ru
    };

    // Окно
    public static string WinMinimize => T("Свернуть", "Minimize", "Minimieren", "Minimizar", "Réduire");
    public static string WinMaximize => T("Развернуть", "Maximize", "Maximieren", "Maximizar", "Agrandir");
    public static string WinRestore => T("Восстановить", "Restore", "Wiederherstellen", "Restaurar", "Restaurer");
    public static string WinClose => T("Закрыть", "Close", "Schließen", "Cerrar", "Fermer");

    // Меню
    public static string MenuFile => T("Файл", "File", "Datei", "Archivo", "Fichier");
    public static string MenuOpenFolder => T("Открыть папку конфига", "Open config folder", "Konfigurationsordner öffnen", "Abrir carpeta de configuración", "Ouvrir dossier de config");
    public static string MenuExit => T("Выход", "Exit", "Beenden", "Salir", "Quitter");
    public static string MenuSettings => T("Настройки", "Settings", "Einstellungen", "Configuración", "Paramètres");
    public static string MenuOpenSettings => T("Настройки…", "Settings…", "Einstellungen…", "Configuración…", "Paramètres…");
    public static string MenuHelp => T("Справка", "Help", "Hilfe", "Ayuda", "Aide");
    public static string MenuAbout => T("О программе", "About", "Über", "Acerca de", "À propos");
    public static string AboutText => T(
        "OSP OpenSoundPad\nВойсчейнджер + саундпад.\nВывод: виртуальный кабель (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nVoice changer + soundpad.\nOutput: virtual cable (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nStimmenverzerrer + Soundpad.\nAusgang: virtuelles Kabel (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nCambiador de voz + soundpad.\nSalida: cable virtual (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nModificateur de voix + soundpad.\nSortie : câble virtuel (VB-CABLE / Animaze / Voicemod).");

    // Режимы
    public static string ModeVoice => T("Войсмод", "Voice Changer", "Stimmenverzerrer", "Cambiador de voz", "Modificateur de voix");
    public static string ModePad => T("Саундпад", "Soundpad", "Soundpad", "Soundpad", "Soundpad");
    public static string VoiceStudio => T("Студия кастомного голоса", "Custom Voice Studio", "Eigenes Stimmstudio", "Estudio de voz", "Studio de voix");
    public static string VoicePresets => T("Голосовые пресеты", "Voice Presets", "Stimmvoreinstellungen", "Preajustes de voz", "Préréglages vocaux");
    public static string ResetVoiceParams => T("Сбросить параметры", "Reset sliders", "Parameter zurücksetzen", "Restablecer controles", "Réinitialiser curseurs");
    public static string ClearAllPads => T("Очистить всё", "Clear all", "Alle leeren", "Borrar todo", "Tout effacer");
    public static string ClearAllAsk => T("Удалить все звуки из списка?", "Remove all sounds from the list?", "Alle Sounds aus der Liste entfernen?", "¿Eliminar todos los sonidos de la lista?", "Supprimer tous les sons de la liste ?");
    public static string AddSound => T("Добавить звук…", "Add sound…", "Sound hinzufügen…", "Añadir sonido…", "Ajouter un son…");
    public static string RemoveSound => T("Удалить", "Remove", "Entfernen", "Eliminar", "Supprimer");
    public static string RenameSound => T("Переименовать…", "Rename…", "Umbenennen…", "Renombrar…", "Renommer…");
    public static string EnterNewName => T("Введите новое название для звука:", "Enter new sound name:", "Geben Sie einen neuen Soundnamen ein:", "Introduce el nuevo nombre del sonido:", "Entrez le nouveau nom du son :");
    public static string PadVolumeTitle => T("ГРОМКОСТЬ", "PAD VOLUME", "PAD-LAUTSTÄRKE", "VOLUMEN DE PADS", "VOLUME DU PAD");
    public static string PlayingCount(int count) => T($"Играет: {count}", $"Active: {count}", $"Aktiv: {count}", $"Activos: {count}", $"Actifs : {count}");
    public static string SoundpadRouting => T("Трансляция в кабель:", "Routing to Cable:", "Übertragung an Kabel:", "Enrutamiento al cable:", "Routage vers le câble :");
    public static string SoundpadHint => T(
        "Правый клик по треку: удалить / переименовать • Перетащите файлы сюда или нажмите «+ Добавить звук»",
        "Right-click track to remove / rename • Drag files here or click «+ Add sound»",
        "Rechtsklick: Entfernen / Umbenennen • Dateien hierher ziehen oder «+ Sound hinzufügen»",
        "Clic derecho: eliminar / renombrar • Arrastra archivos aquí o haz clic en «+ Añadir sonido»",
        "Clic droit : supprimer / renommer • Glissez des fichiers ici ou cliquez «+ Ajouter un son»");
    public static string NoSoundsYet => T(
        "Звуки пока не добавлены. Нажмите «+ Добавить звук» или перетащите аудиофайлы сюда.",
        "No sounds yet. Click «+ Add sound» or drag & drop audio files here.",
        "Noch keine Sounds. Klicken Sie auf «+ Sound hinzufügen» oder Dateien hierher ziehen.",
        "Sin sonidos aún. Haz clic en «+ Añadir sonido» o arrastra archivos de audio aquí.",
        "Aucun son pour le moment. Cliquez sur «+ Ajouter un son» ou glissez des fichiers audio.");
    public static string VoiceHotkeysHint => T(
        "Клавиши: 1-5 пресеты, T эффект вкл/выкл, M мут микрофона, L монитор",
        "Keys: 1-5 voice presets, T effect bypass, M mute mic, L monitor",
        "Tasten: 1-5 Stimmen, T Effekt ein/aus, M Stummschalten, L Abhören",
        "Teclas: 1-5 voces, T alternar efecto, M silenciar, L monitorizar",
        "Touches : 1-5 voix, T bypass effet, M couper micro, L écoute");
    public static string PadHotkeysHint => T(
        "Клавиши: F1-F12 запуск, Space/Enter играть выбранный, Del удалить, Esc стоп всех",
        "Keys: F1-F12 play pads, Space/Enter play selected, Del remove, Esc stop all",
        "Tasten: F1-F12 abspielen, Leertaste/Enter abspielen, Entf löschen, Esc alle stoppen",
        "Teclas: F1-F12 reproducir, Espacio/Enter reproducir, Supr eliminar, Esc detener todo",
        "Touches : F1-F12 jouer, Espace/Entrée jouer sélection, Suppr effacer, Échap tout stopper");

    public static string VoiceDesc(int id) => id switch
    {
        1 => T("Маскировка тембра, сдвиг -7/-13.5 st, перегруз", "Timbre disguise, multi-layer pitch shift, overdrive", "Stimmenverschleierung, Tonhöhenverschiebung, Overdrive", "Disfraz de timbre, cambio de tono multicapa, distorsión", "Masquage du timbre, décalage de hauteur, overdrive"),
        2 => T("Формантный сдвиг +3.8 st, осветление", "Formant shift +3.8 st, bright harmonics", "Formantverschiebung +3.8 st, helle Harmonien", "Cambio de formantes +3.8 st, armónicos brillantes", "Décalage formantique +3.8 st, harmoniques claires"),
        3 => T("Высокий голос +6.0 st, звонкий", "High pitch +6.0 st, child tone", "Hohe Tonhöhe +6.0 st, Kinderstimme", "Tono agudo +6.0 st, voz infantil", "Voix aiguë +6.0 st, tonalité enfant"),
        4 => T("Глубокий рык -9/-15.5 st, массивный перегруз", "Deep growl -9/-15.5 st, massive overdrive", "Tiefes Knurren -9/-15.5 st, massiver Overdrive", "Gruñido profundo -9/-15.5 st, distorsión masiva", "Gronderie profonde -9/-15.5 st, distorsion massive"),
        _ => T("Индивидуальная ручная настройка параметров", "Fully custom parameters", "Individuelle manuelle Einstellung", "Ajuste manual personalizado", "Paramètres entièrement personnalisés"),
    };

    // Тулбар
    public static string TbStart => T("Старт", "Start", "Start", "Iniciar", "Démarrer");
    public static string TbStop => T("Стоп", "Stop", "Stopp", "Detener", "Arrêter");
    public static string TbEffectOff => T("Эффект выкл (T)", "Bypass (T)", "Bypass (T)", "Sin efecto (T)", "Bypass (T)");
    public static string TbEffectOn => T("Эффект вкл (T)", "Effect on (T)", "Effekt ein (T)", "Efecto activo (T)", "Effet actif (T)");
    public static string TbMute => T("Мут (M)", "Mute (M)", "Stumm (M)", "Silenciar (M)", "Couper (M)");
    public static string TbUnmute => T("Анмут (M)", "Unmute (M)", "Ton an (M)", "Reactivar (M)", "Activer (M)");
    public static string TbMonitor => T("Монитор (L)", "Monitor (L)", "Monitor (L)", "Monitor (L)", "Écoute (L)");
    public static string TbMonitorOn => T("Монитор: ВКЛ (L)", "Monitor: ON (L)", "Monitor: EIN (L)", "Monitor: ACTIVO (L)", "Écoute : ON (L)");

    // Группы
    public static string GrVoices => T("Голоса", "Voices", "Stimmen", "Voces", "Voix");
    public static string GrPads => T("Саундпад", "Soundpad", "Soundpad", "Soundpad", "Soundpad");
    public static string GrLevels => T("Уровни", "Levels", "Pegel", "Niveles", "Niveaux");
    public static string Effect => T("Эффект:", "Effect:", "Effekt:", "Efecto:", "Effet :");
    public static string EffectActive => T("АКТИВЕН", "ACTIVE", "AKTIV", "ACTIVO", "ACTIF");
    public static string EffectBypass => T("ОРИГИНАЛ", "ORIGINAL", "ORIGINAL", "ORIGINAL", "ORIGINAL");
    public static string CustomVoice => T("Свой голос (пресет 5):", "Custom voice (preset 5):", "Eigene Stimme (Preset 5):", "Voz personalizada (preajuste 5):", "Voix personnalisée (preset 5) :");

    public static string VoiceName(int id) => id switch
    {
        1 => T("Аноним", "Anonymous", "Anonym", "Anónimo", "Anonyme"),
        2 => T("Женский", "Female", "Weiblich", "Femenina", "Féminine"),
        3 => T("Ребенок", "Kid", "Kind", "Niño", "Enfant"),
        4 => T("Демон", "Demon", "Dämon", "Demonio", "Démon"),
        _ => T("Свой", "Custom", "Benutzerdefiniert", "Personalizada", "Personnalisée"),
    };

    public static string PitchTitle => T("ПИТЧ", "PITCH", "TONHÖHE", "TONO", "HAUTEUR");
    public static string DriveTitle => T("ДИСТОРШН", "DISTORTION", "VERZERRUNG", "DISTORSIÓN", "DISTORSION");
    public static string BassTitle => T("БАС", "BASS", "BASS", "GRAVES", "BASSES");
    public static string RobotTitle => T("РОБОТИЗАЦИЯ", "ROBOT", "ROBOTER", "ROBOT", "ROBOT");
    public static string VolumeTitle => T("ГРОМКОСТЬ МИКРОФОНА", "MIC VOLUME", "MIKROFON-LAUTSTÄRKE", "VOLUMEN DE MICRÓFONO", "VOLUME DU MICRO");

    public static string Pitch(double v) => T($"Питч: {v:+0.0;-0.0} st", $"Pitch: {v:+0.0;-0.0} st", $"Tonhöhe: {v:+0.0;-0.0} st", $"Tono: {v:+0.0;-0.0} st", $"Hauteur : {v:+0.0;-0.0} st");
    public static string Drive(int v) => T($"Дисторшн: {v}%", $"Distortion: {v}%", $"Verzerrung: {v}%", $"Distorsión: {v}%", $"Distorsion : {v}%");
    public static string Bass(double v) => T($"Бас: +{v:0.0} дБ", $"Bass: +{v:0.0} dB", $"Bass: +{v:0.0} dB", $"Graves: +{v:0.0} dB", $"Basses : +{v:0.0} dB");
    public static string Robot(int v) => T($"Робот: {v}%", $"Robot: {v}%", $"Roboter: {v}%", $"Robot: {v}%", $"Robot : {v}%");
    public static string Volume(int v) => T($"Громкость: {v}%", $"Volume: {v}%", $"Lautstärke: {v}%", $"Volumen: {v}%", $"Volume : {v}%");
    public static string HotkeysHint => T(
        "Клавиши: 1-5 голоса, T эффект, M мут, L монитор, F1-F12 пады",
        "Keys: 1-5 voices, T effect, M mute, L monitor, F1-F12 pads",
        "Tasten: 1-5 Stimmen, T Effekt, M Stumm, L Monitor, F1-F12 Pads",
        "Teclas: 1-5 voces, T efecto, M silenciar, L monitor, F1-F12 pads",
        "Touches : 1-5 voix, T effet, M couper, L écoute, F1-F12 pads");

    // Пады
    public static string Pad(int i) => T($"Пад {i}", $"Pad {i}", $"Pad {i}", $"Pad {i}", $"Pad {i}");
    public static string ColNum => "№";
    public static string ColName => T("Название сэмпла", "Sample name", "Sample-Name", "Nombre de muestra", "Nom de l'échantillon");
    public static string ColDur => T("Длит.", "Len", "Dauer", "Dur.", "Durée");
    public static string ColKey => T("Клавиша", "Key", "Taste", "Tecla", "Touche");
    public static string PadGain => T("Громкость падов:", "Pad volume:", "Pad-Lautstärke:", "Volumen de pads:", "Volume des pads :");
    public static string StopPads => T("Стоп все", "Stop all", "Alle stoppen", "Detener todo", "Tout stopper");
    public static string LoadSound => T("Загрузить звук…", "Load sound…", "Sound laden…", "Cargar sonido…", "Charger un son…");
    public static string Clear => T("Очистить", "Clear", "Löschen", "Limpiar", "Effacer");
    public static string LoadFailed => T("Не удалось загрузить файл.", "Could not load file.", "Datei konnte nicht geladen werden.", "No se pudo cargar el archivo.", "Impossible de charger le fichier.");
    public static string PlayTip => T(
        "Двойной клик / Enter — играть. Правый клик — переименовать/удалить.",
        "Double-click / Enter — play. Right-click — rename/remove.",
        "Doppelklick / Enter — abspielen. Rechtsklick — umbenennen/löschen.",
        "Doble clic / Enter — reproducir. Clic derecho — renombrar/eliminar.",
        "Double-clic / Entrée — jouer. Clic droit — renommer/supprimer.");

    // Уровни и устройства
    public static string LevelIn => T("ВХОД МИКРОФОНА", "INPUT (MIC)", "EINGANG (MIKROFON)", "ENTRADA (MIC)", "ENTRÉE (MIC)");
    public static string LevelOut => T("ВЫХОД В ЭФИР (ОБРАБОТКА + ПАДЫ)", "OUTPUT (MOD + PADS)", "AUSGANG (MOD + PADS)", "SALIDA (MOD + PADS)", "SORTIE (MOD + PADS)");
    public static string VirtMic => T("Микрофон в системе (для Discord/CS):", "System mic (for Discord/CS):", "Systemmikrofon (für Discord/Spiele):", "Micrófono del sistema (Discord/juegos):", "Micro système (pour Discord/jeux) :");
    public static string NoCable => T("Кабель не выбран — в Discord нечего выводить", "Cable not selected — nothing to output", "Kein Kabel gewählt — keine Ausgabe", "Cable no seleccionado — nada que emitir", "Câble non sélectionné — aucune sortie");
    public static string CableHint => T(
        "Если кабеля нет в списке — установите VB-CABLE и выберите его в настройках.",
        "If no cable in list — install VB-CABLE, then pick it in Settings.",
        "Falls kein Kabel vorhanden: VB-CABLE installieren und in Einstellungen wählen.",
        "Si no hay cable en la lista: instala VB-CABLE y elígelo en Configuración.",
        "Si aucun câble : installez VB-CABLE et sélectionnez-le dans les Paramètres.");
    public static string ActiveDevices => T("Текущие устройства", "Active Devices", "Aktive Geräte", "Dispositivos activos", "Périphériques actifs");
    public static string SetDevicesBtn => T("Настроить устройства…", "Configure Devices…", "Geräte konfigurieren…", "Configurar dispositivos…", "Configurer périphériques…");
    public static string MasterVolume(int v) => T($"Громкость микрофона: {v}%", $"Master mic volume: {v}%", $"Haupt-Mikrofonlautstärke: {v}%", $"Volumen de micrófono: {v}%", $"Volume du micro : {v}%");
    public static string NotSelected => T("Не выбрано", "Not selected", "Nicht ausgewählt", "No seleccionado", "Non sélectionné");
    public static string MonitorOff => T("Отключено", "Disabled", "Deaktiviert", "Desactivado", "Désactivé");
    public static string PlayPad => T("Играть", "Play", "Abspielen", "Reproducir", "Jouer");
    public static string StopPad => T("Стоп", "Stop", "Stopp", "Detener", "Stopper");
    public static string CustomVoiceHint => T("Ползунки настраивают пресет 5 (Свой)", "Sliders configure preset 5 (Custom)", "Regler konfigurieren Voreinstellung 5 (Benutzerdefiniert)", "Los controles configuran el preajuste 5 (Personalizado)", "Les curseurs configurent le préréglage 5 (Personnalisé)");
    public static string BadgeLive => T("В ЭФИРЕ", "LIVE", "LIVE", "EN VIVO", "EN DIRECT");
    public static string BadgeStopped => T("ОСТАНОВЛЕН", "STOPPED", "GESTOPPT", "DETENIDO", "ARRÊTÉ");

    // Статус
    public static string Stopped => T("Остановлен", "Stopped", "Gestoppt", "Detenido", "Arrêté");
    public static string Live(string inp, string cable, string voice) => T(
        $"В эфире: {inp} → {cable} • {voice}",
        $"Live: {inp} → {cable} • {voice}",
        $"Live: {inp} → {cable} • {voice}",
        $"En vivo: {inp} → {cable} • {voice}",
        $"En direct : {inp} → {cable} • {voice}");
    public static string NeedDevices => T(
        "Выберите микрофон и виртуальный кабель (Настройки).",
        "Pick a microphone and a virtual cable (Settings).",
        "Wählen Sie ein Mikrofon und virtuelles Kabel (Einstellungen).",
        "Elige un micrófono y cable virtual (Configuración).",
        "Choisissez un micro et câble virtuel (Paramètres).");
    public static string StartFailed => T("Не удалось запустить звук: ", "Could not start audio: ", "Audiowiedergabe konnte nicht gestartet werden: ", "No se pudo iniciar el audio: ", "Impossible de démarrer l'audio : ");

    // Настройки
    public static string SettingsTitle => T("Настройки — OSP", "Settings — OSP", "Einstellungen — OSP", "Configuración — OSP", "Paramètres — OSP");
    public static string SetDevices => T("Аудиоустройства", "Audio Devices", "Audiogeräte", "Dispositivos de audio", "Périphériques audio");
    public static string SetMic => T("Микрофон (вход):", "Microphone (input):", "Mikrofon (Eingang):", "Micrófono (entrada):", "Microphone (entrée) :");
    public static string SetCable => T("Виртуальный кабель (выход в Discord / игры):", "Virtual cable (output to Discord / games):", "Virtuelles Kabel (Ausgang für Discord / Spiele):", "Cable virtual (salida para Discord / juegos):", "Câble virtuel (sortie Discord / jeux) :");
    public static string SetCableFallback => T(
        "Устройство вывода (VB-CABLE не обнаружен, выберите другой выход):",
        "Output device (VB-CABLE not detected, pick any output):",
        "Ausgabegerät (VB-CABLE nicht gefunden, beliebigen Ausgang wählen):",
        "Dispositivo de salida (VB-CABLE no detectado, elige otra salida):",
        "Périphérique de sortie (VB-CABLE non détecté, choisissez une sortie) :");
    public static string SetMonitor => T("Наушники (мониторинг себя):", "Headphones (self monitor):", "Kopfhörer (Eigenabhörung):", "Auriculares (monitorización propia):", "Casque (écoute de soi) :");
    public static string SetVirtInSystem => T("Микрофон в системе (выбрать в Discord/CS):", "System mic (pick in Discord/games):", "Systemmikrofon (in Discord/Spielen wählen):", "Micrófono del sistema (elegir en Discord/juegos):", "Micro système (à choisir dans Discord/jeux) :");
    public static string SetVirtFallback => T("Маршрутизация звука:", "Output routing:", "Audio-Routing:", "Enrutamiento de audio:", "Routage audio :");
    public static string Refresh => T("Обновить список устройств", "Refresh device list", "Geräteliste aktualisieren", "Actualizar lista de dispositivos", "Actualiser la liste des périphériques");
    public static string SetParams => T("Обработка звука", "Audio Processing", "Audioverarbeitung", "Procesamiento de audio", "Traitement audio");
    public static string Pregain(double v) => T($"Предусиление входа: x{v:0.0}", $"Input pregain: x{v:0.0}", $"Eingangsverstärkung: x{v:0.0}", $"Preamplificación de entrada: x{v:0.0}", $"Pré-gain d'entrée : x{v:0.0}");
    public static string Gate(double v) => T($"Шумодав (Noise Gate): {v:0.000}", $"Noise gate: {v:0.000}", $"Rauschunterdrückung: {v:0.000}", $"Puerta de ruido: {v:0.000}", $"Porte de bruit : {v:0.000}");
    public static string SetLang => T("Язык / Language", "Language / Язык", "Sprache / Language", "Idioma / Language", "Langue / Language");
    public static string SetConfigFile => T("Файл настроек:", "Config file:", "Konfigurationsdatei:", "Archivo de configuración:", "Fichier de configuration :");
    public static string OpenFolder => T("Открыть папку", "Open folder", "Ordner öffnen", "Abrir carpeta", "Ouvrir dossier");
    public static string ResetAll => T("Сбросить всё", "Reset all", "Alles zurücksetzen", "Restablecer todo", "Tout réinitialiser");
    public static string ResetAsk => T(
        "Сбросить все настройки (голоса, громкости, устройства)?\nФайлы падов сохранятся.",
        "Reset all settings (voices, volumes, devices)?\nPad files are kept.",
        "Alle Einstellungen zurücksetzen?\nSounddateien bleiben erhalten.",
        "¿Restablecer todos los ajustes?\nLos sonidos se mantendrán.",
        "Réinitialiser tous les paramètres ?\nLes fichiers sons seront conservés.");
    public static string SetAbout => T("О приложении", "About OpenSoundPad", "Über OpenSoundPad", "Acerca de OpenSoundPad", "À propos d'OpenSoundPad");
    public static string SetAboutDesc => T(
        "Голосовой модулятор реального времени и саундпад с выводом в виртуальный микрофон Windows.",
        "Real-time voice changer and soundpad with direct routing to Windows virtual microphone.",
        "Echtzeit-Stimmenverzerrer und Soundpad mit Routing zum virtuellen Windows-Mikrofon.",
        "Modulador de voz en tiempo real y soundpad con enrutamiento al micrófono virtual de Windows.",
        "Modulateur vocal en temps réel et soundpad avec routage vers micro virtuel Windows.");
    public static string Ok => T("OK", "OK", "OK", "Aceptar", "Valider");
    public static string Cancel => T("Отмена", "Cancel", "Abbrechen", "Cancelar", "Annuler");

    // Экран выбора языка при первом запуске
    public static string WelcomeTitle => T("Добро пожаловать в OpenSoundPad", "Welcome to OpenSoundPad", "Willkommen bei OpenSoundPad", "Bienvenido a OpenSoundPad", "Bienvenue sur OpenSoundPad");
    public static string SelectLanguagePrompt => T("Выберите язык интерфейса:", "Choose your language:", "Wählen Sie Ihre Sprache:", "Elige tu idioma:", "Choisissez votre langue :");
    public static string ContinueBtn => T("Продолжить", "Continue", "Weiter", "Continuar", "Continuer");
}
