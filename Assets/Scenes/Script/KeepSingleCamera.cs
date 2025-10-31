using UnityEngine;

public class KeepSingleCamera : MonoBehaviour
{
    private static KeepSingleCamera instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}