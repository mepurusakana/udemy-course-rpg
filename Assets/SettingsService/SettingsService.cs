using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// 跨場景常駐「設定服務」：
/// - 提供 SetXXX() 給 UI 呼叫（立即套用 + 存檔 + 廣播）
/// - 場景載入時自動再套用一次，確保新場景跟上
/// - 如果有指定 AudioMixer 就走混音器（推薦，可分開 Master/BGM/SFX）
///   若沒指定，退而只用 AudioListener.volume 控「總音量」（BGM/SFX 無法分開）
/// </summary>
[DisallowMultipleComponent]
public class SettingsService : MonoBehaviour
{
    public static SettingsService Instance { get; private set; }

    [Header("（可選）AudioMixer：建議用，才能分開 Master/BGM/SFX")]
    public AudioMixer audioMixer;
    [Header("Mixer 參數名（需與 Exposed 名稱一致）")]
    public string masterParam = "Master_dB";
    public string bgmParam = "BGM_dB";
    public string sfxParam = "SFX_dB";

    public GameSettings Settings { get; private set; } = new GameSettings();

    // 任一設定變更後廣播（亮度、畫質等訂閱者會收到）
    public static event Action<GameSettings> onChanged;

    private string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromDisk();
        ApplyAll(); // 啟動先套一次
        SceneManager.sceneLoaded += (_, __) => ApplyAll(); // 換場景再套一次
    }

    // ========= 提供給 UI 呼叫的 Setters =========
    public void SetMaster(float v) { Settings.master = Clamp01(v); ApplyAudio(); SaveAndNotify(); }
    public void SetBGM(float v) { Settings.bgm = Clamp01(v); ApplyAudio(); SaveAndNotify(); }
    public void SetSFX(float v) { Settings.sfx = Clamp01(v); ApplyAudio(); SaveAndNotify(); }

    public void SetBrightness(float v) { Settings.brightness = Clamp01(v); SaveAndNotify(); }

    public void SetQualityIndex(int i) { Settings.qualityIndex = Mathf.Clamp(i, 0, 2); ApplyVideo(); SaveAndNotify(); }
    public void SetVSync(int c) { Settings.vSyncCount = Mathf.Clamp(c, 0, 2); ApplyVideo(); SaveAndNotify(); }
    public void SetResolutionIndex(int i)
    {
        var list = Screen.resolutions;
        Settings.resolutionIndex = (i >= 0 && i < list.Length) ? i : -1;
        ApplyVideo();
        SaveAndNotify();
    }
    public void SetScreenMode(FullScreenMode m)
    {
        Settings.screenMode = m;
        ApplyVideo();
        SaveAndNotify();
    }

    // ========= 實際套用 =========
    public void ApplyAll()
    {
        ApplyAudio();
        ApplyVideo();
        onChanged?.Invoke(Settings);
    }

    private void ApplyAudio()
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(masterParam, LinearToDb(Settings.master));
            audioMixer.SetFloat(bgmParam, LinearToDb(Settings.bgm));
            audioMixer.SetFloat(sfxParam, LinearToDb(Settings.sfx));
        }
        else
        {
            // 無 Mixer 時退化為「只控總音量」
            AudioListener.volume = Settings.master;
        }
    }

    private void ApplyVideo()
    {
        // 畫質 + VSync
        QualitySettings.SetQualityLevel(Mathf.Clamp(Settings.qualityIndex, 0, 2), true);
        QualitySettings.vSyncCount = Settings.vSyncCount;

        // 顯示模式 + 解析度（獨佔全螢幕才會套用解析度）
        Screen.fullScreenMode = Settings.screenMode;

        if (Settings.resolutionIndex >= 0 && Settings.resolutionIndex < Screen.resolutions.Length)
        {
            var r = Screen.resolutions[Settings.resolutionIndex];
            // 註：ExclusiveFullScreen 時解析度才會變；其他模式 Unity 可能忽略
            Screen.SetResolution(r.width, r.height, Settings.screenMode, r.refreshRateRatio);
        }
    }

    // ========= 存取 =========
    private void SaveAndNotify()
    {
        try { File.WriteAllText(FilePath, JsonUtility.ToJson(Settings, true)); }
        catch (Exception e) { Debug.LogError($"[SettingsService] 存檔失敗：{e}"); }
        onChanged?.Invoke(Settings);
    }

    private void LoadFromDisk()
    {
        try
        {
            if (File.Exists(FilePath))
                Settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(FilePath)) ?? new GameSettings();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SettingsService] 載入失敗：{e}");
            Settings = new GameSettings();
        }
    }

    // ========= 小工具 =========
    private static float Clamp01(float v) => Mathf.Clamp01(v);
    private static float LinearToDb(float v) => (v <= 0.0001f) ? -80f : 20f * Mathf.Log10(v);
}
