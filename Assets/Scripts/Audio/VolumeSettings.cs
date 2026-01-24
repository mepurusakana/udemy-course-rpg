using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("核心元件")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("UI 滑桿 (請設定 Min=0, Max=4, WholeNumbers=True)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";

    private void Start()
    {
        // 1. 讀取存檔 (預設值為 4，也就是 100%)
        // 載入後直接設定給 Slider，讓 Slider 自動跳到對應位置
        masterSlider.value = PlayerPrefs.GetFloat(MIXER_MASTER, 4f);
        musicSlider.value = PlayerPrefs.GetFloat(MIXER_MUSIC, 4f);
        sfxSlider.value = PlayerPrefs.GetFloat(MIXER_SFX, 4f);

        // 2. 綁定事件
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 3. 初始化音量 (強制執行一次轉換邏輯)
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    // === 通用轉換公式 ===
    // 將 0~4 的整數，轉換成 AudioMixer 需要的 dB 值
    private float ConvertToDecibel(float sliderValue)
    {
        // 1. 先把 0~4 轉成 0~1 的比例 (0, 0.25, 0.5, 0.75, 1)
        float fraction = sliderValue / 4f;

        // 2. 如果是 0，直接給 -80dB (完全靜音)，避免 Log(0) 錯誤
        if (fraction <= 0)
        {
            return -80f;
        }

        // 3. 其他數值用 Log10 轉換成真實聽感音量
        return Mathf.Log10(fraction) * 20;
    }

    // === 各個 Slider 的控制 ===

    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat(MIXER_MASTER, ConvertToDecibel(value));
        PlayerPrefs.SetFloat(MIXER_MASTER, value); // 存檔存的是 0-4 的整數
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat(MIXER_MUSIC, ConvertToDecibel(value));
        PlayerPrefs.SetFloat(MIXER_MUSIC, value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat(MIXER_SFX, ConvertToDecibel(value));
        PlayerPrefs.SetFloat(MIXER_SFX, value);
    }
}