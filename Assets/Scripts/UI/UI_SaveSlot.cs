using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_SaveSlot : MonoBehaviour
{
    public int slotIndex;
    public Text slotText;
    public void Refresh()
    {
        if (SaveManager.instance.HasSaveInSlot(slotIndex))
            slotText.text = $"存檔槽{slotIndex}(已存檔)";
        else
            slotText.text = $"存檔槽{slotIndex}(空)";
    }
    public void OnClick()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogError("SaveManager 尚未初始化！請確保場景中有 SaveManager 物件。");
            return;
        }

        SaveManager.instance.InitSlot(slotIndex);

        if (SaveManager.instance.HasSaveInSlot(slotIndex))
        {
            SaveManager.instance.LoadGame();
        }
        else
        {
            SaveManager.instance.gameData = new GameData();
            SaveManager.instance.SaveGame();
        }

        SceneManager.LoadScene("A001");
    }

}
