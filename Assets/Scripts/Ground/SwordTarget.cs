using UnityEngine;
using UnityEngine.Events;


public class SwordTarget : MonoBehaviour
{
    public UnityEvent onHit;
    private bool triggered = false;

    private BoxCollider2D box;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    //public void Hit(FlyingSwordController sword)
    //{
    //    if (triggered) return;
    //    triggered = true;

    //    onHit.Invoke();
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FlyingSwordController sword=collision.GetComponent<FlyingSwordController>();
        if (sword == null) return;
        else onHit.Invoke();


        //Player player = collision.GetComponent<Player>();
        //if (player == null) return;

        //player.lastAttacker = transform; // ¨ú®øµL¼Ä player.stats.MakeInvincible(false);

        //PlayerStats stats = player.GetComponent<PlayerStats>();
        //if (stats == null) return;

        //StartCoroutine(HandleSpikeTrapSequence(player, stats));
    }
}
