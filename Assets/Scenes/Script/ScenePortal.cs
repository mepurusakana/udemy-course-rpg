using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Spawn Position in Target Scene")]
    [SerializeField] private Vector3 targetSpawnPosition;

    [Header("Visual (Optional)")]
    [SerializeField] private SpriteRenderer portalSprite;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            Debug.Log($"Player entering portal to {targetSceneName}");

            if (SceneTransitionManager.instance != null)
            {
                SceneTransitionManager.instance.TransitionToScene(targetSceneName, targetSpawnPosition);
            }
            else
            {
                Debug.LogError("SceneTransitionManager not found!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 在 Scene 視圖中顯示目標位置
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
    }
}