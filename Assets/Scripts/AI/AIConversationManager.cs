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

    [Header("Systems")]
    public DialogueManager dialogueManager;
    public EventManager eventManager;
    public RelationshipSystem relationshipSystem;
    public OpenAIManager openAIManager;

    // =========================
    // MEMORY
    // =========================

    private string conversationHistory = "";

    // =========================
    // REQUEST LOCK
    // =========================

    private bool isRequesting = false;

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

        prompt +=
"Start every response with an emotion tag.\n";

        prompt +=
        "Available emotions: neutral, happy, nervous, sad, angry.\n";

        prompt +=
        "Example: [nervous] Мне здесь не нравится.\n";

        // DAY
        prompt +=
            "Current Day: " +
            eventManager.currentDay +
            "\n";

        // RELATIONSHIPS
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
    // PERSONALITY
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

        // SHOW RESPONSE
        yield return StartCoroutine(
            ShowResponse(aiResponse));

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
    string ExtractEmotion(ref string text)
    {
        if (!text.StartsWith("["))
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
    // SHOW RESPONSE
    // =========================

    IEnumerator ShowResponse(string response)
    {
        string speaker =
            dialogueManager.GetCurrentSpeaker();

        string emotion =
            ExtractEmotion(ref response);

        dialogueManager.ApplyAIEmotion(
            speaker,
            emotion);

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
}