using UnityEngine;
using UnityEngine.Events;


public class SwordTarget : MonoBehaviour
{
    public UnityEvent onHit;
    private bool triggered = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Hit(FlyingSwordController sword)
    {
        if (triggered) return;
        triggered = true;

        onHit.Invoke();
    }
}
