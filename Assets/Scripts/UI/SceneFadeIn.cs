using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public CanvasGroup fadeGroup;

    public float fadeDuration = 1f;

    private IEnumerator Start()
    {
        if (fadeGroup == null)
            yield break;

        fadeGroup.blocksRaycasts = true;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            fadeGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    t / fadeDuration);

            yield return null;
        }

        fadeGroup.alpha = 0f;

        fadeGroup.blocksRaycasts = false;
    }
}