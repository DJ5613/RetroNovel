using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Choices")]
    public GameObject choicePrefab;
    public Transform choicesContainer;

    [Header("Systems")]
    public DialogueDatabase database;
    public RelationshipSystem relationshipSystem;
    public CharacterManager characterManager;
    public BackgroundManager backgroundManager;
    public EventManager eventManager;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    private DialogueNode currentNode;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    // =========================
    // START
    // =========================

    void Start()
    {
        characterManager.HideAll();

        if (eventManager != null)
        {
            eventManager.StartDayEvent();
        }
    }

    // =========================
    // SHOW NODE
    // =========================

    public void ShowNode(string nodeID)
    {
        Debug.Log("SHOW NODE: " + nodeID);

        ClearChoices();

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
        {
            Debug.LogError(
                "Нода не найдена: " + nodeID);

            return;
        }

        // =========================
        // REQUIRED FLAGS
        // =========================

        if (currentNode.requiredFlags != null)
        {
            foreach (string flag in currentNode.requiredFlags)
            {
                if (!GameFlags.Instance.HasFlag(flag))
                {
                    Debug.Log(
                        "Нет нужного флага: " + flag);

                    return;
                }
            }
        }

        // =========================
        // SET FLAGS
        // =========================

        if (currentNode.setFlags != null)
        {
            foreach (string flag in currentNode.setFlags)
            {
                GameFlags.Instance.SetFlag(flag);
            }
        }

        // =========================
        // BACKGROUND
        // =========================

        if (!string.IsNullOrEmpty(currentNode.background))
        {
            backgroundManager.ChangeBackground(
                currentNode.background);
        }

        // =========================
        // SHOW CHARACTERS
        // =========================

        if (currentNode.setCharacters != null)
        {
            foreach (var character in currentNode.setCharacters)
            {
                characterManager.ShowCharacter(
                    character.name,
                    character.emotion,
                    character.position
                );
            }
        }

        // =========================
        // CHANGE EMOTIONS
        // =========================

        if (currentNode.emotionChanges != null)
        {
            foreach (var change in currentNode.emotionChanges)
            {
                characterManager.ChangeEmotion(
                    change.name,
                    change.emotion
                );
            }
        }

        // =========================
        // HIDE CHARACTERS
        // =========================

        if (currentNode.hideCharacters != null)
        {
            foreach (string characterName in currentNode.hideCharacters)
            {
                characterManager.HideCharacter(
                    characterName);
            }
        }

        // =========================
        // TEXT
        // =========================

        nameText.text = currentNode.speaker;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(
                TypeText(currentNode.text));

        // =========================
        // ACTIVE SPEAKER
        // =========================

        if (characterManager.HasCharacter(currentNode.speaker))
        {
            characterManager.SetSpeaker(
                currentNode.speaker);
        }

        // =========================
        // CHOICES
        // =========================

        if (currentNode.choices != null &&
            currentNode.choices.Count > 0)
        {
            foreach (DialogueChoice choice in currentNode.choices)
            {
                GameObject buttonObj =
                    Instantiate(
                        choicePrefab,
                        choicesContainer);

                buttonObj
                    .GetComponentInChildren<TextMeshProUGUI>()
                    .text = choice.text;

                buttonObj
                    .GetComponent<Button>()
                    .onClick
                    .AddListener(() =>
                    {
                        OnChoiceSelected(choice);
                    });
            }
        }
    }

    // =========================
    // CONTINUE DIALOGUE
    // =========================

    public void ContinueDialogue()
    {
        if (currentNode == null)
            return;

        // если есть выборы
        if (currentNode.choices != null &&
            currentNode.choices.Count > 0)
        {
            return;
        }

        // если текст ещё печатается
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text =
                currentNode.text;

            isTyping = false;

            return;
        }

        NextDialogue();
    }

    // =========================
    // NEXT DIALOGUE
    // =========================

    void NextDialogue()
    {
        // есть следующая нода
        if (!string.IsNullOrEmpty(currentNode.nextNodeID))
        {
            ShowNode(currentNode.nextNodeID);

            return;
        }

        // конец дня
        Debug.Log("Конец дня");

        ClearChoices();

        if (eventManager != null)
        {
            eventManager.NextDay();
        }
    }

    // =========================
    // CHOICE SELECTED
    // =========================

    void OnChoiceSelected(DialogueChoice choice)
    {
        relationshipSystem.ApplyChoice(choice);

        bool passed = true;

        // =========================
        // RELATIONSHIP CHECKS
        // =========================

        if (relationshipSystem.sadako <
            choice.requiredSadako)
        {
            passed = false;
        }

        if (relationshipSystem.sumiko <
            choice.requiredSumiko)
        {
            passed = false;
        }

        if (relationshipSystem.teruko <
            choice.requiredTeruko)
        {
            passed = false;
        }

        string nextID =
            choice.nextNodeID;

        // если не прошёл проверку
        if (!passed &&
            !string.IsNullOrEmpty(choice.failNodeID))
        {
            nextID = choice.failNodeID;
        }

        if (string.IsNullOrEmpty(nextID))
        {
            Debug.LogError(
                "Нет следующей ноды!");

            return;
        }

        ShowNode(nextID);
    }

    // =========================
    // CLEAR CHOICES
    // =========================

    void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // =========================
    // TYPEWRITER
    // =========================

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed);
        }

        isTyping = false;
    }

    // =========================
    // GET CURRENT NODE ID
    // =========================

    public string GetCurrentNodeID()
    {
        if (currentNode == null)
            return "";

        return currentNode.id;
    }

    // =========================
    // GET CURRENT SPEAKER
    // =========================

    public string GetCurrentSpeaker()
    {
        if (currentNode == null)
            return "";

        return currentNode.speaker;
    }
}