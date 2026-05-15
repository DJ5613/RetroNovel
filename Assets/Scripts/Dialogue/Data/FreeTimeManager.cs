using UnityEngine;

public class FreeTimeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject freeTimeMenu;

    [Header("Systems")]
    public DialogueManager dialogueManager;

    // =========================
    // OPEN MENU
    // =========================

    public void OpenFreeTime()
    {
        freeTimeMenu.SetActive(true);

        Debug.Log("Free Time начался");
    }

    // =========================
    // CLOSE MENU
    // =========================

    public void CloseFreeTime()
    {
        freeTimeMenu.SetActive(false);
    }

    // =========================
    // SELECT LOCATION
    // =========================

    public void SelectLocation(string nodeID)
    {
        CloseFreeTime();

        dialogueManager.ShowNode(nodeID);
    }
}