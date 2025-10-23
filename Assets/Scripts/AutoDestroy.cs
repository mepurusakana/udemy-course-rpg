using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(transform.parent.gameObject); // ¾P·´¤÷ª«¥ó
    }

    public void CloseFlash()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.CloseFlash(0.1f); // ©µ¿ð 0.3 ¬íÃö³¬
        }
    }
}