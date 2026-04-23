using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public Image leftOrnament;
    public Image rightOrnament;

    public Color normalColor = new Color(0.7f, 0.7f, 0.7f);
    public Color hoverColor = Color.white;

    void Start()
    {
        SetState(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetState(false);
    }

    void SetState(bool hovered)
    {
        text.color = hovered ? hoverColor : normalColor;

        float alpha = hovered ? 1f : 0f;

        SetAlpha(leftOrnament, alpha);
        SetAlpha(rightOrnament, alpha);
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}