using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject saveMenu;
    public GameObject loadMenu;
    public GameObject dialogue;
    // =========================
    // SAVE MENU
    // =========================

    public void OpenSaveMenu()
    {
        saveMenu.SetActive(true);
        dialogue.SetActive(false);
    }

    public void CloseSaveMenu()
    {
        saveMenu.SetActive(false);
        dialogue.SetActive(true);
    }

    // =========================
    // LOAD MENU
    // =========================

    public void OpenLoadMenu()
    {
        loadMenu.SetActive(true);
        dialogue.SetActive(false);
    }

    public void CloseLoadMenu()
    {
        loadMenu.SetActive(false);
        dialogue.SetActive(true);
    }
}