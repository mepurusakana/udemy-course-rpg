using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(transform.parent.gameObject); // ¾P·´¤÷ª«¥ó
    }
}