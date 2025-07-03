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

//using UnityEngine;
//using UnityEngine.Animations;


//public class MovingPlatform : MonoBehaviour
//{
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
//        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

//        if (Vector3.Distance(transform.position, target) < 0.1f)
//        {
//            target = (target == pointA.position) ? pointB.position : pointA.position;
//        }
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        // 檢查是否是玩家，並將其父物件設為平台
//        Player player = collision.gameObject.GetComponent<Player>();
//        if (player != null)
//        {
//            player.transform.SetParent(this.transform);
//            if ()
//                player.transform.SetParent(null);
//        }
//    }

//    private void OnCollisionExit2D(Collision2D collision)
//    {
//        // 玩家離開平台後取消父子關係
//        Player player = collision.gameObject.GetComponent<Player>();
//        if (player != null)
//            player.transform.SetParent(null);
//    }
//}