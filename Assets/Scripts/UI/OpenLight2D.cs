using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // 加入以使用 Light2D

public class OpenLight2D : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool notifyOnExit = true;

    [SerializeField] private Light2D light2D;

    public bool IsPlayerInRange { get; private set; } = false;

    private void Awake()
    {
        if (light2D == null)
            light2D = GetComponentInChildren<Light2D>();

        ApplyLight2D(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerInRange = true;
        ApplyLight2D(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerInRange = false;
        ApplyLight2D(false);
    }

    private void ApplyLight2D(bool inRange)
    {

        if (light2D != null)
        {
            light2D.enabled = inRange; // 玩家靠近亮燈，離開關燈
        }
    }
}
