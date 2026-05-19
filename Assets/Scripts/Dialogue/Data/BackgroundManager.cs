using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public Image backgroundImage;

    private Dictionary<string, Sprite> backgroundCache =
    new Dictionary<string, Sprite>();

    public void ChangeBackground(string backgroundName)
    {
        string path = $"Sprites/Backgrounds/{backgroundName}";

        Sprite bg;

        if (!backgroundCache.TryGetValue(
            path,
            out bg))
        {
            bg = Resources.Load<Sprite>(path);

            backgroundCache[path] = bg;
        }

        if (bg != null)
        {
            backgroundImage.sprite = bg;
        }
        else
        {
            Debug.LogWarning("‘он не найден: " + path);
        }
    }
}