using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private Transform player;
    private Checkpoint lastCheckpoint;

    // 存檔點資訊
    private string lastCheckpointSceneName;
    private string lastCheckpointId;
    private Vector3 lastCheckpointPosition;

    [Header("預設設定")]
    [SerializeField] private string defaultCheckpointId = "checkpoint_01"; //  預設存檔點ID
    [SerializeField] private string defaultSceneName = "A001"; //  如果有多場景可指定預設場景

    [SerializeField] private float respawnInvincibilityDuration = 2f;
    [SerializeField] private Checkpoint[] checkpoints;

    private bool pasuedGame;
    public bool isRespawning = false; // 新增:防止重複重生

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 重要:保持 GameManager 跨場景存在
        }
    }

    private void Start()
    {
        RefreshCheckpoints();

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            player = PlayerManager.instance.player.transform;
        }

        TrySetDefaultCheckpoint();
    }
    private void OnEnable()
    {
        // 監聽場景載入事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新尋找場景中的存檔點
        RefreshCheckpoints();

        // 重新尋找玩家
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            player = PlayerManager.instance.player.transform;
        }
    }

    private void RefreshCheckpoints()
    {
        checkpoints = FindObjectsOfType<Checkpoint>();
        Debug.Log($"Found {checkpoints.Length} checkpoints in current scene");
    }

    private void TrySetDefaultCheckpoint()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        // 如果已經有存檔點，不要覆蓋
        if (!string.IsNullOrEmpty(lastCheckpointId))
            return;

        foreach (var cp in checkpoints)
        {
            if (cp.id == defaultCheckpointId)
            {
                SetLastCheckpoint(cp);
                Debug.Log($"[GameManager] Default checkpoint set: Scene='{cp.sceneName}', ID='{cp.id}', Pos={cp.transform.position}");
                return;
            }
        }

        Debug.LogWarning($"[GameManager] Default checkpoint '{defaultCheckpointId}' not found in scene '{SceneManager.GetActiveScene().name}'");
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            RestartScene();

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!pasuedGame)
            {
                pasuedGame = true;
                GameManager.instance.PauseGame(pasuedGame);
            }
            else
            {
                pasuedGame = false;
                GameManager.instance.PauseGame(pasuedGame);
            }

        }
    }
    public void RestartScene()
    {   
        //SaveManager.instance.SaveGame();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void SetLastCheckpoint(Checkpoint checkpoint)
    {
        lastCheckpointSceneName = checkpoint.sceneName;
        lastCheckpointId = checkpoint.id;
        lastCheckpointPosition = checkpoint.transform.position;

        Debug.Log($"Last checkpoint set: Scene='{lastCheckpointSceneName}', ID='{lastCheckpointId}', Pos={lastCheckpointPosition}");
    }

    public void CheckIfLastCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint.id == lastCheckpointId && checkpoint.sceneName == lastCheckpointSceneName)
        {
            checkpoint.activationStatus = true;
            Debug.Log($"Restored checkpoint state: {checkpoint.id}");
        }
    }
    public void RespawnPlayer()
    {
        if (isRespawning)
        {
            Debug.LogWarning("[GameManager] RespawnPlayer() 被重複呼叫，忽略。");
            return;
        }

        isRespawning = true; // 提前鎖定
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        isRespawning = true;

        Player player = PlayerManager.instance.player;
        player.isBusy = true;
        // 1. 畫面漸黑
        if (UI.instance != null)
        {
            UI_FadeScreen fadeScreen = UI.instance.GetFadeScreen();
            if (fadeScreen != null)
            {
                fadeScreen.FadeOut();
            }
        }

        yield return new WaitForSeconds(2f);

        // 檢查是否需要切換場景
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(lastCheckpointSceneName) && currentSceneName != lastCheckpointSceneName)
        {
            Debug.Log($"Need to load scene: {lastCheckpointSceneName}");

            
            // 2. 載入存檔點所在場景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(lastCheckpointSceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 3. 等待場景初始化
            yield return new WaitForSeconds(0.5f);
        }

        // 4. 確保 player 參考正確
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            this.player = PlayerManager.instance.player.transform;

            // 5. 傳送玩家到存檔點
            if (lastCheckpointPosition != Vector3.zero)
            {
                this.player.position = lastCheckpointPosition;
                Debug.Log($"Player respawned at {lastCheckpointPosition}");
            }
            player.isBusy = true; //玩家在無敵期間無法移動

            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            // 6. 重置玩家狀態
            if (playerStats != null)
            {
                playerStats.ResetHealthOnRespawn();
            }

            player.stateMachine.ChangeState(player.idleState);
            player.SetZeroVelocity();

            // 7. 重新設定相機
            SetupCameraAfterRespawn();

            // 給予無敵狀態
            playerStats.MakeInvincible(true);

            // 啟動無敵特效
            PlayerFX playerFX = player.fx as PlayerFX;
            if (playerFX != null)
            {
                playerFX.StartPlayerInvincibilityEffect();
            }
            else
            {
                player.fx.StartInvincibilityEffect();
            }

            yield return new WaitForSeconds(1f);

            // 8. 畫面漸亮
            if (UI.instance != null)
            {
                UI_FadeScreen fadeScreen = UI.instance.GetFadeScreen();
                if (fadeScreen != null)
                {
                    fadeScreen.FadeIn();
                }
            }

            // 計算無敵剩餘時間
            float warningTime = 0.5f;
            float remainingTime = 1f - warningTime;

            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);

                // 播放警告特效（無敵即將結束）
                if (playerFX != null)
                {
                    playerFX.PlayInvincibilityEndWarning();
                }

                yield return new WaitForSeconds(warningTime);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }

            // === 階段 7：結束無敵 === //

            playerStats.MakeInvincible(false);

            if (playerFX != null)
            {
                playerFX.StopPlayerInvincibilityEffect();
            }
            else
            {
                player.fx.StopInvincibilityEffect();
            }

            // 恢復玩家控制
            player.isBusy = false;
        }

        isRespawning = false;
    }

    private void SetupCameraAfterRespawn()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return;

        Transform playerTransform = PlayerManager.instance.player.transform;

        // Cinemachine
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = playerTransform;
            Debug.Log("Camera reattached to player after respawn");
        }
    }


    private IEnumerator InvincibilityFlashEffect(Player player, float duration)
    {
        float elapsed = 0f;
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        if (sr == null) yield break;

        while (elapsed < duration)
        {
            sr.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        sr.color = new Color(1, 1, 1, 1f);
    }

    // 新增:取得重生位置
    public Vector3 GetRespawnPosition()
    {
        if (lastCheckpointPosition != Vector3.zero)
        {
            return lastCheckpointPosition;
        }

        // 如果沒有存檔點,返回預設位置
        return new Vector3(0, 0, 0);
    }


    // 新增:取得重生無敵時間
    public float GetRespawnInvincibilityDuration()
    {
        return respawnInvincibilityDuration;
    }

    public void LoadData(GameData _data) => StartCoroutine(LoadWithDelay(_data));

    private void LoadCheckpoints(GameData _data)
    {
        //foreach (KeyValuePair<string, bool> pair in _data.checkpoints)
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                //if (checkpoint.id == pair.Key && pair.Value == true)
                checkpoint.ActivateCheckpoint();
            }
        }
    }


    private IEnumerator LoadWithDelay(GameData _data)
    {
        yield return new WaitForSeconds(.1f);

        LoadCheckpoints(_data);
        LoadClosestCheckpoint(_data);
    }

    public void SaveData(ref GameData _data)
    {

        foreach (Checkpoint checkpoint in checkpoints)
        {

        }
    }
    private void LoadClosestCheckpoint(GameData _data)
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (lastCheckpointId == checkpoint.id)
                player.position = checkpoint.transform.position;
        }
    }
    private Checkpoint FindClosestCheckpoint()
    {
        float closestDistance = Mathf.Infinity;
        Checkpoint closestCheckpoint = null;

        foreach (var checkpoint in checkpoints)
        {
            float distanceToCheckpoint = Vector2.Distance(player.position, checkpoint.transform.position);

            if (distanceToCheckpoint < closestDistance && checkpoint.activationStatus == true)
            {
                closestDistance = distanceToCheckpoint;
                closestCheckpoint = checkpoint;
            }
        }

        return closestCheckpoint;
    }


    public void PauseGame(bool _pause)
    {
        if (_pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
}
