using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class RippleEffect : MonoBehaviour, IPointerDownHandler
{
    public GameObject ripplePrefab;
    public float rippleDuration = 0.42f;   // скорость волны (чем меньше — тем быстрее)
    public float maxScale = 4.2f;          // насколько сильно растягивается (подбери под размер кнопки)
    public float startScale = 0.1f;        // начальный размер (маленький, чтобы выглядело как вспышка из центра)

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject ripple = Instantiate(ripplePrefab, transform);
        RectTransform rt = ripple.GetComponent<RectTransform>();

        rt.anchoredPosition = Vector2.zero;   // строго центр
        rt.localScale = Vector3.one * startScale;

        StartCoroutine(AnimateRipple(rt));
    }

    private IEnumerator AnimateRipple(RectTransform rt)
    {
        float time = 0f;
        Image img = rt.GetComponent<Image>();

        while (time < rippleDuration)
        {
            time += Time.deltaTime;
            float progress = time / rippleDuration;

            // Растягиваем сильно
            float currentScale = Mathf.Lerp(startScale, maxScale, progress);
            rt.localScale = Vector3.one * currentScale;

            // Плавно гасим прозрачность (волна становится прозрачнее к концу)
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(0.9f, 0.0f, Mathf.Pow(progress, 1.8f)); // нелинейное затухание — выглядит лучше
                img.color = c;
            }

            yield return null;
        }

        Destroy(rt.gameObject);
    }
}