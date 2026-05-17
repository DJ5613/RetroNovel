using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class SceneTransition : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup fadeGroup;

    public float fadeDuration = 0.5f;

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
        if (fadeGroup != null)
        {
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;

                fadeGroup.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        t / fadeDuration);

                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        if (delayBeforeLoad > 0f)
        {
            yield return new WaitForSeconds(
                delayBeforeLoad);
        }

        SceneManager.LoadScene(sceneNumber);
    }
}