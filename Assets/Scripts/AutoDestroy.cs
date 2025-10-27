using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(transform.parent.gameObject); // 銷毀父物件
    }

    public void CloseFlash()
    {
        // 關閉閃屏
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.CloseFlash(0.1f);
        }

        // 嘗試尋找 Player 並修改 Sorting Layer
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            // 取得 Player 所有 SpriteRenderer（包含子物件）
            SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                sr.sortingLayerName = "Player";
            }

            Debug.Log("[AutoDestroy] 已將 Player 的 Sorting Layer 改為：Player");
        }
        else
        {
            Debug.LogWarning("[AutoDestroy] 找不到 Player 物件，無法修改 Sorting Layer！");
        }
    }
}
