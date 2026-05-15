using UnityEngine;
using TMPro;
using System.Collections;
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

    private string conversationHistory = "";

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

        conversationHistory +=
            "Player: " +
            playerMessage +
            "\n";

        // LIMIT MEMORY
        if (conversationHistory.Length > 4000)
        {
            conversationHistory =
                conversationHistory.Substring(
                    conversationHistory.Length - 4000);
        }

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

        // EMOTIONS

        prompt +=
            "Start every response with an emotion tag.\n";

        prompt +=
            "Available emotions: neutral, happy, nervous, sad, angry.\n";

        prompt +=
            "Example: [nervous] Мне здесь не нравится.\n";

        // RELATIONSHIPS

        prompt +=
            "The character may include relationship tags.\n";

        prompt +=
            "Available tags: [relationship:+1], [relationship:-1]\n";

        prompt +=
            "Use them only when the player's behavior strongly affects emotions.\n";

        // DAY

        prompt +=
            "Current Day: " +
            eventManager.currentDay +
            "\n";

        // RELATIONSHIPS VALUES

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

        // FLAGS

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

        // HISTORY

        prompt +=
            "Conversation History:\n" +
            conversationHistory +
            "\n";

        // PLAYER MESSAGE

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

        conversationHistory +=
            dialogueManager.GetCurrentSpeaker() +
            ": " +
            aiResponse +
            "\n";

        // LIMIT MEMORY

        if (conversationHistory.Length > 4000)
        {
            conversationHistory =
                conversationHistory.Substring(
                    conversationHistory.Length - 4000);
        }

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
        if (!text.StartsWith("[") ||
            text.StartsWith("[relationship"))
        {
            return "neutral";
        }

        int end =
            text.IndexOf("]");

        if (end == -1)
        {
            return "neutral";
        }

        string emotion =
            text.Substring(1, end - 1);

        text =
            text.Substring(end + 1).Trim();

        return emotion;
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

        // emotion

        string emotion =
            ExtractEmotion(ref response);

        // relationship

        int relationshipChange =
            ExtractRelationshipChange(ref response);

        // apply relationship

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

        // apply emotion

        dialogueManager.ApplyAIEmotion(
            speaker,
            emotion);

        // show text

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
        conversationHistory = "";
    }

    // =========================
    // RESET DAILY LIMIT
    // =========================

    public void ResetDailyTalkLimit()
    {
        remainingTalks =
            Random.Range(5, 8);
    }
}