using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIConversationManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI sendButtonText;
    public GameObject aiPanel;

    [Header("Systems")]
    public DialogueManager dialogueManager;
    public EventManager eventManager;
    public RelationshipSystem relationshipSystem;
    public OpenAIManager openAIManager;

    // =========================
    // FREE TIME
    // =========================

    private int remainingTalks;

    private string returnNodeID;

    // =========================
    // MEMORY
    // =========================

    private Dictionary<string, string> conversationHistories =
    new Dictionary<string, string>();

    // =========================
    // REQUEST LOCK
    // =========================

    private bool isRequesting = false;

    // =========================
    // START
    // =========================

    void Start()
    {
        remainingTalks =
            Random.Range(5, 8);

        aiPanel.SetActive(false);
    }

    // =========================
    // OPEN CHAT
    // =========================

    public void OpenAIChat(string returnNode)
    {
        returnNodeID = returnNode;

        aiPanel.SetActive(true);
    }

    // =========================
    // CLOSE CHAT
    // =========================

    public void CloseAIChat()
    {
        aiPanel.SetActive(false);
    }

    // =========================
    // LEAVE CONVERSATION
    // =========================

    public void LeaveConversation()
    {
        isRequesting = false;

        sendButton.interactable = true;

        sendButtonText.text = "Отправить";

        aiPanel.SetActive(false);

        dialogueManager.ShowNode(
            returnNodeID);
    }

    // =========================
    // SEND MESSAGE
    // =========================

    public void SendMessageToAI()
    {
        // already generating

        if (isRequesting)
            return;

        string playerMessage =
            inputField.text;

        // empty input

        if (string.IsNullOrWhiteSpace(playerMessage))
            return;

        // no talks left

        if (remainingTalks <= 0)
        {
            dialogueManager.ShowAIMessage(
                dialogueManager.GetCurrentSpeaker(),
                "Кажется, разговор уже закончился.");

            return;
        }

        isRequesting = true;

        // =========================
        // UI LOCK
        // =========================

        sendButton.interactable = false;

        sendButtonText.text = "...";

        // =========================
        // SAVE PLAYER MESSAGE
        // =========================

        string history =
    GetCurrentHistory();

        history +=
            "Player: " +
            playerMessage +
            "\n";

        // LIMIT MEMORY

        if (history.Length > 2500)
        {
            history =
                history.Substring(
                    history.Length - 2500);
        }

        SetCurrentHistory(history);

        // =========================
        // BUILD PROMPT
        // =========================

        string prompt =
            BuildPrompt(playerMessage);

        // =========================
        // SEND AI
        // =========================

        StartCoroutine(
            SendRealAI(prompt));

        // clear input

        inputField.text = "";
    }

    // =========================
    // BUILD PROMPT
    // =========================

    string BuildPrompt(string playerMessage)
    {
        string currentCharacter =
            dialogueManager.GetCurrentSpeaker();

        string prompt = "";

        // CHARACTER

        prompt +=
            "Character: " +
            currentCharacter +
            "\n";

        // PERSONALITY

        prompt +=
            "Personality: " +
            GetCharacterPrompt(currentCharacter) +
            "\n";

        // RULES

        prompt +=
            "Speak naturally like a real person.\n";

        prompt +=
            "Always answer ONLY in Russian.\n";

        prompt +=
            "Keep responses short.\n";

        prompt +=
            "Usually 1-3 sentences.\n";

        prompt +=
            "Use believable casual dialogue.\n";

        prompt +=
            "Never explain reasoning.\n";

        prompt +=
            "Never explain thoughts.\n";

        prompt +=
            "Never analyze the request.\n";

        prompt +=
            "Never output timestamps.\n";

        prompt +=
            "Never output metadata.\n";

        prompt +=
            "Never mix languages.\n";

        prompt +=
            "Output ONLY spoken dialogue.\n";

        // =========================
        // EMOTIONS
        // =========================

        prompt +=
            "You may include ONE emotion tag.\n";

        prompt +=
            "Format:\n";

        prompt +=
            "[emotion:nervous]\n";

        prompt +=
            "Available emotions:\n";

        prompt +=
            "neutral\n";

        prompt +=
            "happy\n";

        prompt +=
            "nervous\n";

        prompt +=
            "sad\n";

        prompt +=
            "angry\n";

        prompt +=
            "thinking\n";

        prompt +=
            "shy\n";

        // =========================
        // RELATIONSHIPS
        // =========================

        prompt +=
            "The character may include relationship tags.\n";

        prompt +=
            "Available tags: [relationship:+1], [relationship:-1]\n";

        prompt +=
            "Use them only when the player's behavior strongly affects emotions.\n";

        // =========================
        // DAY
        // =========================

        prompt +=
            "Current Day: " +
            eventManager.currentDay +
            "\n";

        // =========================
        // RELATIONSHIPS VALUES
        // =========================

        prompt +=
            "Sadako Relationship: " +
            relationshipSystem.sadako +
            "\n";

        prompt +=
            "Sumiko Relationship: " +
            relationshipSystem.sumiko +
            "\n";

        prompt +=
            "Teruko Relationship: " +
            relationshipSystem.teruko +
            "\n";

        // =========================
        // FLAGS
        // =========================

        if (GameFlags.Instance.HasFlag("worldBroken"))
        {
            prompt +=
                "The world feels unstable.\n";
        }

        if (GameFlags.Instance.HasFlag("cassetteMentioned"))
        {
            prompt +=
                "A cursed cassette exists.\n";
        }

        // =========================
        // HISTORY
        // =========================

        prompt +=
            "Conversation History:\n" +
            GetCurrentHistory() +
            "\n";

        // =========================
        // PLAYER MESSAGE
        // =========================

        prompt +=
            "Player said: " +
            playerMessage +
            "\n";

        return prompt;
    }

    // =========================
    // PERSONALITIES
    // =========================

    string GetCharacterPrompt(string character)
    {
        switch (character)
        {
            case "Sadako":

                return
                    "Sadako is quiet, observant, and emotionally restrained. " +
                    "She speaks in short calm sentences. " +
                    "She rarely jokes. " +
                    "Sometimes she sounds slightly distant or uneasy.";

            case "Sumiko":

                return
                    "Sumiko is energetic and sociable. " +
                    "She talks casually and tries to lighten tense situations with humor. " +
                    "Sometimes nervousness slips through her cheerful attitude.";

            case "Teruko":

                return
                    "Teruko is gentle, polite, and thoughtful. " +
                    "She speaks softly and carefully. " +
                    "She avoids conflict and chooses her words cautiously.";
        }

        return "";
    }

    // =========================
    // SEND REAL AI
    // =========================

    IEnumerator SendRealAI(string prompt)
    {
        bool finished = false;

        string aiResponse = "";

        yield return StartCoroutine(
            openAIManager.SendRequest(
                prompt,
                (response) =>
                {
                    aiResponse = response;
                    finished = true;
                }
            )
        );

        while (!finished)
        {
            yield return null;
        }

        // fallback

        if (string.IsNullOrEmpty(aiResponse))
        {
            aiResponse = "...";
        }

        // clean

        aiResponse =
            CleanResponse(aiResponse);

        // SAVE AI RESPONSE

        string history =
    GetCurrentHistory();

        history +=
            dialogueManager.GetCurrentSpeaker() +
            ": " +
            aiResponse +
            "\n";

        // LIMIT MEMORY

        if (history.Length > 2500)
        {
            history =
                history.Substring(
                    history.Length - 2500);
        }

        SetCurrentHistory(history);

        // SHOW RESPONSE

        yield return StartCoroutine(
            ShowResponse(aiResponse));

        remainingTalks--;

        // =========================
        // UNLOCK UI
        // =========================

        isRequesting = false;

        sendButton.interactable = true;

        sendButtonText.text = "Отправить";
    }

    // =========================
    // CLEAN RESPONSE
    // =========================

    string CleanResponse(string text)
    {
        text = text.Replace("\n", " ");

        text = text.Replace("\"", "");

        text = text.Replace("*", "");

        text = text.Replace("#", "");

        // relationship tags

        text = text.Replace("[relationship:+1]", "");
        text = text.Replace("[relationship:-1]", "");

        while (text.Contains("  "))
        {
            text =
                text.Replace("  ", " ");
        }

        return text.Trim();
    }

    // =========================
    // EXTRACT EMOTION
    // =========================

    string ExtractEmotion(ref string text)
    {
        if (text.Contains("[emotion:happy]"))
        {
            text =
                text.Replace(
                    "[emotion:happy]",
                    "");

            return "happy";
        }

        if (text.Contains("[emotion:nervous]"))
        {
            text =
                text.Replace(
                    "[emotion:nervous]",
                    "");

            return "nervous";
        }

        if (text.Contains("[emotion:sad]"))
        {
            text =
                text.Replace(
                    "[emotion:sad]",
                    "");

            return "sad";
        }

        if (text.Contains("[emotion:angry]"))
        {
            text =
                text.Replace(
                    "[emotion:angry]",
                    "");

            return "angry";
        }

        if (text.Contains("[emotion:neutral]"))
        {
            text =
                text.Replace(
                    "[emotion:neutral]",
                    "");
        }

        if (text.Contains("[emotion:thinking]"))
        {
            text =
                text.Replace(
                    "[emotion:thinking]",
                    "");

            return "thinking";
        }

        if (text.Contains("[emotion:shy]"))
        {
            text =
                text.Replace(
                    "[emotion:shy]",
                    "");

            return "shy";
        }

        return "neutral";
    }

    // =========================
    // EXTRACT RELATIONSHIP
    // =========================

    int ExtractRelationshipChange(ref string text)
    {
        if (text.Contains("[relationship:+1]"))
        {
            text =
                text.Replace(
                    "[relationship:+1]",
                    "");

            return 1;
        }

        if (text.Contains("[relationship:-1]"))
        {
            text =
                text.Replace(
                    "[relationship:-1]",
                    "");

            return -1;
        }

        return 0;
    }

    // =========================
    // SHOW RESPONSE
    // =========================

    IEnumerator ShowResponse(string response)
    {
        string speaker =
            dialogueManager.GetCurrentSpeaker();

        // =========================
        // EMOTION
        // =========================

        string emotion =
            ExtractEmotion(ref response);

        // =========================
        // RELATIONSHIP
        // =========================

        int relationshipChange =
            ExtractRelationshipChange(ref response);

        response = response.Trim();

        // =========================
        // APPLY RELATIONSHIP
        // =========================

        switch (speaker)
        {
            case "Sadako":

                relationshipSystem.sadako +=
                    relationshipChange;

                break;

            case "Sumiko":

                relationshipSystem.sumiko +=
                    relationshipChange;

                break;

            case "Teruko":

                relationshipSystem.teruko +=
                    relationshipChange;

                break;
        }

        // =========================
        // APPLY EMOTION
        // =========================

        dialogueManager.ApplyAIEmotion(
            speaker,
            emotion);

        // =========================
        // SHOW TEXT
        // =========================

        dialogueManager.ShowAIMessage(
            speaker,
            response);

        yield return null;
    }

    // =========================
    // RESET MEMORY
    // =========================

    public void ResetConversation()
    {
        SetCurrentHistory("");
    }

    // =========================
    // RESET DAILY LIMIT
    // =========================

    public void ResetDailyTalkLimit()
    {
        remainingTalks =
            Random.Range(5, 8);
    }

   

    string GetCurrentHistory()
    {
        string character =
            dialogueManager.GetCurrentSpeaker();

        if (!conversationHistories.ContainsKey(character))
        {
            conversationHistories[character] = "";
        }

        return conversationHistories[character];
    }

    void SetCurrentHistory(string history)
    {
        string character =
            dialogueManager.GetCurrentSpeaker();

        conversationHistories[character] = history;
    }

    public string GetHistory(string character)
    {
        if (!conversationHistories.ContainsKey(character))
        {
            return "";
        }

        return conversationHistories[character];
    }

    public void SetHistory(
        string character,
        string history)
    {
        conversationHistories[character] = history;
    }
}