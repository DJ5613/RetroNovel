using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    [Header("Character Slots")]
    public Image leftSlot;
    public Image centerSlot;
    public Image rightSlot;

    [Header("Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    // Храним кто где стоит
    private Dictionary<string, CharacterData> activeCharacters = new();

    // =========================
    // SHOW CHARACTER
    // =========================

    public void ShowCharacter(string characterName, string emotion, string position)
    {
        string path = $"Sprites/Characters/{characterName}/{emotion}";

        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning("Спрайт не найден: " + path);
            return;
        }

        Image slot = GetSlot(position);

        if (slot == null)
        {
            Debug.LogWarning("Слот не найден: " + position);
            return;
        }

        slot.sprite = sprite;
        slot.enabled = true;

        // По умолчанию затемняем
        slot.color = inactiveColor;

        // Если персонаж уже есть → обновляем
        if (activeCharacters.ContainsKey(characterName))
        {
            activeCharacters[characterName].emotion = emotion;
            activeCharacters[characterName].position = position;
        }
        else
        {
            activeCharacters.Add(characterName, new CharacterData
            {
                emotion = emotion,
                position = position
            });
        }
    }

    // =========================
    // CHANGE EMOTION
    // =========================

    public void ChangeEmotion(string characterName, string newEmotion)
    {
        if (!activeCharacters.ContainsKey(characterName))
        {
            Debug.LogWarning("Персонаж не найден: " + characterName);
            return;
        }

        string position = activeCharacters[characterName].position;

        ShowCharacter(characterName, newEmotion, position);
    }

    // =========================
    // HIDE CHARACTER
    // =========================

    public void HideCharacter(string characterName)
    {
        if (!activeCharacters.ContainsKey(characterName))
            return;

        string position = activeCharacters[characterName].position;

        Image slot = GetSlot(position);

        if (slot != null)
        {
            slot.enabled = false;
        }

        activeCharacters.Remove(characterName);
    }

    // =========================
    // SET ACTIVE SPEAKER
    // =========================

    public void SetSpeaker(string speakerName)
    {
        DimAll();

        if (!activeCharacters.ContainsKey(speakerName))
            return;

        string position = activeCharacters[speakerName].position;

        Image slot = GetSlot(position);

        if (slot != null)
        {
            slot.color = activeColor;
        }
    }

    // =========================
    // DIM ALL
    // =========================

    void DimAll()
    {
        if (leftSlot.enabled)
            leftSlot.color = inactiveColor;

        if (centerSlot.enabled)
            centerSlot.color = inactiveColor;

        if (rightSlot.enabled)
            rightSlot.color = inactiveColor;
    }

    // =========================
    // GET SLOT
    // =========================

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

    // =========================
    // HIDE ALL
    // =========================

    public void HideAll()
    {
        leftSlot.enabled = false;
        centerSlot.enabled = false;
        rightSlot.enabled = false;

        activeCharacters.Clear();
    }
}

// =========================
// CHARACTER DATA
// =========================

[System.Serializable]
public class CharacterData
{
    public string emotion;
    public string position;
}