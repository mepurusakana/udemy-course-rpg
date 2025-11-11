using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]   // 確保先於其他腳本初始化
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    // ===== 場景範圍單例（Scene-scoped）=====
    private static AudioManager _instance;
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

        // 只在「同場景」已有另一個 AudioManager 時，才視為重複並刪除自己
        if (current != null && current != this &&
            current.gameObject.scene == gameObject.scene)
        {
            Debug.LogWarning("[AudioManager] Duplicate in SAME scene. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        // 跨場景交接：永遠讓新場景這個接管靜態實例（不要自刪）
        instance = this;

        // （選用）初始化玩家變數
        if (!playerTransform && !string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) playerTransform = go.transform;
        }

        // —— 保命設定：避免距離/Listener 造成「聽不到」 —— 
        ConfigureSourcesSafety();

        Invoke(nameof(AllowSFX), 1f);
    }

    private void OnDestroy()
    {
        // 只有當自己仍是現任實例時才清空；若已被新場景接手，不動
        if (instance == this) instance = null;
    }

    // ===== 你的原有欄位 =====
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

    // 兩個保命選項（建議如下注解所述開啟）
    [Header("Safety Options")]
    [Tooltip("建議開：BGM 強制 2D，不吃距離/Listener 影響")]
    public bool forceBgm2D = true;
    [Tooltip("可開：SFX 也改為 2D（若不需要 3D 方位感）")]
    public bool forceSfx2D = false;

    private int bgmIndex = -1;
    private bool canPlaySFX;

    private void Start()
    {
        if (autoPlayOnStart && playBgm && bgm != null && bgm.Length > 0)
            PlayBGM(Mathf.Clamp(defaultBgmIndex, 0, bgm.Length - 1));
    }

    private void Update()
    {
        if (!playBgm) { StopAllBGM(); return; }

        if (bgmIndex >= 0 && bgmIndex < (bgm?.Length ?? 0))
        {
            var a = bgm[bgmIndex];
            if (a && !a.isPlaying) a.Play();    // 對應 AudioSource.loop 勾選
        }
    }

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
        a.Play();
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

    private void AllowSFX() => canPlaySFX = true;

    public static AudioManager InstanceInScene
#if UNITY_2022_1_OR_NEWER
        => FindFirstObjectByType<AudioManager>();
#else
        => FindObjectOfType<AudioManager>();
#endif

    // —— 將 BGM 強制 2D；SFX 視需求 —— 
    void ConfigureSourcesSafety()
    {
        if (bgm != null)
        {
            foreach (var a in bgm)
            {
                if (!a) continue;
                if (forceBgm2D) a.spatialBlend = 0f;  // BGM 永遠 2D
                a.playOnAwake = false;
                a.loop = true;
                a.ignoreListenerPause = true;
                a.ignoreListenerVolume = true;
                a.bypassListenerEffects = true;
            }
        }
        if (forceSfx2D && sfx != null)
        {
            foreach (var a in sfx)
            {
                if (!a) continue;
                a.spatialBlend = 0f; // SFX 也改 2D（可選）
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
        else if (autoPlayOnStart && bgm != null && bgm.Length > 0)
        {
            PlayBGM(Mathf.Clamp(defaultBgmIndex, 0, bgm.Length - 1));
        }
    }
}
