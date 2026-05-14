using UnityEngine;

public class EventManager : MonoBehaviour
{
    public EventDatabase eventDatabase;
    public DialogueManager dialogueManager;

    public void StartDayEvent()
    {
        int currentDay = GameManager.Instance.currentDay;

        foreach (EventData eventData in eventDatabase.events)
        {
            if (eventData.day != currentDay)
                continue;

            // проверка route
            if (!string.IsNullOrEmpty(eventData.requiredRoute))
            {
                if (!CheckRoute(eventData.requiredRoute))
                    continue;
            }

            // проверка флагов
            if (!string.IsNullOrEmpty(eventData.requiredFlag))
            {
                if (!CheckFlag(eventData.requiredFlag))
                    continue;
            }

            dialogueManager.ShowNode(eventData.nodeID);
            return;
        }

        Debug.Log("Событие не найдено");
    }

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