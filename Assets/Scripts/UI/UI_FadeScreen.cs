using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_FadeScreen : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void FadeOut(float duration)
    {
        // 根據你的動畫片段原始長度算出速度倍率
        // 例如動畫片段是 1 秒，duration=2f → speed=0.5f
        float clipLength = GetClipLength("fadeOut");
        if (clipLength > 0)
            anim.speed = clipLength / duration;

        anim.SetTrigger("fadeOut");
    }

    public void FadeIn(float duration)
    {
        float clipLength = GetClipLength("fadeIn");
        if (clipLength > 0)
            anim.speed = clipLength / duration;

        anim.SetTrigger("fadeIn");
    }

    private float GetClipLength(string clipName)
    {
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f; // 找不到就預設1秒
    }

    
}

