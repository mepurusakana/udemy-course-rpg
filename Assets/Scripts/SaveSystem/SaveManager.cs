using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    public GameData gameData;
    private List<ISaveable> allSaveables;

    public int currentSlotIndex = 0;
    [SerializeField] private bool encryptData = true;

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
            player.TeleportPlayer(gameData.savedCheckpoint);

            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.SetHealth(gameData.playerHealth);
        }

        allSaveables = FindISaveables().Where(s => s != null).ToList();
    }

    public void SaveGame()
    {
        allSaveables = FindISaveables().Where(s => s != null).ToList();

        foreach (var saveable in allSaveables)
        {
            if (saveable != null)
                saveable.SaveData(ref gameData);
        }

        dataHandler.SaveData(gameData);
        Debug.Log("Game saved successfully.");
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

    private IEnumerator Start()
    {
        Debug.Log(Application.persistentDataPath);

        string fileName = $"saveSlot{currentSlotIndex}.json";
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        allSaveables = FindISaveables();

        yield return new WaitForSeconds(.01f);
        LoadGame();
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
