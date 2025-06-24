using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    public string entryName;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 3f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, entryName);
    }
}
