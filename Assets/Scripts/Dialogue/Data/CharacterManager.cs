using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public Image leftSlot;
    public Image centerSlot;
    public Image rightSlot;

    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);

    void Start()
    {
        HideAll();
    }

    public void ShowCharacter(string characterName, string emotion, string position)
    {
        string path = $"Sprites/Characters/{characterName}/{emotion}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning("Спрайт не найден: " + path);
            return;
        }

        Image targetSlot = GetSlot(position);

        if (targetSlot == null)
        {
            Debug.LogWarning("Позиция не найдена: " + position);
            return;
        }

        // ставим персонажа
        targetSlot.sprite = sprite;
        targetSlot.enabled = true;

        // затемняем ВСЕХ
        DimAll();

        // активного делаем ярким
        targetSlot.color = activeColor;
    }

    Image GetSlot(string position)
    {
        switch (position.ToLower())
        {
            case "left":
                return leftSlot;

            case "center":
                return centerSlot;

            case "right":
                return rightSlot;
        }

        return null;
    }

    void DimAll()
    {
        if (leftSlot.enabled)
            leftSlot.color = inactiveColor;

        if (centerSlot.enabled)
            centerSlot.color = inactiveColor;

        if (rightSlot.enabled)
            rightSlot.color = inactiveColor;
    }

    public void HideAll()
    {
        leftSlot.enabled = false;
        centerSlot.enabled = false;
        rightSlot.enabled = false;
    }

    public void HideCharacter(string position)
    {
        Image targetSlot = GetSlot(position);

        if (targetSlot != null)
        {
            targetSlot.enabled = false;
        }
    }
}