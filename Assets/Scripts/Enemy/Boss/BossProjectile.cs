using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private float speed;
    private float lifeTime;
    private float selfRotation;

    public void Initialize(float speed, float lifeTime, float selfRotation)
    {
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.selfRotation = selfRotation;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;

        if (selfRotation != 0)
        {
            transform.Rotate(0, 0, selfRotation * Time.deltaTime);
        }
    }
}
