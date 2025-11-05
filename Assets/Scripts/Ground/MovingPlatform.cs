using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum MovingPlatformType
    {
        BACK_FORTH,
        LOOP,
        ONCE
    }

    public float speed;
    public MovingPlatformType platformType = MovingPlatformType.BACK_FORTH;
    public bool isMovingAtStart = true;

    public Transform[] waypoints;
    public float[] waitTimes;

    private int currentIndex = 0;
    private int nextIndex = 1;
    private int direction = 1;

    public Rigidbody2D rb;
    public float waitTimer = 0f;
    private Vector2 velocity;

    public Vector2 CurrentVelocity => velocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        if (waypoints.Length < 2)
        {
            Debug.LogWarning("需要至少兩個 waypoints 才能移動平台");
            enabled = false;
            return;
        }

        if (waitTimes.Length != waypoints.Length)
        {
            Debug.LogWarning("waitTimes 長度需與 waypoints 相同，自動補齊為 0");
            System.Array.Resize(ref waitTimes, waypoints.Length);
        }

        waitTimer = waitTimes[currentIndex];
    }

    private void FixedUpdate()
    {
        if (!isMovingAtStart || waypoints.Length < 2)
            return;

        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            velocity = Vector2.zero;
            return;
        }

        Vector2 currentPos = rb.position;
        Vector2 targetPos = waypoints[nextIndex].position;
        Vector2 dir = (targetPos - currentPos).normalized;
        float distanceThisFrame = speed * Time.fixedDeltaTime;

        float distanceToTarget = Vector2.Distance(currentPos, targetPos);
        if (distanceThisFrame >= distanceToTarget)
        {
            rb.MovePosition(targetPos);
            velocity = dir * distanceToTarget;

            currentIndex = nextIndex;
            waitTimer = waitTimes[currentIndex];

            switch (platformType)
            {
                case MovingPlatformType.BACK_FORTH:
                    if (nextIndex == waypoints.Length - 1 || nextIndex == 0)
                        direction *= -1;
                    nextIndex += direction;
                    break;

                case MovingPlatformType.LOOP:
                    nextIndex = (nextIndex + 1) % waypoints.Length;
                    break;

                case MovingPlatformType.ONCE:
                    if (nextIndex < waypoints.Length - 1)
                        nextIndex++;
                    else
                        isMovingAtStart = false;
                    break;
            }
        }
        else
        {
            rb.MovePosition(currentPos + dir * distanceThisFrame);
            velocity = dir * distanceThisFrame;
        }
    }

    public void StartMoving() => isMovingAtStart = true;
    public void StopMoving() => isMovingAtStart = false;
    public void ResetPlatform()
    {
        rb.position = waypoints[0].position;
        currentIndex = 0;
        nextIndex = 1;
        direction = 1;
        waitTimer = waitTimes[0];
        isMovingAtStart = true;
    }
}

//using UnityEngine;

//public class MovingPlatform : MonoBehaviour
//{
//    public Vector2 CurrentVelocity { get; private set; }

//    [SerializeField] private Transform pointA;
//    [SerializeField] private Transform pointB;
//    [SerializeField] private float moveSpeed = 2f;

//    private Vector3 target;

//    private void Start()
//    {
//        target = pointB.position;
//    }

//    private void Update()
//    {
//        Vector3 previousPosition = transform.position;

//        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
//        CurrentVelocity = (transform.position - previousPosition) / Time.deltaTime;

//        if (Vector3.Distance(transform.position, target) < 0.1f)
//        {
//            target = (target == pointA.position) ? pointB.position : pointA.position;
//        }
//    }
//}