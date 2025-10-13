using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_AudioSetting : MonoBehaviour
{
    [Header("Master")]
    public Slider masterSlider;          // Slider（僅 0~4）
    public Image masterDisplayImage;     // 用來顯示五段圖的 Image（建議綁 Slider 的 Background）
    public Sprite[] masterLevelSprites;  // 長度 5：0/25/50/75/100

    [Header("BGM")]
    public Slider bgmSlider;
    public Image bgmDisplayImage;
    public Sprite[] bgmLevelSprites;     // 長度 5

    [Header("SFX")]
    public Slider sfxSlider;
    public Image sfxDisplayImage;
    public Sprite[] sfxLevelSprites;     // 長度 5

    [Header("顯示百分比 (可選)")]
    public TextMeshProUGUI masterValueText; // 顯示 0/25/50/75/100%
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("實際要控制的音源")]
    public AudioSource bgmSource;
    public AudioSource[] sfxSources;     // 可多個 SFX 一起控制

    // 五檔對應的真實音量（0~1）
    private readonly float[] volumes = { 0f, 0.25f, 0.5f, 0.75f, 1f };

    void Awake()
    {
        ConfigureDiscreteSlider(masterSlider);
        ConfigureDiscreteSlider(bgmSlider);
        ConfigureDiscreteSlider(sfxSlider);
    }

    void Start()
    {
        // 綁定事件
        if (masterSlider) masterSlider.onValueChanged.AddListener(_ => { UpdateMasterUI(); ApplyVolumes(); });
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(_ => { UpdateBGMUI(); ApplyVolumes(); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(_ => { UpdateSFXUI(); ApplyVolumes(); });

        // 初始顯示與套用
        UpdateMasterUI();
        UpdateBGMUI();
        UpdateSFXUI();
        ApplyVolumes();
    }

    // ---- UI 更新（圖 + 百分比） ----
    private void UpdateMasterUI()
    {
        int idx = ToIndex(masterSlider);
        SetDisplay(masterDisplayImage, masterLevelSprites, idx);
        SetPercent(masterValueText, idx);
    }

    private void UpdateBGMUI()
    {
        int idx = ToIndex(bgmSlider);
        SetDisplay(bgmDisplayImage, bgmLevelSprites, idx);
        SetPercent(bgmValueText, idx);
    }

    private void UpdateSFXUI()
    {
        int idx = ToIndex(sfxSlider);
        SetDisplay(sfxDisplayImage, sfxLevelSprites, idx);
        SetPercent(sfxValueText, idx);
    }

    // ---- 實際音量套用 ----
    private void ApplyVolumes()
    {
        int mIdx = ToIndex(masterSlider);
        int bIdx = ToIndex(bgmSlider);
        int sIdx = ToIndex(sfxSlider);

        float master = volumes[mIdx];
        float bgm = volumes[bIdx] * master;
        float sfx = volumes[sIdx] * master;

        if (bgmSource) bgmSource.volume = bgm;

        if (sfxSources != null)
        {
            foreach (var src in sfxSources)
                if (src) src.volume = sfx;
        }

        Debug.Log($"[音量] Master={mIdx * 25}%  BGM={bIdx * 25}%  SFX={sIdx * 25}%  → 套用後 BGM={bgm:0.00}, SFX={sfx:0.00}");
    }

    // ---- 小工具 ----
    private static void ConfigureDiscreteSlider(Slider s)
    {
        if (!s) return;
        s.wholeNumbers = true;
        s.minValue = 0;
        s.maxValue = 4; // 五檔
    }

    private static int ToIndex(Slider s)
    {
        if (!s) return 0;
        return Mathf.Clamp(Mathf.RoundToInt(s.value), 0, 4);
    }

    private static void SetDisplay(Image img, Sprite[] sprites, int index)
    {
        if (img != null && sprites != null && sprites.Length == 5)
            img.sprite = sprites[index];
    }

    private static void SetPercent(TextMeshProUGUI label, int index)
    {
        if (label) label.text = $"{index * 25}%";
    }
}