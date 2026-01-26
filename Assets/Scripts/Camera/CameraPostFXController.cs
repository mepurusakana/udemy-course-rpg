using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPostFXController : MonoBehaviour
{
    public static CameraPostFXController instance;

    private Volume volume;
    private MotionBlur motionBlur;

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
        volume.profile.TryGet(out motionBlur);
    }

    public void PlayHitBlur(float intensity = 0.4f, float duration = 0.15f)
    {
        if (motionBlur == null) return;

        if (blurCoroutine != null)
            StopCoroutine(blurCoroutine);

        blurCoroutine = StartCoroutine(HitBlurRoutine(intensity, duration));
    }

    private IEnumerator HitBlurRoutine(float intensity, float duration)
    {
        motionBlur.intensity.value = intensity;
        yield return new WaitForSeconds(duration);
        motionBlur.intensity.value = 0f;
    }
}
