using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class QuitGame : MonoBehaviour
{
    [Header("Кнопка Выход из игры")]
    [Tooltip("Задержка перед выходом из игры (в секундах)")]
    public float delayBeforeQuit = 0.6f;     // можно настроить в инспекторе

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnQuitButtonClicked);
    }

    private void OnQuitButtonClicked()
    {
        StartCoroutine(QuitWithDelay());
    }

    private IEnumerator QuitWithDelay()
    {
        // Даём время отыграть анимацию волны или другие эффекты
        if (delayBeforeQuit > 0f)
        {
            yield return new WaitForSeconds(delayBeforeQuit);
        }
        else
        {
            yield return null;
        }

        // Выход из игры
        QuitApplication();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        // В редакторе Unity просто останавливаем игру
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Игра остановлена в редакторе (Quit)");
#else
        // На реальной сборке (Build) — выходим из приложения
        Application.Quit();
        Debug.Log("Application.Quit() вызван");
#endif
    }
}