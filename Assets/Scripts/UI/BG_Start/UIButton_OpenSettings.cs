using UnityEngine;

public class UIButton_OpenSettings : MonoBehaviour
{
    [Header("可選：若場景內沒有單例可在此手動指定")]
    public UI_Manager manager;

    // 取得 UI_Manager：優先用 Inspector 指定，其次用 Singleton，再退而求其次搜尋場景
    private UI_Manager M =>
        manager ? manager :
        (UI_Manager.Instance != null ? UI_Manager.Instance : FindObjectOfType<UI_Manager>(true));

    // === 給 Button.OnClick 用的公開方法（無參數） ===
    public void OpenSettings()
    {
        var m = M;
        if (m != null) m.ShowSettingUI();
        else Debug.LogWarning("[UIButton_OpenSettings] 找不到 UI_Manager。請確認場景裡有 UI_Manager，且它沒有被關閉或刪除。");
    }

    public void OpenAudioSettings()
    {
        var m = M;
        if (m != null) m.ShowAudioSettingUI();
    }

    public void OpenVideoSettings()
    {
        var m = M;
        if (m != null) m.ShowVideoSettingUI();
    }

    public void OpenPauseMenu()
    {
        var m = M;
        if (m != null) m.ShowPauseMenu();
    }
}
