using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour 
{
    public static PlayerManager instance;
    public Player player;

    private bool playerInitialized = false;

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
            DontDestroyOnLoad(gameObject); // 新增:保持 PlayerManager 跨場景存在
        }
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 新增：場景載入時的回調
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 確保玩家正確設定
        InitializePlayer();

        // 移除場景中重複的 Player
        RemoveDuplicatePlayers();
    }

    private void InitializePlayer()
    {
        if (player == null)
        {
            // 尋找場景中的 Player
            player = FindObjectOfType<Player>();

            if (player != null && !playerInitialized)
            {
                DontDestroyOnLoad(player.gameObject);
                playerInitialized = true;
                Debug.Log("Player initialized and set to DontDestroyOnLoad");
            }
        }
        else
        {
            // 確保 player 已經是 DontDestroyOnLoad
            if (!playerInitialized)
            {
                DontDestroyOnLoad(player.gameObject);
                playerInitialized = true;
            }
        }
    }

    private void RemoveDuplicatePlayers()
    {
        Player[] allPlayers = FindObjectsOfType<Player>();

        if (allPlayers.Length > 1)
        {
            Debug.LogWarning($"Found {allPlayers.Length} players in scene. Removing duplicates...");

            foreach (Player p in allPlayers)
            {
                // 保留我們已經設定的 player，刪除其他的
                if (p != player)
                {
                    Debug.Log($"Destroying duplicate player: {p.name}");
                    Destroy(p.gameObject);
                }
            }
        }
    }


    public void LoadData(GameData _data)
    {
    }

    public void SaveData(ref GameData _data)
    {
    }
}
