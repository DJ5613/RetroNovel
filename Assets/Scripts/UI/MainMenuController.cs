using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private SettingsPanel _settingsPanel;

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsDelayed());
    }

    private System.Collections.IEnumerator OpenSettingsDelayed()
    {
        // Ждём пока анимация кнопки закончится
        yield return new WaitForSeconds(0.2f);
        _settingsPanel.Open();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _mainPanel.SetActive(false);
    }

    private System.Collections.IEnumerator HideMainPanelAfterBlur()
    {
        // Ждём пока блюр сделает скриншот
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _mainPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        _settingsPanel.Close();
        _mainPanel.SetActive(true);
    }
}