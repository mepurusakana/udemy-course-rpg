using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FallingLeavesController : MonoBehaviour
{
    [Header("必要引用")]
    public ParticleSystem ps;
    [Tooltip("若留空，會自動綁定有 'MainCamera' 標籤的相機或 Camera.main")]
    //public Camera cam;

    [Header("整體強度")]
    public float baseRate = 20f;          // 基礎發射率
    public float intensity = 1f;          // 場景級強度倍率

    [Header("陣風參數")]
    public Vector2 gustInterval = new Vector2(6f, 14f);   // 兩次陣風間隔
    public Vector2 gustDuration = new Vector2(2f, 4f);    // 陣風持續時間
    public Vector2 gustWindX = new Vector2(-1.2f, -0.3f); // 風向X（負值=往左）
    public Vector2 gustExtraRate = new Vector2(10f, 25f); // 陣風額外發射率

    [Header("形狀與邊界")]
    public float topMarginWorld = 1.0f;   // 超出畫面上緣的距離
    public float shapeHeight = 0.2f;      // Shape 的 Y（薄帶）

    [Header("相機綁定行為")]
    public bool autoBindMainCamera = true;      // 自動尋找主相機
    public bool rebindOnSceneChange = true;     // 場景切換時重新綁定
    public float bindRetryInterval = 0.25f;     // 相機暫時找不到時的重試間隔

    // 模組快取（注意：是 struct wrapper，來自 ps 當下狀態）
    ParticleSystem.EmissionModule em;
    ParticleSystem.ShapeModule shape;
    ParticleSystem.ForceOverLifetimeModule fol;

    Coroutine gustCo;
    Coroutine bindCo;

    void Reset()
    {
        ps = GetComponent<ParticleSystem>();
        //cam = Camera.main;
    }

    void Awake()
    {
        if (!ps) ps = GetComponent<ParticleSystem>();
        // 不急著拿 Camera.main，交給綁定流程
        CacheModules();
    }

    void OnEnable()
    {
        if (rebindOnSceneChange)
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

        StartBindRoutine();     // 啟動自動綁相機
        FitToCamera();          // 先嘗試一次
        StartGustRoutine();     // 陣風
    }

    void OnDisable()
    {
        if (rebindOnSceneChange)
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (gustCo != null) StopCoroutine(gustCo);
        if (bindCo != null) StopCoroutine(bindCo);
    }

    void CacheModules()
    {
        if (!ps) return;
        em = ps.emission;
        shape = ps.shape;
        fol = ps.forceOverLifetime;
    }

    void StartBindRoutine()
    {
        if (bindCo != null) StopCoroutine(bindCo);
        bindCo = StartCoroutine(BindCameraLoop());
    }

    IEnumerator BindCameraLoop()
    {
        // 若已指定 cam 就直接確認可用性
        //if (cam != null && cam.gameObject.activeInHierarchy) yield break;

        while (autoBindMainCamera)
        {
            // 先用 Tag 尋找（與 DDOL 主相機最相容）
            var tagged = GameObject.FindGameObjectWithTag("MainCamera");
            if (tagged)
            {
                //cam = tagged.GetComponent<Camera>();
                //if (cam) yield break; // 成功綁定
            }

            // 退回 Camera.main
            //if (!cam) cam = Camera.main;

            //if (cam && cam.gameObject.activeInHierarchy) yield break;

            // 仍找不到就稍後重試（跨場景初期常見）
            yield return new WaitForSeconds(bindRetryInterval);
        }
    }

    void OnActiveSceneChanged(Scene _, Scene __)
    {
        // 場景更換後，重新綁定一次（DDOL 主相機通常仍在，但這樣更保險）
        StartBindRoutine();
        // 也重新對齊一次
        FitToCamera();
    }

    void LateUpdate()
    {
        // 若相機在這一幀掉了（例如臨時切換/卸載），嘗試重綁
        //if (!cam && autoBindMainCamera && (bindCo == null))
        //    StartBindRoutine();

        //FitToCamera();

        // 基礎發射率（常數曲線）
        var rate = baseRate * intensity;
        if (em.enabled) em.rateOverTime = rate;
    }

    void FitToCamera()
    {
        //if (!cam) return;

        //if (cam.orthographic)
        //{
        //    float camHalfH = cam.orthographicSize;
        //    float camHalfW = camHalfH * cam.aspect;
        //    float width = camHalfW * 2f;

        //    // 調整 Shape
        //    shape.shapeType = ParticleSystemShapeType.Box;
        //    shape.scale = new Vector3(width, shapeHeight, 1f);

        //    // 把發射器放到畫面上緣外一點
        //    Vector3 topWorld = cam.transform.position + new Vector3(0, camHalfH + topMarginWorld, 0);
        //    transform.position = new Vector3(cam.transform.position.x, topWorld.y, transform.position.z);
        //}
        //else
        //{
        //    // 後備（透視相機）：用與相機距離估算寬度
        //    float dist = Mathf.Abs(transform.position.z - cam.transform.position.z);
        //    float halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
        //    float halfW = halfH * cam.aspect;
        //    float width = halfW * 2f;

        //    shape.shapeType = ParticleSystemShapeType.Box;
        //    shape.scale = new Vector3(width, shapeHeight, 1f);

        //    // 放到視錐上緣近似位置
        //    Vector3 topWorld = cam.transform.position + cam.transform.up * (halfH + topMarginWorld);
        //    transform.position = new Vector3(cam.transform.position.x, topWorld.y, transform.position.z);
        //}
    }

    void StartGustRoutine()
    {
        if (gustCo != null) StopCoroutine(gustCo);
        gustCo = StartCoroutine(GustRoutine());
    }

    IEnumerator GustRoutine()
    {
        while (true)
        {
            float wait = Random.Range(gustInterval.x, gustInterval.y);
            yield return new WaitForSeconds(wait);

            float dur = Random.Range(gustDuration.x, gustDuration.y);
            float targetWind = Random.Range(gustWindX.x, gustWindX.y);
            float extraRate = Random.Range(gustExtraRate.x, gustExtraRate.y);

            float startWind = fol.x.constant;
            float startRate = em.rateOverTime.constant;

            // 緩入
            float t = 0f;
            float inDur = dur * 0.4f;
            while (t < inDur)
            {
                t += Time.deltaTime;
                float k = t / inDur;
                SetWind(Mathf.Lerp(startWind, targetWind, k));
                em.rateOverTime = Mathf.Lerp(startRate, startRate + extraRate * intensity, k);
                yield return null;
            }

            // 平台期
            yield return new WaitForSeconds(dur * 0.2f);

            // 緩出
            t = 0f;
            float outDur = dur * 0.4f;
            while (t < outDur)
            {
                t += Time.deltaTime;
                float k = t / outDur;
                SetWind(Mathf.Lerp(targetWind, 0f, k));
                em.rateOverTime = Mathf.Lerp(startRate + extraRate * intensity, baseRate * intensity, k);
                yield return null;
            }

            // 歸零
            SetWind(0f);
            em.rateOverTime = baseRate * intensity;
        }
    }

    void SetWind(float x)
    {
        fol.enabled = true;
        fol.x = new ParticleSystem.MinMaxCurve(x);
    }
}
