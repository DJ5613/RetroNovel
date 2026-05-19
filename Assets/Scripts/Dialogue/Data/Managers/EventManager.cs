using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Databases")]
    public EventDatabase eventDatabase;

    [Header("Systems")]
    public DialogueManager dialogueManager;
    public AIConversationManager aiConversationManager;


    // =========================
    // START DAY EVENT
    // =========================

    public bool StartDayEvent()
    {
        Debug.Log(
            "Запуск дня: " + GameManager.Instance.currentDay);

        EventData selectedEvent = null;

        foreach (EventData eventData in eventDatabase.events)
        {
            if (eventData.day != GameManager.Instance.currentDay)
                continue;

            if (!ConditionChecker.HasRequiredFlags(
    eventData.requiredFlags))
            {
                continue;
            }

            if (selectedEvent == null)
            {
                selectedEvent = eventData;
            }
            else if (eventData.priority >
                     selectedEvent.priority)
            {
                selectedEvent = eventData;
            }
        }

        if (selectedEvent != null)
        {
            Debug.Log(
                "Selected event: " +
                selectedEvent.nodeID);

            dialogueManager.ShowNode(
                selectedEvent.nodeID);

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
        GameManager.Instance.currentDay++;

        Debug.Log(
            "Новый день: " + GameManager.Instance.currentDay);

        aiConversationManager.ResetDailyTalkLimit();

        bool success = StartDayEvent();

        if (!success)
        {
            GameManager.Instance.currentDay--;

            Debug.Log(
                "Больше дней нет");
        }
    }

    // =========================
    // GET CURRENT DAY
    // =========================

    public int GetCurrentDay()
    {
        return GameManager.Instance.currentDay;
    }

    

    
}