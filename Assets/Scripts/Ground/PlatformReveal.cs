using UnityEngine;
using System.Collections;

public class PlatformReveal : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public bool disableColliderUntilVisible = true;

    [Header("Float Up Settings")]
    public float floatUpDistance = 0.6f;
    public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private SpriteRenderer[] renderers;
    private Collider2D[] colliders;

    private Vector3 finalPosition;
    private Vector3 startPosition;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);

        finalPosition = transform.position;
        startPosition = finalPosition - Vector3.up * floatUpDistance;

        transform.position = startPosition;

        SetAlpha(0f);

        if (disableColliderUntilVisible)
            SetColliders(false);
    }

    public void Reveal()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            float alpha = Mathf.Lerp(0f, 1f, normalized);
            float floatT = floatCurve.Evaluate(normalized);

            SetAlpha(alpha);
            transform.position = Vector3.Lerp(startPosition, finalPosition, floatT);

            yield return null;
        }

        SetAlpha(1f);
        transform.position = finalPosition;

        if (disableColliderUntilVisible)
            SetColliders(true);
    }

    private void SetAlpha(float a)
    {
        foreach (var sr in renderers)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }

    private void SetColliders(bool enable)
    {
        foreach (var col in colliders)
            if (col != null)
                col.enabled = enable;
    }
}
