using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class InGame : MonoBehaviour
{
    public static InGame instance;

    private GameObject player;
    public Slider sliderHp;
    public Slider sliderMp;

    private float currentHp;
    private float currentMp;

    private float maxHp = 100;
    private float maxMp = 100;

    // Start is called before the first frame update
    private void Awake()
    {
        player = GameObject.Find("Player");

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentHp = player.GetComponent<PlayerStats>().currentHealth;
        currentMp = player.GetComponent<PlayerStats>().currentMP;

        sliderHp.value = currentHp / maxHp;
        sliderMp.value = currentMp / maxMp;
    }

    // Update is called once per frame
    

    private void FixedUpdate()
    {

        currentHp = player.GetComponent<PlayerStats>().currentHealth;
        currentMp = player.GetComponent<PlayerStats>().currentMP;

        float mp= currentMp / maxMp;

        sliderHp.value = currentHp / maxHp;
        sliderMp.value = mp;

        Debug.Log(mp);
        Debug.Log(currentMp);
    }
}
