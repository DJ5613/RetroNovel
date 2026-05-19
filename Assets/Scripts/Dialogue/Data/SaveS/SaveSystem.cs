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
            GameManager.Instance.currentDay;

        data.saveTime =
            System.DateTime.Now.ToString(
                "dd.MM.yyyy HH:mm");

        data.routeName =
    GetCurrentRouteName();

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

        // =========================
        // RESTORE DAY
        // =========================

        GameManager.Instance.currentDay =
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

    string GetCurrentRouteName()
    {
        if (GameFlags.Instance.HasFlag("sadako_route"))
            return "Sadako Route";

        if (GameFlags.Instance.HasFlag("sumiko_route"))
            return "Sumiko Route";

        if (GameFlags.Instance.HasFlag("teruko_route"))
            return "Teruko Route";

        return "Common Route";
    }
}