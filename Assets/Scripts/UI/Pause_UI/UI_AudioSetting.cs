using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UI_AudioSetting : MonoBehaviour
{
    [Header("Master（總音量）")]
    public Slider masterSlider;              // 0~4（五段）
    public Image masterDisplayImage;
    public Sprite[] masterLevelSprites = new Sprite[5];
    public Sprite[] masterHoverSprites = new Sprite[5];

    [Header("BGM（背景音樂）")]
    public Slider bgmSlider;
    public Image bgmDisplayImage;
    public Sprite[] bgmLevelSprites = new Sprite[5];
    public Sprite[] bgmHoverSprites = new Sprite[5];

    [Header("SFX（音效）")]
    public Slider sfxSlider;
    public Image sfxDisplayImage;
    public Sprite[] sfxLevelSprites = new Sprite[5];
    public Sprite[] sfxHoverSprites = new Sprite[5];

    [Header("顯示百分比（可選）")]
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("Hover 觸發目標")]
    public bool hoverOnSlider = true;
    public bool hoverOnDisplayImage = true;

    private bool _hoverMaster, _hoverBGM, _hoverSFX;

    private void Awake()
    {
        // 確保 Slider 設定正確 (雖然 VolumeSettings 也會做，但多做無妨)
        ConfigureDiscreteSlider(masterSlider);
        ConfigureDiscreteSlider(bgmSlider);
        ConfigureDiscreteSlider(sfxSlider);

        // 設定滑鼠懸停 (Hover) 事件
        WireHover(masterSlider ? masterSlider.gameObject : null, v => { _hoverMaster = v; UpdateMasterUI(); }, hoverOnSlider);
        WireHover(masterDisplayImage ? masterDisplayImage.gameObject : null, v => { _hoverMaster = v; UpdateMasterUI(); }, hoverOnDisplayImage);

        WireHover(bgmSlider ? bgmSlider.gameObject : null, v => { _hoverBGM = v; UpdateBGMUI(); }, hoverOnSlider);
        WireHover(bgmDisplayImage ? bgmDisplayImage.gameObject : null, v => { _hoverBGM = v; UpdateBGMUI(); }, hoverOnDisplayImage);

        WireHover(sfxSlider ? sfxSlider.gameObject : null, v => { _hoverSFX = v; UpdateSFXUI(); }, hoverOnSlider);
        WireHover(sfxDisplayImage ? sfxDisplayImage.gameObject : null, v => { _hoverSFX = v; UpdateSFXUI(); }, hoverOnDisplayImage);
    }

    private void OnEnable()
    {
        // 當視窗打開時，根據 Slider 目前的位置刷新圖片
        // 不再去讀 SettingsService，而是直接信賴 Slider 上的值 (因為 VolumeSettings 已經幫你設好值了)
        UpdateAllUI();
    }

    private void Start()
    {
        // 監聽 Slider 數值變化 -> 只更新圖片，不處理音量
        if (masterSlider) masterSlider.onValueChanged.AddListener(_ => { UpdateMasterUI(); });
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(_ => { UpdateBGMUI(); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(_ => { UpdateSFXUI(); });
    }

    // ---------- UI 圖 + 文字 更新邏輯 ----------
    private void UpdateAllUI()
    {
        UpdateMasterUI();
        UpdateBGMUI();
        UpdateSFXUI();
    }

    private void UpdateMasterUI()
    {
        if (masterSlider == null) return;
        int idx = ToIndex(masterSlider);
        SetDisplay(masterDisplayImage, masterLevelSprites, masterHoverSprites, idx, _hoverMaster);
        SetPercent(masterValueText, idx);
    }

    private void UpdateBGMUI()
    {
        if (bgmSlider == null) return;
        int idx = ToIndex(bgmSlider);
        SetDisplay(bgmDisplayImage, bgmLevelSprites, bgmHoverSprites, idx, _hoverBGM);
        SetPercent(bgmValueText, idx);
    }

    private void UpdateSFXUI()
    {
        if (sfxSlider == null) return;
        int idx = ToIndex(sfxSlider);
        SetDisplay(sfxDisplayImage, sfxLevelSprites, sfxHoverSprites, idx, _hoverSFX);
        SetPercent(sfxValueText, idx);
    }

    // ---------- 小工具 ----------
    private static void ConfigureDiscreteSlider(Slider s)
    {
        if (!s) return;
        s.wholeNumbers = true;
        s.minValue = 0;
        s.maxValue = 4;
    }

    private static int ToIndex(Slider s) => !s ? 0 : Mathf.Clamp(Mathf.RoundToInt(s.value), 0, 4);

    private static void SetDisplay(Image img, Sprite[] normal, Sprite[] hover, int index, bool isHovering)
    {
        if (!img) return;
        Sprite pick = null;

        // 嘗試抓取 Hover 圖片
        if (isHovering && IsValid(hover)) pick = hover[index];
        // 如果沒有 Hover 或沒設定，抓取普通圖片
        if (pick == null && IsValid(normal)) pick = normal[index];

        if (pick) img.sprite = pick;
    }

    private static bool IsValid(Sprite[] arr) => arr != null && arr.Length == 5 && arr[0] != null;

    private static void SetPercent(TextMeshProUGUI label, int index)
    {
        if (label) label.text = $"{index * 25}%";
    }

    private static void WireHover(GameObject go, System.Action<bool> setHover, bool enabled)
    {
        if (!enabled || !go) return;
        var et = go.GetComponent<EventTrigger>();
        if (!et) et = go.AddComponent<EventTrigger>();
        AddOrBindEvent(et, EventTriggerType.PointerEnter, _ => setHover(true));
        AddOrBindEvent(et, EventTriggerType.PointerExit, _ => setHover(false));
    }

    private static void AddOrBindEvent(EventTrigger et, EventTriggerType type, System.Action<BaseEventData> action)
    {
        var entry = et.triggers.Find(e => e.eventID == type);
        if (entry == null)
        {
            entry = new EventTrigger.Entry { eventID = type, callback = new EventTrigger.TriggerEvent() };
            et.triggers.Add(entry);
        }
        entry.callback.AddListener(data => action(data));
    }
}