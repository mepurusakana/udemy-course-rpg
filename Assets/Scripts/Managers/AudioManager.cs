using System.Collections;
using UnityEngine;
using UnityEngine.Audio; // <--- [新增] 1. 引用 Audio 命名空間

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public bool muteFootsteps = false;

    // [新增] 2. 加入 Mixer Group 欄位，讓你能在 Inspector 把 Mixer 拉進來
    [Header("Mixer 設定 (請拖入對應群組)")]
    public AudioMixerGroup bgmMixerGroup; // <--- [新增]
    public AudioMixerGroup sfxMixerGroup; // <--- [新增]

    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_2022_1_OR_NEWER
                _instance = FindFirstObjectByType<AudioManager>();
#else
                _instance = FindObjectOfType<AudioManager>();
#endif
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private void Awake()
    {
        var current = instance;

        if (current != null && current != this &&
            current.gameObject.scene == gameObject.scene)
        {
            Debug.LogWarning("[AudioManager] Duplicate in SAME scene. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // <--- [重要] 3. 確保切換場景時，音樂管理器不會消失

        if (!playerTransform && !string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) playerTransform = go.transform;
        }

        ConfigureSourcesSafety();
        Invoke(nameof(AllowSFX), 1f);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ===== 原有欄位 =====
    [Header("SFX / BGM 設定")]
    [SerializeField] private float sfxMinimumDistance = 10f;
    [SerializeField] private AudioSource[] sfx;
    [SerializeField] private AudioSource[] bgm;

    [Header("行為控制")]
    public bool playBgm = true;
    public bool autoPlayOnStart = true;
    [SerializeField] private int defaultBgmIndex = 0;

    [Header("SFX 距離門檻（可關）")]
    public bool useDistanceGate = false;
    public Transform playerTransform;
    public string playerTag = "Player";

    [Header("Safety Options")]
    [Tooltip("建議開：BGM 強制 2D")]
    public bool forceBgm2D = true;
    [Tooltip("可開：SFX 也改為 2D")]
    public bool forceSfx2D = false;

    private int bgmIndex = -1;
    private bool canPlaySFX;

    private void Start()
    {
        if (autoPlayOnStart && playBgm && bgm != null && bgm.Length > 0)
            PlayBGM(Mathf.Clamp(defaultBgmIndex, 0, bgm.Length - 1));
    }

    // [重要修改] 4. 徹底刪除 Update
    // 原因：Update 會強制每一幀檢查播放，導致你無法暫停音樂。
    // Unity 的 loop 功能會自己處理循環，不需要這裡寫。
    /* private void Update()
    {
        if (!playBgm) { StopAllBGM(); return; }
        if (bgmIndex >= 0 && bgmIndex < (bgm?.Length ?? 0))
        {
            var a = bgm[bgmIndex];
            if (a && !a.isPlaying) a.Play();   
        }
    }
    */

    public void PlaySFX(int index, Transform source = null)
    {
        if (!canPlaySFX) return;
        if (sfx == null || index < 0 || index >= sfx.Length) return;
        var a = sfx[index];
        if (!a) return;

        if (useDistanceGate && playerTransform && source)
        {
            if (Vector2.Distance(playerTransform.position, source.position) > sfxMinimumDistance)
                return;
        }

        a.pitch = Random.Range(0.85f, 1.10f);
        a.PlayOneShot(a.clip); // [建議] 改用 PlayOneShot 避免短音效互相截斷
    }

    public void PlayLoopSFX(int index)
    {
        if (muteFootsteps) return;
        if (sfx == null || index < 0 || index >= sfx.Length) return;

        var a = sfx[index];
        if (!a) return;

        if (!a.isPlaying) a.Play();
    }

    public void StopSFX(int index)
    {
        if (sfx == null || index < 0 || index >= sfx.Length) return;
        if (sfx[index]) sfx[index].Stop();
    }

    public void StopSFXWithTime(int index)
    {
        if (sfx == null || index < 0 || index >= sfx.Length) return;
        if (sfx[index]) StartCoroutine(DecreaseVolume(sfx[index]));
    }

    private IEnumerator DecreaseVolume(AudioSource audio)
    {
        if (!audio) yield break;
        float defaultVolume = audio.volume;
        while (audio.volume > .1f)
        {
            audio.volume -= audio.volume * .2f;
            yield return new WaitForSeconds(.6f);
        }
        audio.Stop();
        audio.volume = defaultVolume;
    }

    public void PlayRandomBGM()
    {
        if (bgm == null || bgm.Length == 0) return;
        PlayBGM(Random.Range(0, bgm.Length));
    }

    public void PlayBGM(int index)
    {
        if (bgm == null || index < 0 || index >= bgm.Length) return;

        // 如果是同一首且正在播，就不重頭開始
        if (bgmIndex == index && bgm[index].isPlaying) return;

        bgmIndex = index;
        StopAllBGM();
        var a = bgm[bgmIndex];
        if (a) a.Play();
    }

    public void StopAllBGM()
    {
        if (bgm == null) return;
        for (int i = 0; i < bgm.Length; i++)
            if (bgm[i]) bgm[i].Stop();
    }

    // [新增] 5. 暫停功能 (UI Manager 需要用這個)
    public void PauseAllBGM()
    {
        if (bgm == null) return;
        foreach (var a in bgm)
        {
            if (a != null && a.isPlaying) a.Pause();
        }
    }

    // [新增] 6. 恢復播放功能
    public void ResumeAllBGM()
    {
        if (bgm == null) return;
        foreach (var a in bgm)
        {
            if (a != null) a.UnPause();
        }
    }

    private void AllowSFX() => canPlaySFX = true;

    public static AudioManager InstanceInScene
#if UNITY_2022_1_OR_NEWER
        => FindFirstObjectByType<AudioManager>();
#else
        => FindObjectOfType<AudioManager>();
#endif

    void ConfigureSourcesSafety()
    {
        if (bgm != null)
        {
            foreach (var a in bgm)
            {
                if (!a) continue;

                // [新增] 自動將 BGM 連接到 Mixer 的 Music 群組
                if (bgmMixerGroup != null) a.outputAudioMixerGroup = bgmMixerGroup;

                a.playOnAwake = false;
                a.loop = true;

                if (forceBgm2D) a.spatialBlend = 0f;

                // [重要保留] 讓 BGM 無視 Time.timeScale 暫停，這樣我們才能手動控制 Pause
                a.ignoreListenerPause = true;

                // [刪除] 這些設定會導致 Audio Mixer 調整音量無效，必須刪除！
                // a.ignoreListenerVolume = true; 
                // a.bypassListenerEffects = true;
            }
        }

        if (sfx != null)
        {
            foreach (var a in sfx)
            {
                if (!a) continue;

                // [新增] 自動將 SFX 連接到 Mixer 的 SFX 群組
                if (sfxMixerGroup != null) a.outputAudioMixerGroup = sfxMixerGroup;

                if (forceSfx2D) a.spatialBlend = 0f;
            }
        }
    }

    public void ResumeBgmIfNeeded()
    {
        if (!playBgm) return;
        if (bgmIndex >= 0 && bgmIndex < (bgm?.Length ?? 0))
        {
            var a = bgm[bgmIndex];
            if (a && !a.isPlaying) a.Play();
        }
    }
}