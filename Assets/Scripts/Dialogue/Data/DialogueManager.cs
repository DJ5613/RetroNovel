using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public GameObject choicePrefab;
    public Transform choicesContainer;

    public DialogueDatabase database;
    public RelationshipSystem relationshipSystem;

    private DialogueNode currentNode;

    void Start()
    {
        ShowNode("start");
    }

    void Update()
    {
        // Если нет выборов — кликом идём дальше
        if (currentNode != null && (currentNode.choices == null || currentNode.choices.Count == 0))
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                NextDialogue();
            }
        }
    }

    public void ShowNode(string nodeID)
    {
        ClearChoices();

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nodeID);
            return;
        }

        nameText.text = currentNode.character;
        dialogueText.text = currentNode.text;

        // Если есть выборы — создаём кнопки
        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            foreach (var choice in currentNode.choices)
            {
                var btn = Instantiate(choicePrefab, choicesContainer);

                btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;

                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnChoiceSelected(choice);
                });
            }
        }
    }

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

    void OnChoiceSelected(DialogueChoice choice)
    {
        // применяем отношения
        relationshipSystem.ApplyChoice(choice);

        bool passed = true;

        if (relationshipSystem.sadako < choice.requiredSadako)
            passed = false;

        if (relationshipSystem.sumiko < choice.requiredSumiko)
            passed = false;

        if (relationshipSystem.teruko < choice.requiredTeruko)
            passed = false;

        string nextID = choice.nextNodeID;

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

    void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}