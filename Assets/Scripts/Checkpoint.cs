using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // 加入以使用 Light2D

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private Light2D light2D; // 新增 Light2D 欄位

    public string id;
    public bool activationStatus;

    private void Start()
    {
        anim = GetComponent<Animator>();
        light2D = GetComponent<Light2D>(); // 若光源在子物件；若在同物件，請用 GetComponent<Light2D>()
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
            AudioManager.instance.PlaySFX(4, transform);
        }

        activationStatus = true;

        // 播放動畫
        anim.SetBool("active", true);

        // 開啟光源
        if (light2D != null)
        {
            light2D.enabled = true;
        }
    }
}
