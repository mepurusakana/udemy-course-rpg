using UnityEngine;

public class UI_SaveSlotManager : MonoBehaviour
{
    [SerializeField] private UI_SaveSlot[] slots;

    private void Start()
    {
        foreach (var slot in slots)
        {
            slot.Refresh();
        }
    }
}