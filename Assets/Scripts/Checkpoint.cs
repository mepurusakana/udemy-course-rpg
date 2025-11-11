using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // 加入以使用 Light2D
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    //private Animator anim;
    private Light2D light2D; // 新增 Light2D 欄位

    public string id;
    public bool activationStatus;

    [SerializeField] private SpriteRenderer checkpointSprite;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    [HideInInspector] public string sceneName;

    private void Awake()
    {
        // 自動記錄所在場景名稱
        sceneName = SceneManager.GetActiveScene().name;
    }

    private void Start()
    {
        // 初始化為未啟動狀態
        if (checkpointSprite != null)
        {
            checkpointSprite.color = activationStatus ? activeColor : inactiveColor;
        }

        // 檢查這個是否是最後啟動的存檔點
        if (GameManager.instance != null)
        {
            GameManager.instance.CheckIfLastCheckpoint(this);
        }
    }

    [ContextMenu("Generate checkpoint id")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        if (!activationStatus)
        {
            //AudioManager.instance.PlaySFX(4, transform);
        }

        activationStatus = true;
        if (checkpointSprite != null)
        {
            checkpointSprite.color = activeColor;
        }

        // 播放動畫
        //anim.SetBool("active", true);

        // 開啟光源
        if (light2D != null)
        {
            light2D.enabled = true;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SetLastCheckpoint(this);
            Debug.Log($"Checkpoint '{id}' activated in scene '{sceneName}'");
        }
    }
}
