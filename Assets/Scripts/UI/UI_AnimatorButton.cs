using UnityEngine;
using UnityEngine.EventSystems;


public class UI_AnimatorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.SetBool("Play", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("Play", false);
    }
}
