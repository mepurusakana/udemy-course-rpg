using UnityEngine;
using UnityEngine.UI;

public class UI_ArrowFloat : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 10f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeSpeed = 2f;

    private RectTransform rectTransform;
    private Image image;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        rectTransform.anchoredPosition = new Vector2(startPosition.x, newY);

        // 讓透明度在 0.5~1 之間變化
        //float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * fadeSpeed);
        //if (image != null)
        //{
        //    Color c = image.color;
        //    c.a = alpha;
        //    image.color = c;
        //}
    }
}
