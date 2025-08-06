using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_AudioSetting : MonoBehaviour
{
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    public AudioSource bgmSource;
    public AudioSource[] sfxSources; // 可支援多個 SFX 同時播

    void Start()
    {
        // 初始化滑條
        masterSlider.onValueChanged.AddListener(delegate { UpdateVolumes(); });
        bgmSlider.onValueChanged.AddListener(delegate { UpdateVolumes(); });
        sfxSlider.onValueChanged.AddListener(delegate { UpdateVolumes(); });

        UpdateVolumes(); // 初始套用
    }

    void UpdateVolumes()
    {
        float master = masterSlider.value; // 0~1
        float bgm = bgmSlider.value * master;
        float sfx = sfxSlider.value * master;

        if (bgmSource != null)
            bgmSource.volume = bgm;

        foreach (AudioSource src in sfxSources)
        {
            if (src != null)
                src.volume = sfx;
        }

        masterValueText.text = $"{Mathf.RoundToInt(master * 100)}%";
        bgmValueText.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}%";
        sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";

        Debug.Log($"[音量調整] Master: {master:F2} / BGM: {bgm:F2} / SFX: {sfx:F2}");
    }
}