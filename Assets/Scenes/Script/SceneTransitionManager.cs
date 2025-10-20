using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // 如果你使用 Cinemachine

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;

    private string targetScene;
    private Vector3 targetSpawnPosition;
    private bool isTransitioning = false;

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
            DontDestroyOnLoad(gameObject);
        }
    }

    // 新增：場景切換方法（帶有目標位置）
    public void TransitionToScene(string sceneName, Vector3 spawnPosition)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionCoroutine(sceneName, spawnPosition));
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector3 spawnPosition)
    {
        isTransitioning = true;

        // 1. 畫面漸黑
        if (UI.instance != null)
        {
            UI_FadeScreen fadeScreen = UI.instance.GetFadeScreen();
            if (fadeScreen != null)
            {
                fadeScreen.FadeOut();
            }
        }

        yield return new WaitForSeconds(transitionDuration);

        // 2. 儲存目標位置
        targetSpawnPosition = spawnPosition;

        // 3. 載入場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. 場景載入完成後，移動玩家到目標位置
        yield return new WaitForSeconds(0.2f);

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.transform.position = targetSpawnPosition;
            Debug.Log($"Player moved to spawn position: {targetSpawnPosition}");

            // 重置玩家速度
            PlayerManager.instance.player.SetZeroVelocity();
        }

        // 5. 重新設定相機目標
        SetupCamera();

        // 6. 重新連接 UI
        if (UI.instance != null)
        {
            UI.instance.GetComponentInChildren<UI_InGame>()?.FindAndSubscribeToPlayer();
        }

        // 7. 畫面漸亮
        yield return new WaitForSeconds(0.3f);

        if (UI.instance != null)
        {
            UI_FadeScreen fadeScreen = UI.instance.GetFadeScreen();
            if (fadeScreen != null)
            {
                fadeScreen.FadeIn();
            }
        }

        isTransitioning = false;
    }

    // 新增：設定相機跟隨玩家
    private void SetupCamera()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return;

        Transform playerTransform = PlayerManager.instance.player.transform;

        // 方案 1：如果使用 Cinemachine
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = playerTransform;
            Debug.Log("Cinemachine camera set to follow player");
        }

        // 方案 2：如果使用傳統 Camera Follow 腳本
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 如果你有自定義的相機跟隨腳本，在這裡設定
            // CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            // if (cameraFollow != null)
            // {
            //     cameraFollow.target = playerTransform;
            // }
        }
    }
}