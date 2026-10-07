using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour
{
    [Header("Ãö³¬«öÁä")]
    public KeyCode closeKey = KeyCode.Space;

    private void Update()
    {
        if (Input.GetKeyDown(closeKey))
        {
            CloseTutorial();
        }
    }

    private void CloseTutorial()
    {
        gameObject.SetActive(false);

        if (UI.instance != null)
        {
            UI.instance.SwitchTo(UI.instance.inGameUI);
        }
    }
}