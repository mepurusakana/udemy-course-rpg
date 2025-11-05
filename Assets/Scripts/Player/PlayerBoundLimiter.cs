using UnityEngine;

public class PlayerBoundsLimiter : MonoBehaviour
{
    private Transform player;
    private PolygonCollider2D cameraBound;

    private void Start()
    {
        player = transform;
        FindCurrentCameraBound();
    }

    private void Update()
    {
        if (cameraBound == null)
        {
            FindCurrentCameraBound();
            return;
        }

        ClampPlayerWithinBounds();
    }

    private void FindCurrentCameraBound()
    {
        GameObject boundObj = GameObject.Find("CameraBound");
        if (boundObj != null)
            cameraBound = boundObj.GetComponent<PolygonCollider2D>();
    }

    private void ClampPlayerWithinBounds()
    {
        Bounds b = cameraBound.bounds;
        Vector3 pos = player.position;

        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

        player.position = pos;
    }
}

