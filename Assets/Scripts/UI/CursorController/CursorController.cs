using UnityEngine;

public class CursorController : MonoBehaviour
{
    private static CursorController instance;
    private int uiOpenCount = 0;  // 記錄有多少個 UI 開啟

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideCursor();
    }

    // UI 開啟時呼叫
    public static void OnUIOpen()
    {
        if (instance != null)
        {
            instance.uiOpenCount++;
            instance.ShowCursor();
        }
    }

    // UI 關閉時呼叫
    public static void OnUIClose()
    {
        if (instance != null)
        {
            instance.uiOpenCount--;

            if (instance.uiOpenCount <= 0)
            {
                instance.uiOpenCount = 0;
                instance.HideCursor();
            }
        }
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}