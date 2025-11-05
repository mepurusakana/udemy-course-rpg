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

    // 五檔對應線性音量
    private readonly float[] volumes = { 0f, 0.25f, 0.5f, 0.75f, 1f };

    private bool _hoverMaster, _hoverBGM, _hoverSFX;

    private void Awake()
    {
        ConfigureDiscreteSlider(masterSlider);
        ConfigureDiscreteSlider(bgmSlider);
        ConfigureDiscreteSlider(sfxSlider);

        WireHover(masterSlider ? masterSlider.gameObject : null, v => { _hoverMaster = v; UpdateMasterUI(); }, hoverOnSlider);
        WireHover(masterDisplayImage ? masterDisplayImage.gameObject : null, v => { _hoverMaster = v; UpdateMasterUI(); }, hoverOnDisplayImage);

        WireHover(bgmSlider ? bgmSlider.gameObject : null, v => { _hoverBGM = v; UpdateBGMUI(); }, hoverOnSlider);
        WireHover(bgmDisplayImage ? bgmDisplayImage.gameObject : null, v => { _hoverBGM = v; UpdateBGMUI(); }, hoverOnDisplayImage);

        WireHover(sfxSlider ? sfxSlider.gameObject : null, v => { _hoverSFX = v; UpdateSFXUI(); }, hoverOnSlider);
        WireHover(sfxDisplayImage ? sfxDisplayImage.gameObject : null, v => { _hoverSFX = v; UpdateSFXUI(); }, hoverOnDisplayImage);
    }

    private void OnEnable()
    {
        // 從服務讀目前值 → 更新 UI
        var s = SettingsService.Instance.Settings;
        masterSlider.SetValueWithoutNotify(Mathf.RoundToInt(s.master * 4f));
        bgmSlider.SetValueWithoutNotify(Mathf.RoundToInt(s.bgm * 4f));
        sfxSlider.SetValueWithoutNotify(Mathf.RoundToInt(s.sfx * 4f));
        UpdateAllUI();

        // 別的場景也可能在改 → 訂閱事件來同步 UI
        SettingsService.onChanged += OnSettingsChanged;
    }
    private void OnDisable()
    {
        SettingsService.onChanged -= OnSettingsChanged;
    }

    private void Start()
    {
        // 值改變 → 呼叫服務（會立即套用 + 存檔 + 廣播）
        if (masterSlider) masterSlider.onValueChanged.AddListener(_ => { ApplyMasterFromUI(); });
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(_ => { ApplyBGMFromUI(); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(_ => { ApplySFXFromUI(); });
    }

    private void OnSettingsChanged(GameSettings gs)
    {
        // 其他場景動到時，同步我們面板的顯示
        masterSlider.SetValueWithoutNotify(Mathf.RoundToInt(gs.master * 4f));
        bgmSlider.SetValueWithoutNotify(Mathf.RoundToInt(gs.bgm * 4f));
        sfxSlider.SetValueWithoutNotify(Mathf.RoundToInt(gs.sfx * 4f));
        UpdateAllUI();
    }

    // ---------- 從 UI 套用到服務 ----------
    private void ApplyMasterFromUI()
    {
        int idx = ToIndex(masterSlider);
        SettingsService.Instance.SetMaster(volumes[idx]);
        UpdateMasterUI();
    }
    private void ApplyBGMFromUI()
    {
        int idx = ToIndex(bgmSlider);
        SettingsService.Instance.SetBGM(volumes[idx]);
        UpdateBGMUI();
    }
    private void ApplySFXFromUI()
    {
        int idx = ToIndex(sfxSlider);
        SettingsService.Instance.SetSFX(volumes[idx]);
        UpdateSFXUI();
    }

    // ---------- UI 圖 + 文字 ----------
    private void UpdateAllUI()
    {
        UpdateMasterUI();
        UpdateBGMUI();
        UpdateSFXUI();
    }

    private void UpdateMasterUI()
    {
        int idx = ToIndex(masterSlider);
        SetDisplay(masterDisplayImage, masterLevelSprites, masterHoverSprites, idx, _hoverMaster);
        SetPercent(masterValueText, idx);
    }
    private void UpdateBGMUI()
    {
        int idx = ToIndex(bgmSlider);
        SetDisplay(bgmDisplayImage, bgmLevelSprites, bgmHoverSprites, idx, _hoverBGM);
        SetPercent(bgmValueText, idx);
    }
    private void UpdateSFXUI()
    {
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
        if (isHovering && IsValid(hover)) pick = hover[index];
        if (pick == null && IsValid(normal)) pick = normal[index];
        if (pick) img.sprite = pick;
    }
    private static bool IsValid(Sprite[] arr) => arr != null && arr.Length == 5;
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
