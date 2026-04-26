using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class SceneTransition : MonoBehaviour
{
    [Header("Настройки перехода")]
    [Tooltip("Номер сцены в Build Settings")]
    public int sceneNumber = 1;

    [Tooltip("Задержка перед загрузкой следующей сцены (в секундах)")]
    [Range(0f, 5f)]                    // ограничиваем от 0 до 5 секунд (можно изменить)
    public float delayBeforeLoad = 0.5f;   // ← вот это поле будет видно и настраиваться в инспекторе

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    // Этот метод можно также вызывать из других скриптов, если нужно
    public void Transition()
    {
        StartCoroutine(LoadSceneWithDelay());
    }

    private void OnButtonClicked()
    {
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        // Ждём указанную задержку
        if (delayBeforeLoad > 0f)
        {
            yield return new WaitForSeconds(delayBeforeLoad);
        }
        else
        {
            yield return null; // если задержка = 0 — просто следующий кадр
        }

        // Загружаем сцену
        SceneManager.LoadScene(sceneNumber);
    }
}