using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlotToMainMenu : MonoBehaviour
{
    [SerializeField] private string targetSceneName;  // Continue / Load 選單要前往的場景名

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[UI_Manager] 請在 Inspector 設定 targetSceneName！", this);
        }
    }
}
