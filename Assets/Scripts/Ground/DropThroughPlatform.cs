using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DropThroughPlatform : MonoBehaviour
{
    private Collider2D platformCollider;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 暫時忽略平台和玩家的碰撞
    /// </summary>
    public void DisableCollisionTemporarily(Collider2D playerCollider, float duration = 0.3f)
    {
        StartCoroutine(DisableCollision(playerCollider, duration));
    }

    private System.Collections.IEnumerator DisableCollision(Collider2D playerCollider, float duration)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(duration);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}
