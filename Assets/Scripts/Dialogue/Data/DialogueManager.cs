using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private DialogueNode currentNode;

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
    // UPDATE
    // =========================

    void Update()
    {
        if (currentNode == null)
            return;

        // если нет выборов → клик идёт дальше
        if (currentNode.choices == null || currentNode.choices.Count == 0)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                NextDialogue();
            }
        }
    }

    // =========================
    // SHOW NODE
    // =========================

    public void ShowNode(string nodeID)
    {
        ClearChoices();

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
        {
            Debug.LogError("Нода не найдена: " + nodeID);
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
                    Debug.Log("Нет нужного флага: " + flag);
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
            backgroundManager.ChangeBackground(currentNode.background);
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
            foreach (var characterName in currentNode.hideCharacters)
            {
                characterManager.HideCharacter(characterName);
            }
        }

        // =========================
        // TEXT
        // =========================

        nameText.text = currentNode.speaker;
        dialogueText.text = currentNode.text;

        // =========================
        // ACTIVE SPEAKER
        // =========================

        characterManager.SetSpeaker(currentNode.speaker);

        // =========================
        // CHOICES
        // =========================

        if (currentNode.choices != null &&
            currentNode.choices.Count > 0)
        {
            foreach (DialogueChoice choice in currentNode.choices)
            {
                GameObject buttonObj =
                    Instantiate(choicePrefab, choicesContainer);

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
    // NEXT DIALOGUE
    // =========================

    void NextDialogue()
    {
        if (!string.IsNullOrEmpty(currentNode.nextNodeID))
        {
            ShowNode(currentNode.nextNodeID);
        }
        else
        {
            Debug.Log("Конец диалога");

            ClearChoices();
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

        if (relationshipSystem.sadako < choice.requiredSadako)
            passed = false;

        if (relationshipSystem.sumiko < choice.requiredSumiko)
            passed = false;

        if (relationshipSystem.teruko < choice.requiredTeruko)
            passed = false;

        string nextID = choice.nextNodeID;

        // если провалили проверку
        if (!passed && !string.IsNullOrEmpty(choice.failNodeID))
        {
            nextID = choice.failNodeID;
        }

        if (string.IsNullOrEmpty(nextID))
        {
            Debug.LogError("Нет следующей ноды!");
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
}