using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector2 CurrentVelocity { get; private set; }

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 target;

    private void Start()
    {
        target = pointB.position;
    }

    private void Update()
    {
        Vector3 previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        CurrentVelocity = (transform.position - previousPosition) / Time.deltaTime;

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }
    }
}