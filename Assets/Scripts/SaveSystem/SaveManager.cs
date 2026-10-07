using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    public GameData gameData;
    private List<ISaveable> allSaveables;

    public int currentSlotIndex = 0;
    [SerializeField] private bool encryptData = true;

    public bool skipLoad = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(.01f);

        if (skipLoad)
        {
            skipLoad = false;
            yield break;
        }

        LoadGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameData == null) return;

        allSaveables = FindISaveables();

        foreach (var saveable in allSaveables)
        {
            saveable.LoadData(gameData);
        }

        Debug.Log("[SaveManager] Scene reloaded and data applied");
    }

    //private IEnumerator Start()
    //{
    //    Debug.Log(Application.persistentDataPath);

    //    string fileName = $"saveSlot{currentSlotIndex}.json";
    //    dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
    //    allSaveables = FindISaveables();

    //    yield return new WaitForSeconds(.01f);
    //    LoadGame();
    //}

    public void InitSlot(int slotIndex)
    { 
        currentSlotIndex = slotIndex;
        string fileName = $"saveSlot{slotIndex}.json";
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        allSaveables = FindISaveables();

        // 外部使用時的保護邏輯
        var data = dataHandler.LoadData();
        if (data == null)
        {
            Debug.Log("沒有有效存檔，建立新檔案");
            data = new GameData();
            dataHandler.SaveData(data);
        }
    }
    public void LoadGame()
    {
        gameData = dataHandler.LoadData();
        if (gameData == null)
        {
            Debug.Log($"No save data found in slot {currentSlotIndex}, creating new save!");
            CreateNewGame();
            SaveGame();
            return;
        }

        foreach (var saveable in allSaveables)
            saveable.LoadData(gameData);

        //  讀取完資料後，直接設定玩家位置 & 血量
        var player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.TeleportPlayer(gameData.savedCheckpoint.ToVector3());  //  修改這裡
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.SetHealth(gameData.playerHealth);
        }

        allSaveables = FindISaveables().Where(s => s != null).ToList();

        allSaveables = FindISaveables();

        foreach (var saveable in allSaveables)
        {
            saveable.LoadData(gameData);
        }
    }
    public void SaveGame()
    {
        if (gameData == null)
        {
            Debug.LogError("[SaveManager] gameData is NULL > 自動建立");
            gameData = new GameData();
        }

        if (dataHandler == null)
        {
            Debug.LogError("[SaveManager] dataHandler is NULL > InitSlot");
            InitSlot(currentSlotIndex);
        }

        allSaveables = FindISaveables().Where(s => s != null).ToList();

        foreach (var saveable in allSaveables)
        {
            saveable.SaveData(ref gameData);
        }

        dataHandler.SaveData(gameData);
    }

    public bool HasSaveInSlot(int slotIndex)
    {
        string path = Path.Combine(Application.persistentDataPath, $"saveSlot{slotIndex}.json");
        return File.Exists(path);
    }


    private List<ISaveable> FindISaveables()
    {
        return
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToList();
    }



    public GameData GetGameData() => gameData;

    [ContextMenu("***Delete save data")]
    public void DeleteSaveData()
    {
        string fileName = $"saveSlot{currentSlotIndex}.json";
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        dataHandler.Delete();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    public void CreateNewGame()
    {
        gameData = new GameData(); // 這裡會用 GameData 的建構子，血量=100、位置=Vector3.zero
        allSaveables = FindISaveables();
        foreach (var saveable in allSaveables)
        {
            saveable.LoadData(gameData); // 確保場景中的物件用新的數據初始化
        }
    }
}
