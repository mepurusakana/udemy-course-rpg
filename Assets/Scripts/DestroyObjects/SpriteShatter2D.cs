using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SpriteShatter2D : MonoBehaviour
{
    public enum TriggerMode { TriggerAndKey, Manual }

    [Header("目標渲染器(可選)")]
    public SpriteRenderer targetSpriteRenderer;

    [Header("觸發設定（玩家需在觸發器內 + 按鍵）")]
    public TriggerMode triggerMode = TriggerMode.TriggerAndKey;
    public KeyCode activationKey = KeyCode.E;
    public Transform playerTransform;
    public string playerTag = "Player";
    [Tooltip("互動範圍 Trigger Collider2D（留空會自動抓本物件第一個 Trigger）")]
    public Collider2D interactionTrigger;

    [Header("破壞前阻擋設定（擋路用）")]
    public bool autoFindBlockingColliders = true;
    public List<Collider2D> blockingColliders = new List<Collider2D>();
    public PostBreakBlockerMode blockerModeAfterBreak = PostBreakBlockerMode.Disable;
    public enum PostBreakBlockerMode { Disable, SetTrigger, Destroy }
    public string rootLayerAfterBreak = "";

    [Header("爆炸後放行控制（B 方法）")]
    [Tooltip("爆炸後過多久才把阻擋碰撞器關閉/改Trigger/刪除")]
    public float releaseBlockDelay = 0.35f;
    [Tooltip("爆炸時依阻擋碰撞器邊界生成一片隱形臨時地板撐住玩家")]
    public bool spawnTemporaryFloor = true;
    [Tooltip("臨時地板存活秒數")]
    public float tempFloorLifetime = 0.8f;
    [Tooltip("臨時地板厚度（世界單位），例如 0.1")]
    public float tempFloorThickness = 0.12f;
    [Tooltip("臨時地板使用的Layer（留空=沿用原層）")]
    public string tempFloorLayerName = "";

    [Header("一般碎裂（傳統切片）")]
    [Min(1)] public int columns = 6;
    [Min(1)] public int rows = 6;

    [Header("方塊模式（像素風碎片）")]
    [Tooltip("開啟後改用固定像素大小的小方塊當碎片")]
    public bool useSquareBlocks = true;
    [Tooltip("每個方塊的邊長（以原圖的像素為單位），例如 8 或 16")]
    [Min(1)] public int blockPixelSize = 8;
    [Tooltip("如果你不指定，會自動產生 1x1 白色方塊並用顏色染色")]
    public Sprite overrideBlockSprite;
    [Tooltip("是否使用原圖顏色（平均色）染色方塊；關掉就全用白色或 overrideBlockSprite 預設色")]
    public bool colorizeFromTexture = true;

    [Header("物理/爆散效果(基本)")]
    public float explodeForce = 2f;
    public float randomJitter = 0.5f;
    public float randomTorque = 20f;

    [Header("物理/爆散效果(控制飛太高/太遠)")]
    [Range(0f, 2f)] public float forceMultiplier = 0.4f;
    [Range(0f, 1.5f)] public float verticalForceScale = 0.3f;
    public Vector2 extraDirectionBias = new Vector2(0f, -0.3f);
    public float maxInitialSpeed = 1.5f;

    [Header("碎片剛體屬性")]
    public float fragmentsGravityScale = 1.0f;
    public float fragmentsLinearDrag = 0.3f;
    public float fragmentsAngularDrag = 0.15f;

    [Header("壽命/排序")]
    public float fragmentLifetime = 2.5f;
    public bool hideOriginalOnShatter = true;
    public string fragmentSortingLayerName = "";
    public int fragmentOrderInLayer = -9999;

    [Header("碰撞/玩家安全（碎片不影響玩家）")]
    public bool addBoxCollider2D = true;
    public float fragmentDensity = 1f;
    public bool ignorePlayerByCode = true;
    public bool fragmentsUseTrigger = false;
    public string fragmentsLayerName = "Debris";

    private SpriteRenderer _sr;
    private bool _hasShattered;
    private bool _playerInside;
    private Sprite _runtimeWhiteSquare; // 若沒給 overrideBlockSprite，動態產生 1x1 白方塊

    void Awake()
    {
        _sr = targetSpriteRenderer != null ? targetSpriteRenderer : GetComponent<SpriteRenderer>();
        if (_sr == null) Debug.LogError("[SpriteShatter2D] 找不到 SpriteRenderer！", this);

        if (autoFindBlockingColliders)
        {
            var cols = GetComponents<Collider2D>();
            foreach (var c in cols) if (c && c.enabled && !c.isTrigger) blockingColliders.Add(c);
        }

        if (interactionTrigger == null)
        {
            var cols = GetComponents<Collider2D>();
            foreach (var c in cols) if (c && c.isTrigger) { interactionTrigger = c; break; }
        }

        // 建立 1x1 白色方塊（當沒提供 overrideBlockSprite 時使用）
        if (overrideBlockSprite == null)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _runtimeWhiteSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
        }
    }

    void Update()
    {
        if (_hasShattered) return;
        if (triggerMode == TriggerMode.TriggerAndKey)
        {
            if (_playerInside && Input.GetKeyDown(activationKey)) Shatter();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (interactionTrigger == null) return;
        if (other.CompareTag(playerTag)) _playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (interactionTrigger == null) return;
        if (other.CompareTag(playerTag)) _playerInside = false;
    }

    public void Shatter()
    {
        if (_sr == null || _sr.sprite == null) return;
        if (_hasShattered) return;

        // === B 方法：先生成臨時地板撐住玩家，再延遲放行原碰撞器 ===
        if (spawnTemporaryFloor) SpawnTemporaryFloorFromBlockers();
        StartCoroutine(ReleaseBlockersAfterDelay());

        if (!string.IsNullOrEmpty(rootLayerAfterBreak))
        {
            int lyr = LayerMask.NameToLayer(rootLayerAfterBreak);
            if (lyr != -1) gameObject.layer = lyr;
        }

        var srcSprite = _sr.sprite;
        var tex = srcSprite.texture;
        if (!tex.isReadable)
        {
            Debug.LogWarning("[SpriteShatter2D] 貼圖不可讀，若要色彩取樣請勾 Read/Write Enabled。", this);
        }

        Rect spriteRect = srcSprite.rect;       // 像素座標中的區塊
        float ppu = srcSprite.pixelsPerUnit;    // 每單位多少像素
        string originalSortingLayer = _sr.sortingLayerName;
        int originalOrderInLayer = _sr.sortingOrder;
        Material originalMaterial = _sr.sharedMaterial;

        if (hideOriginalOnShatter) _sr.enabled = false;

        // 收集玩家 Collider（忽略碰撞用）
        List<Collider2D> playerCols = new List<Collider2D>();
        if (ignorePlayerByCode)
        {
            Transform playerT = playerTransform != null ? playerTransform
                                : (GameObject.FindGameObjectWithTag(playerTag)?.transform);
            if (playerT != null) playerCols.AddRange(playerT.GetComponentsInChildren<Collider2D>());
        }

        if (useSquareBlocks)
        {
            // ==== 方塊模式 ====
            int bx = Mathf.Max(1, blockPixelSize);
            int by = bx; // 正方形
            int colsCount = Mathf.CeilToInt(spriteRect.width / bx);
            int rowsCount = Mathf.CeilToInt(spriteRect.height / by);

            // spriteRect 中心（像素）
            Vector2 rectCenterPx = new Vector2(spriteRect.x + spriteRect.width * 0.5f,
                                               spriteRect.y + spriteRect.height * 0.5f);

            for (int ry = 0; ry < rowsCount; ry++)
            {
                for (int rx = 0; rx < colsCount; rx++)
                {
                    float rxPx = spriteRect.x + rx * bx;
                    float ryPx = spriteRect.y + ry * by;
                    float w = Mathf.Min(bx, spriteRect.x + spriteRect.width - rxPx);
                    float h = Mathf.Min(by, spriteRect.y + spriteRect.height - ryPx);
                    if (w <= 0 || h <= 0) continue;

                    // 世界相對位移（以像素差 / ppu）
                    Vector2 tileCenterPx = new Vector2(rxPx + w * 0.5f, ryPx + h * 0.5f);
                    Vector2 offsetUnits = (tileCenterPx - rectCenterPx) / ppu;

                    // 建立方塊物件
                    GameObject piece = new GameObject($"Block_{ry}_{rx}");
                    piece.transform.SetParent(transform, false);
                    piece.transform.localPosition = offsetUnits;
                    piece.transform.localRotation = Quaternion.identity;
                    piece.transform.localScale = Vector3.one;

                    // 指定 Layer
                    if (!string.IsNullOrEmpty(fragmentsLayerName))
                    {
                        int layer = LayerMask.NameToLayer(fragmentsLayerName);
                        if (layer != -1) piece.layer = layer;
                    }

                    // SpriteRenderer：用 override 的方塊或 1x1 白
                    var psr = piece.AddComponent<SpriteRenderer>();
                    psr.sprite = overrideBlockSprite != null ? overrideBlockSprite : _runtimeWhiteSquare;
                    psr.sharedMaterial = originalMaterial;
                    psr.sortingLayerName = string.IsNullOrEmpty(fragmentSortingLayerName) ? originalSortingLayer : fragmentSortingLayerName;
                    psr.sortingOrder = (fragmentOrderInLayer == -9999) ? originalOrderInLayer : fragmentOrderInLayer;

                    // 顏色取樣（平均色）
                    if (colorizeFromTexture && tex.isReadable)
                    {
                        Color avg = SampleAverage(tex, Mathf.RoundToInt(rxPx), Mathf.RoundToInt(ryPx),
                                                  Mathf.RoundToInt(w), Mathf.RoundToInt(h));
                        psr.color = avg;
                    }

                    // 依方塊像素大小，等比縮放到世界單位
                    piece.transform.localScale = new Vector3(w / 1f / ppu, h / 1f / ppu, 1f);

                    var rb = piece.AddComponent<Rigidbody2D>();
                    rb.gravityScale = fragmentsGravityScale;
                    rb.drag = fragmentsLinearDrag;
                    rb.angularDrag = fragmentsAngularDrag;

                    if (addBoxCollider2D)
                    {
                        var col = piece.AddComponent<BoxCollider2D>();
                        col.isTrigger = fragmentsUseTrigger;

                        if (ignorePlayerByCode && playerCols.Count > 0)
                            foreach (var pcol in playerCols)
                                if (pcol) Physics2D.IgnoreCollision(col, pcol, true);
                    }

                    // 力度：較溫和
                    Vector2 worldCenter = transform.position;
                    Vector2 pieceWorldPos = piece.transform.position;
                    Vector2 dir = (pieceWorldPos - worldCenter).normalized;
                    dir.y *= verticalForceScale;

                    Vector2 randomVec = Random.insideUnitCircle * randomJitter;
                    Vector2 bias = extraDirectionBias;

                    Vector2 impulse = (dir * explodeForce + randomVec + bias) * forceMultiplier;
                    rb.AddForce(impulse, ForceMode2D.Impulse);
                    rb.AddTorque(Random.Range(-randomTorque, randomTorque), ForceMode2D.Impulse);

                    if (maxInitialSpeed > 0f && rb.velocity.sqrMagnitude > maxInitialSpeed * maxInitialSpeed)
                        rb.velocity = rb.velocity.normalized * maxInitialSpeed;

                    if (fragmentLifetime > 0f) Destroy(piece, fragmentLifetime);
                }
            }
        }
        else
        {
            // ==== 傳統切片（保留原功能備用） ====
            int colsInt = Mathf.Max(1, columns);
            int rowsInt = Mathf.Max(1, rows);
            float tileW = spriteRect.width / colsInt;
            float tileH = spriteRect.height / rowsInt;

            Vector2 rectCenterPx = new Vector2(spriteRect.x + spriteRect.width * 0.5f,
                                               spriteRect.y + spriteRect.height * 0.5f);

            for (int y = 0; y < rowsInt; y++)
            {
                for (int x = 0; x < colsInt; x++)
                {
                    float rxPx = spriteRect.x + x * tileW;
                    float ryPx = spriteRect.y + y * tileH;
                    Rect tileRect = new Rect(rxPx, ryPx, tileW, tileH);

                    Sprite sub = Sprite.Create(tex, tileRect, new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);

                    GameObject piece = new GameObject($"Piece_{y}_{x}");
                    piece.transform.SetParent(transform, false);

                    if (!string.IsNullOrEmpty(fragmentsLayerName))
                    {
                        int layer = LayerMask.NameToLayer(fragmentsLayerName);
                        if (layer != -1) piece.layer = layer;
                    }

                    Vector2 tileCenterPx = new Vector2(tileRect.x + tileRect.width * 0.5f,
                                                       tileRect.y + tileRect.height * 0.5f);
                    Vector2 offsetUnits = (tileCenterPx - rectCenterPx) / ppu;
                    piece.transform.localPosition = offsetUnits;
                    piece.transform.localRotation = Quaternion.identity;
                    piece.transform.localScale = Vector3.one;

                    var psr = piece.AddComponent<SpriteRenderer>();
                    psr.sprite = sub;
                    psr.sharedMaterial = originalMaterial;
                    psr.sortingLayerName = string.IsNullOrEmpty(fragmentSortingLayerName) ? originalSortingLayer : fragmentSortingLayerName;
                    psr.sortingOrder = (fragmentOrderInLayer == -9999) ? originalOrderInLayer : fragmentOrderInLayer;

                    var rb = piece.AddComponent<Rigidbody2D>();
                    rb.gravityScale = fragmentsGravityScale;
                    rb.drag = fragmentsLinearDrag;
                    rb.angularDrag = fragmentsAngularDrag;

                    BoxCollider2D col = null;
                    if (addBoxCollider2D)
                    {
                        col = piece.AddComponent<BoxCollider2D>();
                        col.isTrigger = fragmentsUseTrigger;
                        if (ignorePlayerByCode && playerCols.Count > 0)
                            foreach (var pcol in playerCols)
                                if (pcol) Physics2D.IgnoreCollision(col, pcol, true);
                    }

                    Vector2 worldCenter = transform.position;
                    Vector2 pieceWorldPos = piece.transform.position;
                    Vector2 dir = (pieceWorldPos - worldCenter).normalized;
                    dir.y *= verticalForceScale;

                    Vector2 randomVec = Random.insideUnitCircle * randomJitter;
                    Vector2 bias = extraDirectionBias;
                    Vector2 impulse = (dir * explodeForce + randomVec + bias) * forceMultiplier;

                    rb.AddForce(impulse, ForceMode2D.Impulse);
                    rb.AddTorque(Random.Range(-randomTorque, randomTorque), ForceMode2D.Impulse);

                    if (maxInitialSpeed > 0f && rb.velocity.sqrMagnitude > maxInitialSpeed * maxInitialSpeed)
                        rb.velocity = rb.velocity.normalized * maxInitialSpeed;

                    if (fragmentLifetime > 0f) Destroy(piece, fragmentLifetime);
                }
            }
        }

        _hasShattered = true;
    }

    private IEnumerator ReleaseBlockersAfterDelay()
    {
        if (releaseBlockDelay > 0f)
            yield return new WaitForSeconds(releaseBlockDelay);

        HandleBlockersAfterBreak();
    }

    private void HandleBlockersAfterBreak()
    {
        if (blockingColliders == null || blockingColliders.Count == 0) return;
        foreach (var c in blockingColliders)
        {
            if (!c) continue;
            switch (blockerModeAfterBreak)
            {
                case PostBreakBlockerMode.Disable: c.enabled = false; break;
                case PostBreakBlockerMode.SetTrigger: c.isTrigger = true; break;
                case PostBreakBlockerMode.Destroy: Destroy(c); break;
            }
        }
    }

    /// <summary>依阻擋碰撞器邊界生成臨時地板（非 Trigger）</summary>
    private void SpawnTemporaryFloorFromBlockers()
    {
        if (blockingColliders == null || blockingColliders.Count == 0) return;

        foreach (var bc in blockingColliders)
        {
            if (!bc || bc.isTrigger) continue; // 只處理真正在擋路的

            Bounds b = bc.bounds;
            var go = new GameObject("TempFloor");
            go.transform.position = new Vector3(b.center.x, b.center.y, transform.position.z);

            // Layer
            if (!string.IsNullOrEmpty(tempFloorLayerName))
            {
                int lyr = LayerMask.NameToLayer(tempFloorLayerName);
                if (lyr != -1) go.layer = lyr;
            }
            else
            {
                go.layer = bc.gameObject.layer; // 沿用原層
            }

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(b.size.x, Mathf.Max(0.01f, tempFloorThickness));

            // 可選：若你需要剛體，改成 Kinematic
            // var rb = go.AddComponent<Rigidbody2D>();
            // rb.bodyType = RigidbodyType2D.Kinematic;

            Destroy(go, Mathf.Max(0.05f, tempFloorLifetime));
        }
    }

    /// <summary>對指定像素區域取平均色（簡易取樣）</summary>
    private Color SampleAverage(Texture2D tex, int sx, int sy, int w, int h)
    {
        if (w <= 0 || h <= 0) return Color.white;
        var pixels = tex.GetPixels(sx, sy, w, h); // 需 Read/Write
        float r = 0, g = 0, b = 0, a = 0;
        int len = pixels.Length;
        for (int i = 0; i < len; i++) { var c = pixels[i]; r += c.r; g += c.g; b += c.b; a += c.a; }
        float inv = 1f / Mathf.Max(1, len);
        return new Color(r * inv, g * inv, b * inv, a * inv);
    }
}
