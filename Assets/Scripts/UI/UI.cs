using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour, ISaveable
{
    public static UI instance; // 新增：Singleton

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string[] gameplaySceneNames = { "A001", "B001" }; // 關卡場景清單

    [Header("End screen")]
    [SerializeField] private UI_FadeScreen fadeScreen;
    [SerializeField] private GameObject endText;
    [SerializeField] private GameObject restartButton;
    [Space]
    
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private GameObject inGameUI;
    public GameObject UI_Skill;

    public static event Action<GameObject> OnSkillUILoaded;

    public UI_Dialogue dialogueUI { get; private set; }
    public AudioManager audioManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded_OpenHUD;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_OpenHUD;
    }

    private void OnSceneLoaded_OpenHUD(Scene scene, LoadSceneMode mode)
    {
        // 回到主選單：保險關閉遊戲內 UI / 解除暫停
        if (scene.name == mainMenuSceneName)
        {
            if (PauseUI) PauseUI.SetActive(false);
            if (inGameUI) inGameUI.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        // 進入關卡場景：下一幀強制切到 InGame_UI（壓過先前的關閉狀態）
        if (Array.Exists(gameplaySceneNames, n => n == scene.name))
            StartCoroutine(OpenHudNextFrame());
    }

    private IEnumerator OpenHudNextFrame()
    {
        yield return null;            // 等一幀，避免被其他 Start/OnEnable 又關掉
        SwitchTo(inGameUI);           // 你的既有方法：會自動關其他 UI、開 inGameUI、並取消暫停
    }



    private void Awake()
    {
        // 新增：Singleton 設定
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 讓 Canvas 跨場景存在
        }

        fadeScreen.gameObject.SetActive(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
    }

    void Start()
    {
        SwitchTo(inGameUI);
        if (SceneManager.GetActiveScene().name != mainMenuSceneName)
            SwitchWithKeyTo(inGameUI);
    }


    // Update is called once per frame
    void Update()
    {


        if (Input.GetKeyDown(KeyCode.O))
            SwitchWithKeyTo(inGameUI);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 主選單禁止開 PauseUI
            if (SceneManager.GetActiveScene().name != mainMenuSceneName)
            {
                SwitchWithKeyTo(PauseUI);
            }
        }
    }

    public UI_FadeScreen GetFadeScreen()
    {
        return fadeScreen;
    }

    public void SwitchTo(GameObject _menu)
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            bool fadeScreen = transform.GetChild(i).GetComponent<UI_FadeScreen>() != null; // we need this to keep fade screen game object active


            if (fadeScreen == false)
                transform.GetChild(i).gameObject.SetActive(false);
        }



        if (_menu != null)
        {
            AudioManager.instance.PlaySFX(5, null);
            _menu.SetActive(true);
        }


        if (GameManager.instance != null)
        {
            if (_menu == inGameUI)
                GameManager.instance.PauseGame(false);
            else
                GameManager.instance.PauseGame(true);
        }
    }

    public void SwitchWithKeyTo(GameObject _menu)
    {
        if (_menu != null && _menu.activeSelf)
        {
            _menu.SetActive(false);
            if (audioManager != null)
            {
                // 關閉 BGM 旗標，並立即停止所有 BGM
                audioManager.playBgm = true;
            }
            CheckForInGameUI();

            return;
        }
        
        SwitchTo(_menu);
        if (audioManager != null)
        {
            // 關閉 BGM 旗標，並立即停止所有 BGM
            audioManager.playBgm = false;
            audioManager.StopAllBGM();
        }
    }

    private void CheckForInGameUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf && transform.GetChild(i).GetComponent<UI_FadeScreen>() == null)
                return;
        }

        SwitchTo(inGameUI);
    }

    public void SwitchOnEndScreen()
    {
        fadeScreen.FadeOut();
        StartCoroutine(EndScreenCorutione());
    }

    IEnumerator EndScreenCorutione()
    {
        yield return new WaitForSeconds(1);
        endText.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        restartButton.SetActive(true);

    }

    public void ShowSkillsUI()
    {
        UI_Skill.SetActive(true);
    }

    public void HideSkillsUI()
    {
        UI_Skill.SetActive(false);
    }

    public void RegisterSkillUI(GameObject skillUI)
    {
        OnSkillUILoaded?.Invoke(skillUI);
    }

    public void RestartGameButton() => GameManager.instance.RestartScene();

    public void LoadData(GameData _data)
    {
        //foreach (KeyValuePair<string, float> pair in _data.volumeSettings)
        //{
        //    foreach (UI_VolumeSlider item in volumeSettings)
        //    {
        //        if (item.parametr == pair.Key)
        //            item.LoadSlider(pair.Value);
        //    }
        //}
    }

    public void SaveData(ref GameData _data)
    {
        //_data.volumeSettings.Clear();

        //foreach (UI_VolumeSlider item in volumeSettings)
        //{
        //    _data.volumeSettings.Add(item.parametr, item.slider.value);
        //}
    }
    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        //stopPlayerControls(true);
        //HideALLTooltips();
        dialogueUI.gameObject.SetActive(true);
        //dialogueUI.PlayDialogueLine(firstLine);
    }
}
