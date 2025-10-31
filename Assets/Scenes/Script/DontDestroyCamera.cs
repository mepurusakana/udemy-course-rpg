using UnityEngine;

public class DontDestroyCamera : MonoBehaviour
{
    private static Camera mainCameraInstance;

    private void Awake()
    {
        if (mainCameraInstance == null)
        {
            mainCameraInstance = Camera.main;
            DontDestroyOnLoad(mainCameraInstance.transform.root.gameObject);
        }
    }
}
