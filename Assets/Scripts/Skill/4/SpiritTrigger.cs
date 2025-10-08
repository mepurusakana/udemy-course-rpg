using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritTrigger : MonoBehaviour
{
    private SpiritController spirit;

        private void Awake()
    {
        spirit = GetComponentInParent<SpiritController>();
    }

    public void SpiritAttack()
    {
        if (spirit != null)
        {
            spirit.FireMissileEvent();
        }
    }
}
