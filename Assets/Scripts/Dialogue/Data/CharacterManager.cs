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

    public Color inactiveColor =
        new Color(0.5f, 0.5f, 0.5f, 1f);

    // активные персонажи
    private Dictionary<string, CharacterData> activeCharacters =
        new Dictionary<string, CharacterData>();

    // =========================
    // SHOW CHARACTER
    // =========================

    public void ShowCharacter(
        string characterName,
        string emotion,
        string position)
    {
        string path =
            $"Sprites/Characters/{characterName}/{emotion}";

        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning(
                "Спрайт не найден: " + path);

            return;
        }

        Image slot = GetSlot(position);

        if (slot == null)
        {
            Debug.LogWarning(
                "Слот не найден: " + position);

            return;
        }

        // показать слот
        slot.enabled = true;

        // установить спрайт
        slot.sprite = sprite;

        // затемнить по умолчанию
        slot.color = inactiveColor;

        // обновить/добавить персонажа
        if (activeCharacters.ContainsKey(characterName))
        {
            activeCharacters[characterName].emotion =
                emotion;

            activeCharacters[characterName].position =
                position;
        }
        else
        {
            activeCharacters.Add(
                characterName,
                new CharacterData
                {
                    emotion = emotion,
                    position = position
                });
        }
    }

    // =========================
    // CHANGE EMOTION
    // =========================

    public void ChangeEmotion(
        string characterName,
        string newEmotion)
    {
        if (!activeCharacters.ContainsKey(characterName))
        {
            Debug.LogWarning(
                "Персонаж не найден: " + characterName);

            return;
        }

        string position =
            activeCharacters[characterName].position;

        ShowCharacter(
            characterName,
            newEmotion,
            position);
    }

    // =========================
    // HIDE CHARACTER
    // =========================

    public void HideCharacter(string characterName)
    {
        if (!activeCharacters.ContainsKey(characterName))
            return;

        string position =
            activeCharacters[characterName].position;

        Image slot = GetSlot(position);

        if (slot != null)
        {
            slot.enabled = false;
            slot.sprite = null;
        }

        activeCharacters.Remove(characterName);
    }

    // =========================
    // HIDE ALL
    // =========================

    public void HideAll()
    {
        ClearSlot(leftSlot);
        ClearSlot(centerSlot);
        ClearSlot(rightSlot);

        activeCharacters.Clear();
    }

    void ClearSlot(Image slot)
    {
        slot.enabled = false;
        slot.sprite = null;
    }

    // =========================
    // SET SPEAKER
    // =========================

    public void SetSpeaker(string speakerName)
    {
        // затемняем всех
        DimAll();

        if (string.IsNullOrEmpty(speakerName))
            return;

        if (!activeCharacters.ContainsKey(speakerName))
        {
            Debug.LogWarning(
                "Активный персонаж не найден: " +
                speakerName);

            return;
        }

        string position =
            activeCharacters[speakerName].position;

        Image slot = GetSlot(position);

        if (slot == null)
            return;

        slot.color = activeColor;
    }

    // =========================
    // HAS CHARACTER
    // =========================

    public bool HasCharacter(string characterName)
    {
        return activeCharacters.ContainsKey(characterName);
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
    // SAVE CHARACTERS
    // =========================

    public List<CharacterSaveData> GetCharacters()
    {
        List<CharacterSaveData> list =
            new List<CharacterSaveData>();

        foreach (var pair in activeCharacters)
        {
            list.Add(
                new CharacterSaveData
                {
                    name = pair.Key,
                    emotion = pair.Value.emotion,
                    position = pair.Value.position
                });
        }

        return list;
    }

    // =========================
    // LOAD CHARACTERS
    // =========================

    public void LoadCharacters(
        List<CharacterSaveData> characters)
    {
        HideAll();

        foreach (var character in characters)
        {
            ShowCharacter(
                character.name,
                character.emotion,
                character.position);
        }
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