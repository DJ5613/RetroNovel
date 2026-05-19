using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
    public AIConversationManager aiConversationManager;
    public FreeTimeManager freeTimeManager;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    private DialogueNode currentNode;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private string currentSpeakerVisual = "";

    private int nodeRedirectDepth = 0;
    private const int MAX_REDIRECT_DEPTH = 10;


    // AI MESSAGE
    private bool aiMessageActive = false;

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
        aiMessageActive = false;
        Debug.Log("SHOW NODE: " + nodeID);

        nodeRedirectDepth++;

        if (nodeRedirectDepth >
            MAX_REDIRECT_DEPTH)
        {
            Debug.LogError(
                "Infinite node redirect detected!");

            return;
        }

        ClearChoices();

        currentNode = database.GetNode(nodeID);


        aiConversationManager.CloseAIChat();

        if (currentNode == null)
        {
            Debug.LogError(
                "Нода не найдена: " + nodeID);

            return;
        }

        // =========================
        // NODE CONDITIONS
        // =========================

        if (!ConditionChecker.CanEnterNode(
            currentNode,
            relationshipSystem))
        {
            Debug.Log(
                "Node conditions failed: " +
                currentNode.id);

            // fail node

            if (!string.IsNullOrEmpty(
                currentNode.failNodeID))
            {
                ShowNode(
                    currentNode.failNodeID);
            }

            return;
        }

        if (nodeID.Contains("_ai"))
        {
            string returnNode =
                currentNode.nextNodeID;

            aiConversationManager.OpenAIChat(
                returnNode);
        }

        // AI режим выключается
        aiMessageActive = false;


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
        // CLEAR CHARACTERS
        // =========================

        if (currentNode.clearCharacters)
        {
            characterManager.HideAll();
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
        // =========================
        // ACTIVE SPEAKER
        // =========================

        if (currentSpeakerVisual != currentNode.speaker)
        {
            currentSpeakerVisual =
                currentNode.speaker;

            if (characterManager.HasCharacter(currentNode.speaker))
            {
                characterManager.SetSpeaker(
                    currentNode.speaker);
            }
            else
            {
                characterManager.ClearSpeaker();
            }
        }

        // =========================
        // CHOICES
        // =========================

        if (currentNode.choices != null &&


            currentNode.choices.Count > 0)
        {
            foreach (DialogueChoice choice in currentNode.choices)
            {
                bool available =
    ConditionChecker.IsChoiceAvailable(
        choice,
        relationshipSystem);

                // скрыть choice

                if (!available &&
                    choice.hideIfLocked)
                {
                    continue;
                }

                GameObject buttonObj =
                    Instantiate(
                        choicePrefab,
                        choicesContainer);

                Button button =
                    buttonObj.GetComponent<Button>();

                buttonObj
                    .GetComponentInChildren<TextMeshProUGUI>()
                    .text = choice.text;

                // disabled state

                if (!available)
                {
                    button.interactable = false;
                }
                else
                {
                    button.onClick.AddListener(() =>
                    {
                        OnChoiceSelected(choice);
                    });
                }
            }
        }

        if (!string.IsNullOrEmpty(currentNode.music))
        {
            MusicManager.Instance.PlayMusic(
                currentNode.music);
        }

        nodeRedirectDepth = 0;
    }

    // =========================
    // CONTINUE DIALOGUE
    // =========================

    public void ContinueDialogue()
    {
        // AI сообщение активно
        if (aiMessageActive)
        {
            return;
        }

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

        if (currentNode.id == "free_time_start")
        {
            freeTimeManager
                .OpenFreeTime();

            return;
        }

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
        

        // =========================
        // SET FLAGS
        // =========================

        if (choice.setFlags != null)
        {
            foreach (string flag in choice.setFlags)
            {
                GameFlags.Instance.SetFlag(flag);
            }
        }

        bool passed =
     ConditionChecker.IsChoiceAvailable(
         choice,
         relationshipSystem);

        string nextID =
            choice.nextNodeID;

        // если не прошёл проверку
        if (!passed &&
            !string.IsNullOrEmpty(choice.failNodeID))
        {
            nextID = choice.failNodeID;
        }

        if (passed)
        {
            relationshipSystem.ApplyChoice(choice);
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
    // SHOW AI MESSAGE
    // =========================

    public void ShowAIMessage(
    string speaker,
    string message)
    {

        // стопаем старую печать
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isTyping = false;

        // AI режим
        aiMessageActive = true;

        // speaker
        nameText.text = speaker;

        // portrait focus
        if (characterManager.HasCharacter(speaker))
        {
            characterManager.SetSpeaker(
                speaker);
        }

        // TYPEWRITER
        typingCoroutine =
            StartCoroutine(
                TypeText(message));
    }


    public void ApplyAIEmotion(
    string speaker,
    string emotion)
    {
        if (characterManager.HasCharacter(speaker))
        {
            characterManager.ChangeEmotion(
                speaker,
                emotion);
        }
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