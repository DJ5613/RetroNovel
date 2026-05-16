using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    [Header("Systems")]
    public DialogueManager dialogueManager;
    public RelationshipSystem relationshipSystem;
    public SaveSlotUI[] saveSlots;
    public EventManager eventManager;
    public AIConversationManager aiConversationManager;

    // =========================
    // SAVE GAME
    // =========================

    public void SaveGame(int slot)
    {
        SaveData data = new SaveData();

        // =========================
        // CURRENT NODE
        // =========================

        data.currentNodeID =
            dialogueManager.GetCurrentNodeID();

        // =========================
        // RELATIONSHIPS
        // =========================

        data.sadakoRelationship =
            relationshipSystem.sadako;

        data.sumikoRelationship =
            relationshipSystem.sumiko;

        data.terukoRelationship =
            relationshipSystem.teruko;

        // =========================
        // FLAGS
        // =========================

        data.flags =
            GameFlags.Instance.GetAllFlags();

        // =========================
        // CHARACTERS
        // =========================

        data.characters =
            dialogueManager.characterManager
                .GetCharacters();

        data.sumikoHistory =
    aiConversationManager.GetHistory("Sumiko");

        data.terukoHistory =
            aiConversationManager.GetHistory("Teruko");

        data.sadakoHistory =
            aiConversationManager.GetHistory("Sadako");

        // =========================
        // SAVE JSON
        // =========================

        // =========================
        // SAVE INFO
        // =========================

        data.currentDay =
            eventManager.currentDay;

        data.saveTime =
            System.DateTime.Now.ToString(
                "dd.MM.yyyy HH:mm");

        data.routeName =
            dialogueManager.GetCurrentSpeaker();

        // =========================
        // SAVE JSON
        // =========================

        string json =
            JsonUtility.ToJson(data, true);

        string path = GetSavePath(slot);

        File.WriteAllText(path, json);

        Debug.Log(
            "Сохранено в слот: " + slot);
        RefreshSlots();
    }

    // =========================
    // LOAD GAME
    // =========================

    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        // =========================
        // CHECK FILE
        // =========================

        if (!File.Exists(path))
        {
            Debug.LogWarning(
                "Слот пуст: " + slot);

            return;
        }

        // =========================
        // LOAD JSON
        // =========================

        string json =
            File.ReadAllText(path);

        SaveData data =
            JsonUtility.FromJson<SaveData>(json);

        // =========================
        // RELATIONSHIPS
        // =========================

        relationshipSystem.sadako =
            data.sadakoRelationship;

        relationshipSystem.sumiko =
            data.sumikoRelationship;

        relationshipSystem.teruko =
            data.terukoRelationship;

        // =========================
        // FLAGS
        // =========================

        GameFlags.Instance.LoadFlags(data.flags);

        eventManager.currentDay =
    data.currentDay;

        aiConversationManager.SetHistory(
    "Sumiko",
    data.sumikoHistory);

        aiConversationManager.SetHistory(
            "Teruko",
            data.terukoHistory);

        aiConversationManager.SetHistory(
            "Sadako",
            data.sadakoHistory);

        // =========================
        // LOAD NODE
        // =========================

        dialogueManager.ShowNode(
            data.currentNodeID);

        // =========================
        // RESTORE CHARACTERS
        // =========================

        dialogueManager.characterManager
            .LoadCharacters(data.characters);

        // =========================
        // RESTORE ACTIVE SPEAKER
        // =========================

        dialogueManager.characterManager
            .SetSpeaker(
                dialogueManager.GetCurrentSpeaker());

        Debug.Log(
            "Загружен слот: " + slot);
    }

    // =========================
    // SAVE PATH
    // =========================

    string GetSavePath(int slot)
    {
        return Application.persistentDataPath +
               "/save_slot_" +
               slot +
               ".json";
    }
    void RefreshSlots()
    {
        foreach (SaveSlotUI slotUI in saveSlots)
        {
            slotUI.Refresh();
        }
    }
}