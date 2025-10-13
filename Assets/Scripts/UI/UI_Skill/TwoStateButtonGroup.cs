using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TwoStateButtonGroup : MonoBehaviour
{
    [Header("自動掃描設定")]
    [Tooltip("啟動時自動掃描本物件下的第一層子物件，並在每個子物件中尋找名為 Light / Dark 的 Button")]
    public bool autoDiscoverOnStart = true;

    [Tooltip("啟動時預設選中的索引（-1 代表全部顯示 Dark）")]
    public int defaultSelectedIndex = -1;

    [Tooltip("是否允許點擊已點亮的 Light 來取消選取（變回 Dark）")]
    public bool allowDeselect = false;

    [Serializable]
    public class IntEvent : UnityEvent<int> { }
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    [Header("事件（選取變更時觸發）")]
    public IntEvent onSelectedIndexChanged;
    public StringEvent onSelectedNameChanged;

    // 內部結構：一個父項（如 Double）底下有兩顆按鈕
    [Serializable]
    public class Pair
    {
        public Transform root;      // 父項：例如 Double
        public Button lightButton;  // 其下名為 "Light" 的按鈕
        public Button darkButton;   // 其下名為 "Dark" 的按鈕
    }

    [Header("（可選）手動指定配對；若啟用自動掃描可留空")]
    public List<Pair> pairs = new List<Pair>();

    public int SelectedIndex { get; private set; } = -1;

    // 用於解除綁定（避免 RemoveAllListeners 影響你額外綁的事件）
    private readonly List<UnityAction> _lightHandlers = new List<UnityAction>();
    private readonly List<UnityAction> _darkHandlers = new List<UnityAction>();

    void Start()
    {
        if (autoDiscoverOnStart)
        {
            AutoDiscoverPairs();
        }

        WireUpButtons();

        // 全部先顯示 Dark
        for (int i = 0; i < pairs.Count; i++)
            SetVisual(i, isSelected: false);

        if (IsValidIndex(defaultSelectedIndex))
        {
            Select(defaultSelectedIndex, invokeEvent: false);
        }
        else
        {
            SelectedIndex = -1;
        }
    }

    void OnDestroy()
    {
        UnwireButtons();
    }

    // === 主要互動 ===
    private void OnClickLight(int index)
    {
        // 點到已選中的 Light：若允許反選 -> 關閉，否則不動
        if (SelectedIndex == index && allowDeselect)
        {
            SetVisual(index, false);
            SelectedIndex = -1;
            FireEvents();
        }
    }

    private void OnClickDark(int index)
    {
        // 任一 Dark 都是要求選取該項
        if (SelectedIndex == index) return; // 本來就選中，不必重複
        Select(index);
    }

    public void Select(int index, bool invokeEvent = true)
    {
        if (!IsValidIndex(index)) return;

        // 關閉原本選中的
        if (IsValidIndex(SelectedIndex))
            SetVisual(SelectedIndex, false);

        // 開啟新的
        SetVisual(index, true);
        SelectedIndex = index;

        if (invokeEvent)
            FireEvents();
    }

    public void ClearSelection(bool invokeEvent = true)
    {
        if (IsValidIndex(SelectedIndex))
            SetVisual(SelectedIndex, false);

        SelectedIndex = -1;
        if (invokeEvent)
            FireEvents();
    }

    // === 視覺切換：Selected -> 顯示 Light、隱藏 Dark ===
    private void SetVisual(int index, bool isSelected)
    {
        if (!IsValidIndex(index)) return;
        var p = pairs[index];
        if (p == null) return;

        if (p.lightButton) p.lightButton.gameObject.SetActive(isSelected);
        if (p.darkButton) p.darkButton.gameObject.SetActive(!isSelected);

        // 可選：避免不可見的按鈕仍可被事件射線點到
        if (p.lightButton) p.lightButton.interactable = isSelected;
        if (p.darkButton) p.darkButton.interactable = !isSelected;
    }

    private void FireEvents()
    {
        onSelectedIndexChanged?.Invoke(SelectedIndex);
        onSelectedNameChanged?.Invoke(IsValidIndex(SelectedIndex) ? pairs[SelectedIndex].root.name : string.Empty);
    }

    // === 掃描與綁定 ===
    private void AutoDiscoverPairs()
    {
        pairs.Clear();

        // 僅掃描本物件「第一層子物件」作為各配對的 root
        foreach (Transform child in transform)
        {
            var p = new Pair { root = child };
            // 先用精確名稱尋找
            var lightTr = child.Find("Light");
            var darkTr = child.Find("Dark");

            // 若找不到，再用較寬鬆的方式（大小寫不敏感、名稱包含）
            if (lightTr == null || darkTr == null)
            {
                var buttons = child.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    var lower = b.name.ToLowerInvariant();
                    if (p.lightButton == null && (lower == "light" || lower.Contains("light")))
                        p.lightButton = b;
                    else if (p.darkButton == null && (lower == "dark" || lower.Contains("dark")))
                        p.darkButton = b;
                }
            }
            else
            {
                p.lightButton = lightTr.GetComponent<Button>();
                p.darkButton = darkTr.GetComponent<Button>();
            }

            if (p.lightButton == null || p.darkButton == null)
            {
                Debug.LogWarning($"[TwoStateButtonGroup] 在「{child.name}」底下找不到 Light/Dark 兩顆 Button，請確認命名或手動指定。");
                continue;
            }

            pairs.Add(p);
        }

        if (pairs.Count == 0)
        {
            Debug.LogWarning("[TwoStateButtonGroup] 沒有找到任何配對（Pair）。請確認此腳本掛在正確的共同父容器上。");
        }
    }

    private void WireUpButtons()
    {
        UnwireButtons();
        _lightHandlers.Clear();
        _darkHandlers.Clear();

        for (int i = 0; i < pairs.Count; i++)
        {
            int captured = i;

            if (pairs[i].lightButton != null)
            {
                UnityAction hLight = () => OnClickLight(captured);
                _lightHandlers.Add(hLight);
                pairs[i].lightButton.onClick.AddListener(hLight);
            }
            else
            {
                _lightHandlers.Add(null);
            }

            if (pairs[i].darkButton != null)
            {
                UnityAction hDark = () => OnClickDark(captured);
                _darkHandlers.Add(hDark);
                pairs[i].darkButton.onClick.AddListener(hDark);
            }
            else
            {
                _darkHandlers.Add(null);
            }
        }
    }

    private void UnwireButtons()
    {
        for (int i = 0; i < pairs.Count; i++)
        {
            if (i < _lightHandlers.Count && _lightHandlers[i] != null && pairs[i].lightButton != null)
                pairs[i].lightButton.onClick.RemoveListener(_lightHandlers[i]);

            if (i < _darkHandlers.Count && _darkHandlers[i] != null && pairs[i].darkButton != null)
                pairs[i].darkButton.onClick.RemoveListener(_darkHandlers[i]);
        }
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < pairs.Count;
    }
}
