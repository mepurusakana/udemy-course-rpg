using UnityEngine;

public class UICursorHandler : MonoBehaviour
{
    void OnEnable()
    {
        CursorController.OnUIOpen();
    }

    void OnDisable()
    {
        CursorController.OnUIClose();
    }
}