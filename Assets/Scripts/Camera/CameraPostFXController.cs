using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPostFXController : MonoBehaviour
{
    public static CameraPostFXController instance;

    private Volume volume;
    private DepthOfField dof;
    private Coroutine blurCoroutine;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        volume = GetComponent<Volume>();
        volume.profile.TryGet(out dof);

        // 確保一開始是清楚的
        if (dof != null)
            dof.active = false;
    }

    public void PlayHitBlur(float duration = 0.12f)
    {
        if (dof == null) return;

        if (blurCoroutine != null)
            StopCoroutine(blurCoroutine);

        blurCoroutine = StartCoroutine(HitBlurRoutine(duration));
    }

    private IEnumerator HitBlurRoutine(float duration)
    {
        dof.active = true;

        yield return new WaitForSeconds(duration);

        dof.active = false;
    }
}
