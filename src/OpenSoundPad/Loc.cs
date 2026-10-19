namespace OpenSoundPad;

/// <summary>
/// Локализация приложения: ru, en, de, es, fr, zh, ja, ko.
/// </summary>
public static class Loc
{
    public static string Lang = "en";

    public static bool IsRu => Lang == "ru";
    public static bool IsEn => Lang == "en";
    public static bool IsDe => Lang == "de";
    public static bool IsEs => Lang == "es";
    public static bool IsFr => Lang == "fr";
    public static bool IsZh => Lang == "zh";
    public static bool IsJa => Lang == "ja";
    public static bool IsKo => Lang == "ko";

    public static string T(string ru, string en, string de, string es, string fr, string zh = "", string ja = "", string ko = "") => Lang switch
    {
        "en" => en,
        "de" => de,
        "es" => es,
        "fr" => fr,
        "zh" => string.IsNullOrEmpty(zh) ? en : zh,
        "ja" => string.IsNullOrEmpty(ja) ? en : ja,
        "ko" => string.IsNullOrEmpty(ko) ? en : ko,
        _ => ru
    };

    // Окно
    public static string WinMinimize => T("Свернуть", "Minimize", "Minimieren", "Minimizar", "Réduire", "最小化", "最小化", "최소화");
    public static string WinMaximize => T("Развернуть", "Maximize", "Maximieren", "Maximizar", "Agrandir", "最大化", "最大化", "최대화");
    public static string WinRestore => T("Восстановить", "Restore", "Wiederherstellen", "Restaurar", "Restaurer", "还原", "元に戻す", "이전 크기로");
    public static string WinClose => T("Закрыть", "Close", "Schließen", "Cerrar", "Fermer", "关闭", "閉じる", "닫기");

    // Меню
    public static string MenuFile => T("Файл", "File", "Datei", "Archivo", "Fichier", "文件", "ファイル", "파일");
    public static string MenuOpenFolder => T("Открыть папку конфига", "Open config folder", "Konfigurationsordner öffnen", "Abrir carpeta de configuración", "Ouvrir dossier de config", "打开配置文件夹", "設定フォルダーを開く", "설정 폴더 열기");
    public static string MenuExit => T("Выход", "Exit", "Beenden", "Salir", "Quitter", "退出", "終了", "종료");
    public static string MenuSettings => T("Настройки", "Settings", "Einstellungen", "Configuración", "Paramètres", "设置", "設定", "설정");
    public static string MenuOpenSettings => T("Настройки…", "Settings…", "Einstellungen…", "Configuración…", "Paramètres…", "设置…", "設定…", "설정…");
    public static string MenuHelp => T("Справка", "Help", "Hilfe", "Ayuda", "Aide", "帮助", "ヘルプ", "도움말");
    public static string MenuAbout => T("О программе", "About", "Über", "Acerca de", "À propos", "关于", "バージョン情報", "정보");
    public static string AboutText => T(
        "OSP OpenSoundPad\nВойсчейнджер + саундпад.\nВывод: виртуальный кабель (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nVoice changer + soundpad.\nOutput: virtual cable (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nStimmenverzerrer + Soundpad.\nAusgang: virtuelles Kabel (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nCambiador de voz + soundpad.\nSalida: cable virtual (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\nModificateur de voix + soundpad.\nSortie : câble virtuel (VB-CABLE / Animaze / Voicemod).",
        "OSP OpenSoundPad\n变声器 + 声音板。\n输出: 虚拟音频线 (VB-CABLE / Animaze / Voicemod)。",
        "OSP OpenSoundPad\nボイスチェンジャー + サウンドパッド。\n出力: 仮想オーディオケーブル (VB-CABLE / Animaze / Voicemod)。",
        "OSP OpenSoundPad\n보이스 체인저 + 사운드패드.\n출력: 가상 오디오 케이블 (VB-CABLE / Animaze / Voicemod)。");

    // Режимы
    public static string ModeVoice => T("Войсмод", "Voice Changer", "Stimmenverzerrer", "Cambiador de voz", "Modificateur de voix", "变声器", "ボイスチェンジャー", "보이스 체인저");
    public static string ModePad => T("Саундпад", "Soundpad", "Soundpad", "Soundpad", "Soundpad", "声音板", "サウンドパッド", "사운드패드");
    public static string VoiceStudio => T("Студия кастомного голоса", "Custom Voice Studio", "Eigenes Stimmstudio", "Estudio de voz", "Studio de voix", "自定义声音工作室", "カスタムボイススタジオ", "커스텀 보이스 스튜디오");
    public static string VoicePresets => T("Голосовые пресеты", "Voice Presets", "Stimmvoreinstellungen", "Preajustes de voz", "Préréglages vocaux", "语音预设", "ボイスプリセット", "음성 프리셋");
    public static string ResetVoiceParams => T("Сбросить параметры", "Reset sliders", "Parameter zurücksetzen", "Restablecer controles", "Réinitialiser curseurs", "重置参数", "パラメータをリセット", "설정 초기화");
    public static string ClearAllPads => T("Очистить всё", "Clear all", "Alle leeren", "Borrar todo", "Tout effacer", "清空全部", "すべてクリア", "모두 지우기");
    public static string ClearAllAsk => T("Удалить все звуки из списка?", "Remove all sounds from the list?", "Alle Sounds aus der Liste entfernen?", "¿Eliminar todos los sonidos de la lista?", "Supprimer tous les sons de la liste ?", "确定从列表中移除所有声音吗？", "リストからすべてのサウンドを削除しますか？", "목록의 모든 사운드를 삭제하시겠습니까?");
    public static string AddSound => T("Добавить звук…", "Add sound…", "Sound hinzufügen…", "Añadir sonido…", "Ajouter un son…", "添加声音…", "サウンドを追加…", "사운드 추가…");
    public static string RemoveSound => T("Удалить", "Remove", "Entfernen", "Eliminar", "Supprimer", "删除", "削除", "삭제");
    public static string RenameSound => T("Переименовать…", "Rename…", "Umbenennen…", "Renombrar…", "Renommer…", "重命名…", "名前を変更…", "이름 바꾸기…");
    public static string EnterNewName => T("Введите новое название для звука:", "Enter new sound name:", "Geben Sie einen neuen Soundnamen ein:", "Introduce el nuevo nombre del sonido:", "Entrez le nouveau nom du son :", "输入声音的新名称:", "サウンドの新しい名前を入力してください:", "새 사운드 이름을 입력하세요:");
    public static string PadVolumeTitle => T("ГРОМКОСТЬ", "PAD VOLUME", "PAD-LAUTSTÄRKE", "VOLUMEN DE PADS", "VOLUME DU PAD", "声音板音量", "パッド音量", "패드 볼륨");
    public static string PlayingCount(int count) => T($"Играет: {count}", $"Active: {count}", $"Aktiv: {count}", $"Activos: {count}", $"Actifs : {count}", $"播放中: {count}", $"再生中: {count}", $"재생 중: {count}");
    public static string SoundpadRouting => T("Трансляция в кабель:", "Routing to Cable:", "Übertragung an Kabel:", "Enrutamiento al cable:", "Routage vers le câble :", "输出至虚拟线缆:", "仮想ケーブルへのルーティング:", "가상 케이블로 라우팅:");
    public static string SoundpadHint => T(
        "Правый клик по треку: удалить / переименовать • Перетащите файлы сюда или нажмите «+ Добавить звук»",
        "Right-click track to remove / rename • Drag files here or click «+ Add sound»",
        "Rechtsklick: Entfernen / Umbenennen • Dateien hierher ziehen oder «+ Sound hinzufügen»",
        "Clic derecho: eliminar / renombrar • Arrastra archivos aquí o haz clic en «+ Añadir sonido»",
        "Clic droit : supprimer / renommer • Glissez des fichiers ici ou cliquez «+ Ajouter un son»",
        "右键点击音轨: 删除 / 重命名 • 拖放音频文件至此处或点击「+ 添加声音」",
        "トラックを右クリック: 削除 / 名前の変更 • ファイルをここにドラッグ＆ドロップまたは「+ サウンドを追加」",
        "트랙 우클릭: 삭제 / 이름 변경 • 오디오 파일을 여기로 드래그하거나 «+ 사운드 추가» 클릭");
    public static string NoSoundsYet => T(
        "Звуки пока не добавлены. Нажмите «+ Добавить звук» или перетащите аудиофайлы сюда.",
        "No sounds yet. Click «+ Add sound» or drag & drop audio files here.",
        "Noch keine Sounds. Klicken Sie auf «+ Sound hinzufügen» oder Dateien hierher ziehen.",
        "Sin sonidos aún. Haz clic en «+ Añadir sonido» o arrastra archivos de audio aquí.",
        "Aucun son pour le moment. Cliquez sur «+ Ajouter un son» ou glissez des fichiers audio.",
        "尚未添加声音。点击「+ 添加声音」或直接拖放音频文件至此处。",
        "サウンドがまだ追加されていません。「+ サウンドを追加」をクリックするか、オーディオファイルをここにドラッグ＆ドロップしてください。",
        "추가된 사운드가 없습니다. «+ 사운드 추가»를 누르거나 오디오 파일을 여기로 끌어다 놓으세요.");
    public static string VoiceHotkeysHint => T(
        "Клавиши: 1-5 пресеты, T эффект вкл/выкл, M мут микрофона, L монитор",
        "Keys: 1-5 voice presets, T effect bypass, M mute mic, L monitor",
        "Tasten: 1-5 Stimmen, T Effekt ein/aus, M Stummschalten, L Abhören",
        "Teclas: 1-5 voces, T alternar efecto, M silenciar, L monitorizar",
        "Touches : 1-5 voix, T bypass effet, M couper micro, L écoute",
        "按键: 1-5 预设切换, T 效果开/关, M 静音麦克风, L 耳机监听",
        "キー: 1-5 プリセット, T エフェクト切替, M マイク消音, L モニター",
        "단축키: 1-5 프리셋 전환, T 효과 켜기/끄기, M 마이크 음소거, L 모니터링");
    public static string PadHotkeysHint => T(
        "Клавиши: F1-F12 запуск, Space/Enter играть выбранный, Del удалить, Esc стоп всех",
        "Keys: F1-F12 play pads, Space/Enter play selected, Del remove, Esc stop all",
        "Tasten: F1-F12 abspielen, Leertaste/Enter abspielen, Entf löschen, Esc alle stoppen",
        "Teclas: F1-F12 reproducir, Espacio/Enter reproducir, Supr eliminar, Esc detener todo",
        "Touches : F1-F12 jouer, Espace/Entrée jouer sélection, Suppr effacer, Échap tout stopper",
        "按键: F1-F12 播放声音, 空格/回车 播放选定项, Del 删除, Esc 停止所有",
        "キー: F1-F12 パッド再生, Space/Enter 選択再生, Del 削除, Esc すべて停止",
        "단축키: F1-F12 패드 재생, Space/Enter 선택 재생, Del 삭제, Esc 모두 정지");

    public static string VoiceDesc(int id) => id switch
    {
        1 => T("Маскировка тембра, сдвиг -7/-13.5 st, перегруз", "Timbre disguise, multi-layer pitch shift, overdrive", "Stimmenverschleierung, Tonhöhenverschiebung, Overdrive", "Disfraz de timbre, cambio de tono multicapa, distorsión", "Masquage du timbre, décalage de hauteur, overdrive", "音色伪装，多层音调偏移 -7/-13.5 st，失真", "声色偽装、マルチレイヤーピッチシフト -7/-13.5 st、オーバードライブ", "음색 변조, 멀티레이어 피치 이동 -7/-13.5 st, 오버드라이브"),
        2 => T("Формантный сдвиг +3.8 st, осветление", "Formant shift +3.8 st, bright harmonics", "Formantverschiebung +3.8 st, helle Harmonien", "Cambio de formantes +3.8 st, armónicos brillantes", "Décalage formantique +3.8 st, harmoniques claires", "共振峰偏移 +3.8 st，明亮谐波", "フォルマントシフト +3.8 st、明るいハーモニクス", "포먼트 이동 +3.8 st, 맑고 밝은 음색"),
        3 => T("Высокий голос +6.0 st, звонкий", "High pitch +6.0 st, child tone", "Hohe Tonhöhe +6.0 st, Kinderstimme", "Tono agudo +6.0 st, voz infantil", "Voix aiguë +6.0 st, tonalité enfant", "高音 +6.0 st，清脆童音", "ハイトーン +6.0 st、クリアな子供の声", "고음 +6.0 st, 맑은 어린이 목소리"),
        4 => T("Глубокий рык -9/-15.5 st, массивный перегруз", "Deep growl -9/-15.5 st, massive overdrive", "Tiefes Knurren -9/-15.5 st, massiver Overdrive", "Gruñido profundo -9/-15.5 st, distorsión masiva", "Gronderie profonde -9/-15.5 st, distorsion massive", "低沉低吼 -9/-15.5 st，强力过载失真", "重低音グラウル -9/-15.5 st、強力なディストーション", "굵은 포효음 -9/-15.5 st, 강력한 디스토션"),
        _ => T("Индивидуальная ручная настройка параметров", "Fully custom parameters", "Individuelle manuelle Einstellung", "Ajuste manual personalizado", "Paramètres entièrement personnalisés", "全自定义参数调节", "完全カスタムパラメータ設定", "완전 사용자 정의 매개변수 설정"),
    };

    // Тулбар
    public static string TbStart => T("Старт", "Start", "Start", "Iniciar", "Démarrer", "启动", "開始", "시작");
    public static string TbStop => T("Стоп", "Stop", "Stopp", "Detener", "Arrêter", "停止", "停止", "정지");
    public static string TbEffectOff => T("Эффект выкл (T)", "Bypass (T)", "Bypass (T)", "Sin efecto (T)", "Bypass (T)", "旁路原声 (T)", "バイパス (T)", "효과 끄기 (T)");
    public static string TbEffectOn => T("Эффект вкл (T)", "Effect on (T)", "Effekt ein (T)", "Efecto activo (T)", "Effet actif (T)", "效果开启 (T)", "エフェクトON (T)", "효과 켜기 (T)");
    public static string TbMute => T("Мут (M)", "Mute (M)", "Stumm (M)", "Silenciar (M)", "Couper (M)", "静音 (M)", "ミュート (M)", "음소거 (M)");
    public static string TbUnmute => T("Анмут (M)", "Unmute (M)", "Ton an (M)", "Reactivar (M)", "Activer (M)", "取消静音 (M)", "ミュート解除 (M)", "음소거 해제 (M)");
    public static string TbMonitor => T("Монитор (L)", "Monitor (L)", "Monitor (L)", "Monitor (L)", "Écoute (L)", "监听 (L)", "モニター (L)", "모니터링 (L)");
    public static string TbMonitorOn => T("Монитор: ВКЛ (L)", "Monitor: ON (L)", "Monitor: EIN (L)", "Monitor: ACTIVO (L)", "Écoute : ON (L)", "监听: 开启 (L)", "モニター: ON (L)", "모니터링: 켜짐 (L)");

    // Группы
    public static string GrVoices => T("Голоса", "Voices", "Stimmen", "Voces", "Voix", "声音预设", "ボイス", "목소리");
    public static string GrPads => T("Саундпад", "Soundpad", "Soundpad", "Soundpad", "Soundpad", "声音板", "サウンドパッド", "사운드패드");
    public static string GrLevels => T("Уровни", "Levels", "Pegel", "Niveles", "Niveaux", "电平", "レベル", "레벨");
    public static string Effect => T("Эффект:", "Effect:", "Effekt:", "Efecto:", "Effet :", "效果:", "エフェクト:", "효과:");
    public static string EffectActive => T("АКТИВЕН", "ACTIVE", "AKTIV", "ACTIVO", "ACTIF", "生效中", "アクティブ", "활성");
    public static string EffectBypass => T("ОРИГИНАЛ", "ORIGINAL", "ORIGINAL", "ORIGINAL", "ORIGINAL", "原声", "オリジナル", "원본");
    public static string CustomVoice => T("Свой голос (пресет 5):", "Custom voice (preset 5):", "Eigene Stimme (Preset 5):", "Voz personalizada (preajuste 5):", "Voix personnalisée (preset 5) :", "自定义声音 (预设 5):", "カスタムボイス (プリセット 5):", "커스텀 보이스 (프리셋 5):");

    public static string VoiceName(int id) => id switch
    {
        1 => T("Аноним", "Anonymous", "Anonym", "Anónimo", "Anonyme", "匿名", "匿名", "익명"),
        2 => T("Женский", "Female", "Weiblich", "Femenina", "Féminine", "女性", "女性", "여성"),
        3 => T("Ребенок", "Kid", "Kind", "Niño", "Enfant", "儿童", "子供", "어린이"),
        4 => T("Демон", "Demon", "Dämon", "Demonio", "Démon", "恶魔", "悪魔", "악마"),
        _ => T("Свой", "Custom", "Benutzerdefiniert", "Personalizada", "Personnalisée", "自定义", "カスタム", "사용자 정의"),
    };

    public static string PitchTitle => T("ПИТЧ", "PITCH", "TONHÖHE", "TONO", "HAUTEUR", "音调", "ピッチ", "피치");
    public static string DriveTitle => T("ДИСТОРШН", "DISTORTION", "VERZERRUNG", "DISTORSIÓN", "DISTORSION", "失真", "ディストーション", "디스토션");
    public static string BassTitle => T("БАС", "BASS", "BASS", "GRAVES", "BASSES", "低音", "低音", "베이스");
    public static string RobotTitle => T("РОБОТИЗАЦИЯ", "ROBOT", "ROBOTER", "ROBOT", "ROBOT", "机器人", "ロボット", "로봇");
    public static string VolumeTitle => T("ГРОМКОСТЬ МИКРОФОНА", "MIC VOLUME", "MIKROFON-LAUTSTÄRKE", "VOLUMEN DE MICRÓFONO", "VOLUME DU MICRO", "麦克风音量", "マイク音量", "마이크 볼륨");

    public static string Pitch(double v) => T($"Питч: {v:+0.0;-0.0} st", $"Pitch: {v:+0.0;-0.0} st", $"Tonhöhe: {v:+0.0;-0.0} st", $"Tono: {v:+0.0;-0.0} st", $"Hauteur : {v:+0.0;-0.0} st", $"音调: {v:+0.0;-0.0} st", $"ピッチ: {v:+0.0;-0.0} st", $"피치: {v:+0.0;-0.0} st");
    public static string Drive(int v) => T($"Дисторшн: {v}%", $"Distortion: {v}%", $"Verzerrung: {v}%", $"Distorsión: {v}%", $"Distorsion : {v}%", $"失真: {v}%", $"ディストーション: {v}%", $"디스토션: {v}%");
    public static string Bass(double v) => T($"Бас: +{v:0.0} дБ", $"Bass: +{v:0.0} dB", $"Bass: +{v:0.0} dB", $"Graves: +{v:0.0} dB", $"Basses : +{v:0.0} dB", $"低音: +{v:0.0} dB", $"低音: +{v:0.0} dB", $"베이스: +{v:0.0} dB");
    public static string Robot(int v) => T($"Робот: {v}%", $"Robot: {v}%", $"Roboter: {v}%", $"Robot: {v}%", $"Robot : {v}%", $"机器人: {v}%", $"ロボット: {v}%", $"로봇: {v}%");
    public static string Volume(int v) => T($"Громкость: {v}%", $"Volume: {v}%", $"Lautstärke: {v}%", $"Volumen: {v}%", $"Volume : {v}%", $"音量: {v}%", $"音量: {v}%", $"볼륨: {v}%");
    public static string HotkeysHint => T(
        "Клавиши: 1-5 голоса, T эффект, M мут, L монитор, F1-F12 пады",
        "Keys: 1-5 voices, T effect, M mute, L monitor, F1-F12 pads",
        "Tasten: 1-5 Stimmen, T Effekt, M Stumm, L Monitor, F1-F12 Pads",
        "Teclas: 1-5 voces, T efecto, M silenciar, L monitor, F1-F12 pads",
        "Touches : 1-5 voix, T effet, M couper, L écoute, F1-F12 pads",
        "按键: 1-5 声音, T 效果, M 静音, L 监听, F1-F12 声音板",
        "キー: 1-5 ボイス, T エフェクト, M ミュート, L モニター, F1-F12 パッド",
        "단축키: 1-5 음성, T 효과, M 음소거, L 모니터링, F1-F12 패드");

    // Пады
    public static string Pad(int i) => T($"Пад {i}", $"Pad {i}", $"Pad {i}", $"Pad {i}", $"Pad {i}", $"按键 {i}", $"パッド {i}", $"패드 {i}");
    public static string ColNum => "№";
    public static string ColName => T("Название сэмпла", "Sample name", "Sample-Name", "Nombre de muestra", "Nom de l'échantillon", "样本名称", "サンプル名", "샘플 이름");
    public static string ColDur => T("Длит.", "Len", "Dauer", "Dur.", "Durée", "时长", "長さ", "길이");
    public static string ColKey => T("Клавиша", "Key", "Taste", "Tecla", "Touche", "按键", "キー", "단축키");
    public static string PadGain => T("Громкость падов:", "Pad volume:", "Pad-Lautstärke:", "Volumen de pads:", "Volume des pads :", "声音板音量:", "パッド音量:", "패드 볼륨:");
    public static string StopPads => T("Стоп все", "Stop all", "Alle stoppen", "Detener todo", "Tout stopper", "停止所有", "すべて停止", "모두 정지");
    public static string LoadSound => T("Загрузить звук…", "Load sound…", "Sound laden…", "Cargar sonido…", "Charger un son…", "加载声音…", "サウンドを読み込む…", "사운드 불러오기…");
    public static string Clear => T("Очистить", "Clear", "Löschen", "Limpiar", "Effacer", "清空", "クリア", "지우기");
    public static string LoadFailed => T("Не удалось загрузить файл.", "Could not load file.", "Datei konnte nicht geladen werden.", "No se pudo cargar el archivo.", "Impossible de charger le fichier.", "无法加载文件。", "ファイルを読み込めませんでした。", "파일을 불러올 수 없습니다.");
    public static string PlayTip => T(
        "Двойной клик / Enter — играть. Правый клик — переименовать/удалить.",
        "Double-click / Enter — play. Right-click — rename/remove.",
        "Doppelklick / Enter — abspielen. Rechtsklick — umbenennen/löschen.",
        "Doble clic / Enter — reproducir. Clic derecho — renombrar/eliminar.",
        "Double-clic / Entrée — jouer. Clic droit — renommer/supprimer.",
        "双击 / 回车 — 播放。右键 — 重命名/删除。",
        "ダブルクリック / Enter — 再生。右クリック — 名前変更/削除。",
        "더블 클릭 / Enter — 재생. 우클릭 — 이름 변경/삭제.");

    // Уровни и устройства
    public static string LevelIn => T("ВХОД МИКРОФОНА", "INPUT (MIC)", "EINGANG (MIKROFON)", "ENTRADA (MIC)", "ENTRÉE (MIC)", "输入 (麦克风)", "入力 (マイク)", "입력 (마이크)");
    public static string LevelOut => T("ВЫХОД В ЭФИР (ОБРАБОТКА + ПАДЫ)", "OUTPUT (MOD + PADS)", "AUSGANG (MOD + PADS)", "SALIDA (MOD + PADS)", "SORTIE (MOD + PADS)", "输出 (变声 + 声音板)", "出力 (変調 + パッド)", "출력 (변조 + 패드)");
    public static string VirtMic => T("Микрофон в системе (для Discord/CS):", "System mic (for Discord/CS):", "Systemmikrofon (für Discord/Spiele):", "Micrófono del sistema (Discord/juegos):", "Micro système (pour Discord/jeux) :", "系统虚拟麦克风 (在 Discord/游戏 中选择):", "システムマイク (Discord/ゲームで選択):", "시스템 가상 마이크 (Discord/게임에서 선택):");
    public static string NoCable => T("Кабель не выбран — в Discord нечего выводить", "Cable not selected — nothing to output", "Kein Kabel gewählt — keine Ausgabe", "Cable no seleccionado — nada que emitir", "Câble non sélectionné — aucune sortie", "未选择虚拟音频线 — 无法输出到 Discord", "ケーブル未選択 — Discordへの出力なし", "케이블 미선택 — Discord로 출력 불가");
    public static string CableHint => T(
        "Если кабеля нет в списке — установите VB-CABLE и выберите его в настройках.",
        "If no cable in list — install VB-CABLE, then pick it in Settings.",
        "Falls kein Kabel vorhanden: VB-CABLE installieren und in Einstellungen wählen.",
        "Si no hay cable en la lista: instala VB-CABLE y elígelo en Configuración.",
        "Si aucun câble : installez VB-CABLE et sélectionnez-le dans les Paramètres.",
        "如果列表中没有音频线 — 请安装 VB-CABLE 并在设置中选择。",
        "リストにケーブルがない場合 — VB-CABLEをインストールして設定で選択してください。",
        "목록에 케이블이 없는 경우 — VB-CABLE을 설치하고 설정에서 선택하세요.");
    public static string ActiveDevices => T("Текущие устройства", "Active Devices", "Aktive Geräte", "Dispositivos activos", "Périphériques actifs", "当前设备", "現在のデバイス", "현재 장치");
    public static string SetDevicesBtn => T("Настроить устройства…", "Configure Devices…", "Geräte konfigurieren…", "Configurar dispositivos…", "Configurer périphériques…", "配置设备…", "デバイス設定…", "장치 설정…");
    public static string MasterVolume(int v) => T($"Громкость микрофона: {v}%", $"Master mic volume: {v}%", $"Haupt-Mikrofonlautstärke: {v}%", $"Volumen de micrófono: {v}%", $"Volume du micro : {v}%", $"麦克风主音量: {v}%", $"マスターマイク音量: {v}%", $"마이크 마스터 볼륨: {v}%");
    public static string NotSelected => T("Не выбрано", "Not selected", "Nicht ausgewählt", "No seleccionado", "Non sélectionné", "未选择", "未選択", "선택 안 됨");
    public static string MonitorOff => T("Отключено", "Disabled", "Deaktiviert", "Desactivado", "Désactivé", "已禁用", "無効", "비활성화");
    public static string PlayPad => T("Играть", "Play", "Abspielen", "Reproducir", "Jouer", "播放", "再生", "재생");
    public static string StopPad => T("Стоп", "Stop", "Stopp", "Detener", "Stopper", "停止", "停止", "정지");
    public static string CustomVoiceHint => T("Ползунки настраивают пресет 5 (Свой)", "Sliders configure preset 5 (Custom)", "Regler konfigurieren Voreinstellung 5 (Benutzerdefiniert)", "Los controles configuran el preajuste 5 (Personalizado)", "Les curseurs configurent le préréglage 5 (Personnalisé)", "滑块用于调节预设 5 (自定义)", "スライダーはプリセット5 (カスタム) を調整します", "슬라이더는 프리셋 5 (커스텀)를 조절합니다");
    public static string BadgeLive => T("В ЭФИРЕ", "LIVE", "LIVE", "EN VIVO", "EN DIRECT", "直播中", "配信中", "온에어");
    public static string BadgeStopped => T("ОСТАНОВЛЕН", "STOPPED", "GESTOPPT", "DETENIDO", "ARRÊTÉ", "已停止", "停止中", "정지됨");

    // Статус
    public static string Stopped => T("Остановлен", "Stopped", "Gestoppt", "Detenido", "Arrêté", "已停止", "停止中", "정지됨");
    public static string Live(string inp, string cable, string voice) => T(
        $"В эфире: {inp} → {cable} • {voice}",
        $"Live: {inp} → {cable} • {voice}",
        $"Live: {inp} → {cable} • {voice}",
        $"En vivo: {inp} → {cable} • {voice}",
        $"En direct : {inp} → {cable} • {voice}",
        $"已就绪: {inp} → {cable} • {voice}",
        $"配信中: {inp} → {cable} • {voice}",
        $"온에어: {inp} → {cable} • {voice}");
    public static string NeedDevices => T(
        "Выберите микрофон и виртуальный кабель (Настройки).",
        "Pick a microphone and a virtual cable (Settings).",
        "Wählen Sie ein Mikrofon und virtuelles Kabel (Einstellungen).",
        "Elige un micrófono y cable virtual (Configuración).",
        "Choisissez un micro et câble virtuel (Paramètres).",
        "请选择麦克风和虚拟音频线 (设置)。",
        "マイクと仮想ケーブルを選択してください (設定)。",
        "마이크와 가상 케이블을 선택하세요 (설정).");
    public static string StartFailed => T("Не удалось запустить звук: ", "Could not start audio: ", "Audiowiedergabe konnte nicht gestartet werden: ", "No se pudo iniciar el audio: ", "Impossible de démarrer l'audio : ", "无法启动音频: ", "オーディオを開始できませんでした: ", "오디오를 시작할 수 없습니다: ");

    // Настройки
    public static string SettingsTitle => T("Настройки — OSP", "Settings — OSP", "Einstellungen — OSP", "Configuración — OSP", "Paramètres — OSP", "设置 — OSP", "設定 — OSP", "설정 — OSP");
    public static string SetDevices => T("Аудиоустройства", "Audio Devices", "Audiogeräte", "Dispositivos de audio", "Périphériques audio", "音频设备", "オーディオデバイス", "오디오 장치");
    public static string SetMic => T("Микрофон (вход):", "Microphone (input):", "Mikrofon (Eingang):", "Micrófono (entrada):", "Microphone (entrée) :", "麦克风 (输入):", "マイク (入力):", "마이크 (입력):");
    public static string SetCable => T("Виртуальный кабель (выход в Discord / игры):", "Virtual cable (output to Discord / games):", "Virtuelles Kabel (Ausgang für Discord / Spiele):", "Cable virtual (salida para Discord / juegos):", "Câble virtuel (sortie Discord / jeux) :", "虚拟音频线 (输出到 Discord / 游戏):", "仮想ケーブル (Discord / ゲームへの出力):", "가상 케이블 (Discord / 게임 출력):");
    public static string SetCableFallback => T(
        "Устройство вывода (VB-CABLE не обнаружен, выберите другой выход):",
        "Output device (VB-CABLE not detected, pick any output):",
        "Ausgabegerät (VB-CABLE nicht gefunden, beliebigen Ausgang wählen):",
        "Dispositivo de salida (VB-CABLE no detectado, elige otra salida):",
        "Périphérique de sortie (VB-CABLE non détecté, choisissez une sortie) :",
        "输出设备 (未检测到 VB-CABLE，请选择其他输出):",
        "出力デバイス (VB-CABLE未検出、他の出力を選択):",
        "출력 장치 (VB-CABLE 미감지, 다른 출력 선택):");
    public static string SetMonitor => T("Наушники (мониторинг себя):", "Headphones (self monitor):", "Kopfhörer (Eigenabhörung):", "Auriculares (monitorización propia):", "Casque (écoute de soi) :", "耳机 (自身监听):", "ヘッドフォン (自身のモニタリング):", "헤드폰 (자체 모니터링):");
    public static string SetVirtInSystem => T("Микрофон в системе (выбрать в Discord/CS):", "System mic (pick in Discord/games):", "Systemmikrofon (in Discord/Spielen wählen):", "Micrófono del sistema (elegir en Discord/juegos):", "Micro système (à choisir dans Discord/jeux) :", "系统输入麦克风 (在 Discord/游戏中选择):", "システムマイク (Discord/ゲームで選択):", "시스템 마이크 (Discord/게임에서 선택):");
    public static string SetVirtFallback => T("Маршрутизация звука:", "Output routing:", "Audio-Routing:", "Enrutamiento de audio:", "Routage audio :", "音频输出路由:", "オーディオル低下ーティング:", "오디오 라우팅:");
    public static string Refresh => T("Обновить список устройств", "Refresh device list", "Geräteliste aktualisieren", "Actualizar lista de dispositivos", "Actualiser la liste des périphériques", "刷新设备列表", "デバイスリストを更新", "장치 목록 새로고침");
    public static string SetParams => T("Обработка звука", "Audio Processing", "Audioverarbeitung", "Procesamiento de audio", "Traitement audio", "音频处理", "オーディオ処理", "오디오 처리");
    public static string Pregain(double v) => T($"Предусиление входа: x{v:0.0}", $"Input pregain: x{v:0.0}", $"Eingangsverstärkung: x{v:0.0}", $"Preamplificación de entrada: x{v:0.0}", $"Pré-gain d'entrée : x{v:0.0}", $"输入前级增益: x{v:0.0}", $"入力プリアンプゲイン: x{v:0.0}", $"입력 프리게인: x{v:0.0}");
    public static string Gate(double v) => T($"Шумодав (Noise Gate): {v:0.000}", $"Noise gate: {v:0.000}", $"Rauschunterdrückung: {v:0.000}", $"Puerta de ruido: {v:0.000}", $"Porte de bruit : {v:0.000}", $"降噪阈值 (Noise Gate): {v:0.000}", $"ノイズゲート: {v:0.000}", $"노이즈 게이트: {v:0.000}");
    public static string SetLang => T("Язык / Language", "Language / Язык", "Sprache / Language", "Idioma / Language", "Langue / Language", "语言 / Language", "言語 / Language", "언어 / Language");
    public static string SetConfigFile => T("Файл настроек:", "Config file:", "Konfigurationsdatei:", "Archivo de configuración:", "Fichier de configuration :", "配置文件:", "設定ファイル:", "설정 파일:");
    public static string OpenFolder => T("Открыть папку", "Open folder", "Ordner öffnen", "Abrir carpeta", "Ouvrir dossier", "打开文件夹", "フォルダーを開く", "폴더 열기");
    public static string ResetAll => T("Сбросить всё", "Reset all", "Alles zurücksetzen", "Restablecer todo", "Tout réinitialiser", "重置所有", "すべてリセット", "모두 초기화");
    public static string ResetAsk => T(
        "Сбросить все настройки (голоса, громкости, устройства)?\nФайлы падов сохранятся.",
        "Reset all settings (voices, volumes, devices)?\nPad files are kept.",
        "Alle Einstellungen zurücksetzen?\nSounddateien bleiben erhalten.",
        "¿Restablecer todos los ajustes?\nLos sonidos se mantendrán.",
        "Réinitialiser tous les paramètres ?\nLes fichiers sons seront conservés.",
        "重置所有设置（声音、音量、设备）？\n声音板音频文件将保留。",
        "すべての設定 (ボイス、音量、デバイス) をリセットしますか？\nサウンドファイルは保持されます。",
        "모든 설정(음성, 볼륨, 장치)을 초기화하시겠습니까?\n사운드 파일은 유지됩니다.");
    public static string SetAbout => T("О приложении", "About OpenSoundPad", "Über OpenSoundPad", "Acerca de OpenSoundPad", "À propos d'OpenSoundPad", "关于 OpenSoundPad", "OpenSoundPad について", "OpenSoundPad 정보");
    public static string SetAboutDesc => T(
        "Голосовой модулятор реального времени и саундпад с выводом в виртуальный микрофон Windows.",
        "Real-time voice changer and soundpad with direct routing to Windows virtual microphone.",
        "Echtzeit-Stimmenverzerrer und Soundpad mit Routing zum virtuellen Windows-Mikrofon.",
        "Modulador de voz en tiempo real y soundpad con enrutamiento al micrófono virtual de Windows.",
        "Modulateur vocal en temps réel et soundpad avec routage vers micro virtuel Windows.",
        "实时变声器与声音板，可直接路由至 Windows 虚拟麦克风。",
        "Windows仮想マイクにルーティング可能なリアルタイムボイスチェンジャー＆サウンドパッド。",
        "Windows 가상 마이크로 직접 라우팅되는 실시간 보이스 체인저 및 사운드패드.");
    public static string Ok => T("OK", "OK", "OK", "Aceptar", "Valider", "确定", "OK", "확인");
    public static string Cancel => T("Отмена", "Cancel", "Abbrechen", "Cancelar", "Annuler", "取消", "キャンセル", "취소");

    // Экран выбора языка при первом запуске
    public static string WelcomeTitle => T("Добро пожаловать в OpenSoundPad", "Welcome to OpenSoundPad", "Willkommen bei OpenSoundPad", "Bienvenido a OpenSoundPad", "Bienvenue sur OpenSoundPad", "欢迎使用 OpenSoundPad", "OpenSoundPad へようこそ", "OpenSoundPad에 오신 것을 환영합니다");
    public static string SelectLanguagePrompt => T("Выберите язык интерфейса:", "Choose your language:", "Wählen Sie Ihre Sprache:", "Elige tu idioma:", "Choisissez votre langue :", "选择您的界面语言:", "インターフェースの言語を選択してください:", "인터페이스 언어를 선택하세요:");
    public static string ContinueBtn => T("Продолжить", "Continue", "Weiter", "Continuar", "Continuer", "继续", "次へ", "계속");
}
