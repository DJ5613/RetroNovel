using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public float glowIntensity = 1.5f;

    private float defaultGlow;

    void Start()
    {
        defaultGlow = text.fontMaterial.GetFloat("_GlowPower");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.fontMaterial.SetFloat("_GlowPower", glowIntensity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.fontMaterial.SetFloat("_GlowPower", defaultGlow);
    }
}