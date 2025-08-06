using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    [Header("主 UI")]
    public GameObject pauseMenuUI;
    public GameObject settingUI;

    [Header("子設定頁面")]
    public GameObject audioSettingUI;
    public GameObject videoSettingUI;
    public GameObject instructionsUI;

    [Header("其他 UI")]
    public GameObject inGameUI; // 可選擇要不要自動顯示 InGame_UI

    void Start()
    {
        //ShowPauseMenu(); // 預設進入顯示主選單（視你需求可刪）
    }

    public void ShowPauseMenu()
    {
        HideAll();
        pauseMenuUI.SetActive(true);
    }

    public void ShowSetting()
    {
        HideAll();
        settingUI.SetActive(true);
    }

    public void ShowAudioSetting()
    {
        HideAll();
        audioSettingUI.SetActive(true);
    }

    public void ShowVideoSetting()
    {
        HideAll();
        videoSettingUI.SetActive(true);
    }

    public void ShowInstructions()
    {
        HideAll();
        instructionsUI.SetActive(true);
    }

    public void ContinueGame()
    {
        HideAll();
        Time.timeScale = 1f; // 解除暫停
        if (inGameUI != null)
            inGameUI.SetActive(true);
        Debug.Log("繼續遊戲");
    }

    private void HideAll()
    {
        pauseMenuUI.SetActive(false);
        settingUI.SetActive(false);
        audioSettingUI.SetActive(false);
        videoSettingUI.SetActive(false);
        instructionsUI.SetActive(false);
        if (inGameUI != null)
            inGameUI.SetActive(false);
    }
}
