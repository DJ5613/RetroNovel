using UnityEngine;

public class MenuButtonManager : MonoBehaviour
{
    public static MenuButtonManager Instance;

    [Header("Первая выделенная кнопка")]
    public MenuButtonUI firstActiveButton;     // ← перетащи сюда кнопку "Начать играть"

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Активируем первую кнопку при запуске
        if (firstActiveButton != null)
        {
            firstActiveButton.SetAsInitiallyActive();
            // Делаем её текущей активной
            currentActiveButton = firstActiveButton;
        }
    }

    private MenuButtonUI currentActiveButton;

    public void DeactivateAllExcept(MenuButtonUI activeButton)
    {
        if (currentActiveButton == activeButton) return;

        if (currentActiveButton != null)
        {
            currentActiveButton.Deactivate();
        }

        currentActiveButton = activeButton;
    }
}