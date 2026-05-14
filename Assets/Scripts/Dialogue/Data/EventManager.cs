using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Databases")]
    public EventDatabase eventDatabase;

    [Header("Systems")]
    public DialogueManager dialogueManager;

    [Header("Day System")]
    public int currentDay = 1;

    // =========================
    // START DAY EVENT
    // =========================

    public bool StartDayEvent()
    {
        Debug.Log(
            "Запуск дня: " + currentDay);

        foreach (EventData eventData in eventDatabase.events)
        {
            // день не совпадает
            if (eventData.day != currentDay)
                continue;

            // =========================
            // ROUTE CHECK
            // =========================

            if (!string.IsNullOrEmpty(eventData.requiredRoute))
            {
                if (!CheckRoute(eventData.requiredRoute))
                    continue;
            }

            // =========================
            // FLAG CHECK
            // =========================

            if (!string.IsNullOrEmpty(eventData.requiredFlag))
            {
                if (!CheckFlag(eventData.requiredFlag))
                    continue;
            }

            // =========================
            // START EVENT
            // =========================

            dialogueManager.ShowNode(
                eventData.nodeID);

            dialogueManager.ShowNode(eventData.nodeID);

            return true;
        }
        Debug.LogWarning(
    "Событие для дня не найдено");

        return false;
    }

    // =========================
    // NEXT DAY
    // =========================

    public void NextDay()
    {
        currentDay++;

        Debug.Log(
            "Новый день: " + currentDay);

        bool success = StartDayEvent();

        if (!success)
        {
            currentDay--;

            Debug.Log(
                "Больше дней нет");
        }
    }

    // =========================
    // GET CURRENT DAY
    // =========================

    public int GetCurrentDay()
    {
        return currentDay;
    }

    // =========================
    // CHECK ROUTE
    // =========================

    bool CheckRoute(string route)
    {
        switch (route)
        {
            case "Sadako":
                return GameManager.Instance.sadakoRoute;

            case "Sumiko":
                return GameManager.Instance.sumikoRoute;

            case "Teruko":
                return GameManager.Instance.terukoRoute;
        }

        return false;
    }

    // =========================
    // CHECK FLAG
    // =========================

    bool CheckFlag(string flag)
    {
        switch (flag)
        {
            case "cassetteFound":
                return GameManager.Instance.cassetteFound;

            case "worldBroken":
                return GameManager.Instance.worldBroken;
        }

        return false;
    }
}