using UnityEngine;

public class BossIntroController : MonoBehaviour
{
    public Player player;
    public Cinemachine.CinemachineVirtualCamera vcam;

    public void PlayerBusyOn()
    {
        player.isBusy = true;
        player.rb.velocity = Vector2.zero;
    }

    public void PlayerBusyOff()
    {
        player.isBusy = false;
    }

    public void SetBossCamera()
    {
        vcam.m_Lens.OrthographicSize = 28.5f;
    }
}
