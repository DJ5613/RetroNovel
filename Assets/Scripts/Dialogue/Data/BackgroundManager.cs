using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public Image backgroundImage;

    public void ChangeBackground(string backgroundName)
    {
        string path = $"Sprites/Backgrounds/{backgroundName}";

        Sprite bg = Resources.Load<Sprite>(path);

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