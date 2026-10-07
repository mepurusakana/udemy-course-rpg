using UnityEngine;

public class InstructionsUIInput : MonoBehaviour
{
    [Header("返回 Setting 的按鍵")]
    [SerializeField] private KeyCode backKey = KeyCode.Space;

    private void Update()
    {
        // UI 沒開啟時不處理
        //if (!gameObject.activeInHierarchy)
            //return;

        if (Input.GetKeyDown(backKey))
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.ShowSetting();
            }
        }
    }
}