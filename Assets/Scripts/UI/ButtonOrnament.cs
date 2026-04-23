using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonOrnament : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image left;
    public Image right;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        Color c = left.color;
        c.a = a;
        left.color = c;
        right.color = c;
    }
}