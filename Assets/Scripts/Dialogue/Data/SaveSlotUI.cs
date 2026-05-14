using UnityEngine;
using TMPro;
using System.IO;

public class SaveSlotUI : MonoBehaviour
{
    public int slotNumber;

    public TextMeshProUGUI slotText;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string path =
            Application.persistentDataPath +
            "/save_slot_" +
            slotNumber +
            ".json";

        if (!File.Exists(path))
        {
            slotText.text =
                "Empty Slot";

            return;
        }

        string json =
            File.ReadAllText(path);

        SaveData data =
            JsonUtility.FromJson<SaveData>(json);

        slotText.text =
            "Day " + data.currentDay +
            "\n" +
            data.saveTime +
            "\n" +
            data.routeName;
    }
}