using UnityEngine;
using TMPro;
using System.Collections;

public class AIConversationManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;

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
    // SEND MESSAGE
    // =========================

    public void SendMessageToAI()
    {
        string playerMessage = inputField.text;

        if (string.IsNullOrEmpty(playerMessage))
            return;


        // =========================
        // SAVE PLAYER MESSAGE
        // =========================

        conversationHistory +=
            "Player: " +
            playerMessage +
            "\n";

        // =========================
        // BUILD PROMPT
        // =========================

        string prompt =
            BuildPrompt(playerMessage);

        Debug.Log(prompt);

        // =========================
        // GENERATE RESPONSE
        // =========================

        string response =
            GenerateFakeResponse(playerMessage);

        // =========================
        // SAVE AI RESPONSE
        // =========================

        conversationHistory +=
            dialogueManager.GetCurrentSpeaker() +
            ": " +
            response +
            "\n";

        // =========================
        // SHOW RESPONSE
        // =========================

        StartCoroutine(
            ShowResponse(response));

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

        // =========================
        // CHARACTER
        // =========================
        
        prompt +=
            "Character: " +
            currentCharacter +
            "\n";

        // =========================
        // PERSONALITY
        // =========================

        prompt +=
            "Personality: " +
            GetCharacterPrompt(currentCharacter) +
            "\n";

        // =========================
        // DAY
        // =========================

        prompt +=
            "Current Day: " +
            eventManager.currentDay +
            "\n";

        // =========================
        // RELATIONSHIPS
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
                "The world is unstable.\n";
        }

        if (GameFlags.Instance.HasFlag("cassetteMentioned"))
        {
            prompt +=
                "The cursed cassette exists.\n";
        }

        // =========================
        // HISTORY
        // =========================

        prompt +=
            "Conversation History:\n" +
            conversationHistory +
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
    // PERSONALITY PROMPTS
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
    // FAKE AI
    // =========================

    string GenerateFakeResponse(string message)
    {
        int currentDay =
            eventManager.currentDay;

        string currentCharacter =
            dialogueManager.GetCurrentSpeaker();

        switch (currentCharacter)
        {
            case "Sadako":

                if (currentDay >= 2)
                {
                    return
                        "Ты тоже заметил, что школа изменилась?";
                }

                return
                    "...Ты странный.";

            case "Sumiko":

                return
                    "Хм? Почему ты спрашиваешь?";

            case "Teruko":

                return
                    "Не думаю, что это хорошая идея.";
        }

        return "...";
    }

    // =========================
    // SHOW RESPONSE
    // =========================

    IEnumerator ShowResponse(string response)
    {
        string currentCharacter =
            dialogueManager.GetCurrentSpeaker();

        dialogueManager.nameText.text =
            currentCharacter;

        dialogueManager.dialogueText.text = "";

        dialogueManager.characterManager
            .SetSpeaker(currentCharacter);

        foreach (char letter in response)
        {
            dialogueManager.dialogueText.text +=
                letter;

            yield return new WaitForSeconds(
                dialogueManager.typingSpeed);
        }
    }

    public void ResetConversation()
    {
        conversationHistory = "";
    }

    public void TestAI()
    {
        StartCoroutine(
            openAIManager.SendRequest(
                "Say hello like a creepy anime girl.",
                OnAIResponse
            )
        );
    }

    void OnAIResponse(string response)
    {
        Debug.Log(response);
    }
}