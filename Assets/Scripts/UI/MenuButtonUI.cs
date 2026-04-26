using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Текст")]
    public TextMeshProUGUI text;
    public Color normalColor = new Color(0.7f, 0.7f, 0.7f);
    public Color hoverColor = Color.white;

    [Header("Орнаменты")]
    public Image leftOrnament;
    public Image rightOrnament;

    [Header("Анимация")]
    public float animationDuration = 0.28f;
    public Vector3 hiddenScale = new Vector3(0.05f, 0.05f, 0.05f);
    public Vector3 targetScale = new Vector3(0.8f, 0.2f, 0.3f);

    private Coroutine currentAnim = null;
    private bool isActive = false;

    void Start()
    {
        // По умолчанию всё скрыто
        SetOrnamentsImmediate(hiddenScale, 0f);
        text.color = normalColor;
    }

    // Новый метод: сразу активирует кнопку без анимации (для первой кнопки)
    public void SetAsInitiallyActive()
    {
        isActive = true;
        text.color = hoverColor;
        SetOrnamentsImmediate(targetScale, 1f);   // сразу ставим в финальное состояние
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isActive) return;

        isActive = true;
        text.color = hoverColor;

        MenuButtonManager.Instance?.DeactivateAllExcept(this);
        StartAnimation(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ничего не делаем
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        text.color = normalColor;
        StartAnimation(false);
    }

    private void StartAnimation(bool show)
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);

        currentAnim = StartCoroutine(AnimateOrnaments(show));
    }

    private IEnumerator AnimateOrnaments(bool show)
    {
        Vector3 startScale = show ? hiddenScale : targetScale;
        Vector3 endScale = show ? targetScale : hiddenScale;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animationDuration);

            Vector3 currScale = Vector3.Lerp(startScale, endScale, t);
            float currAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            ApplyToOrnament(leftOrnament, currScale, currAlpha);
            ApplyToOrnament(rightOrnament, currScale, currAlpha);

            yield return null;
        }

        ApplyToOrnament(leftOrnament, endScale, endAlpha);
        ApplyToOrnament(rightOrnament, endScale, endAlpha);
    }

    private void ApplyToOrnament(Image img, Vector3 scale, float alpha)
    {
        if (img == null) return;
        img.rectTransform.localScale = scale;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetOrnamentsImmediate(Vector3 scale, float alpha)
    {
        ApplyToOrnament(leftOrnament, scale, alpha);
        ApplyToOrnament(rightOrnament, scale, alpha);
    }
}