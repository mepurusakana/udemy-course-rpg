using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[Boot] Game Initializing...");

        //ClearOldPlayer();

        //  確保時間恢復（避免 pause 殘留）
        Time.timeScale = 1f;

        if(Player.instance != null )
        {
            //Destroy(Player.instance.gameObject);
            Player.instance = null;
        }

        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.player = null;
        }

        //  初始化 SaveManager（確保沒有殘資料）
        if (SaveManager.instance != null)
        {
            int newSlot = SaveManager.instance.currentSlotIndex + 1;

            SaveManager.instance.skipLoad = true;
            //SaveManager.instance.DeleteSaveData();
            SaveManager.instance.InitSlot(newSlot);
            SaveManager.instance.CreateNewGame();
            SaveManager.instance.SaveGame();
        }

        //  重置 GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetState();
        }

        //  載入主選單
        SceneManager.LoadScene("MainMenu");
    }

    private void ClearOldPlayer()
    {
        if (Player.instance != null)
        {
            Destroy(Player.instance.gameObject);
        }
    }
}