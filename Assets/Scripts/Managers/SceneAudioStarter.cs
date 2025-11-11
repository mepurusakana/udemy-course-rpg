using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-900)]   // 比 AudioManager 晚一點
public class SceneAudioStarter : MonoBehaviour
{
    public int bgmIndex = 0;
    public bool playOnStart = true;
    public AudioManager audioManager;

    private void Start()
    {
        if (!playOnStart) return;
        StartCoroutine(EnsureAndPlay());
    }

    IEnumerator EnsureAndPlay()
    {
        // 嘗試在幾幀內等到 AudioManager 就緒
        for (int i = 0; i < 10 && audioManager == null; i++)
        {
            audioManager = AudioManager.instance;
            if (audioManager) break;
            yield return null; // 下一幀再試
        }

        //if (audioManager)
        //    audioManager.PlayBGM(bgmIndex);
        //else
        //    Debug.LogWarning("[SceneAudioStarter] 找不到 AudioManager，略過播放。");
    }
}
