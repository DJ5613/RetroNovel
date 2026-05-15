using System.Collections.Generic;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    private Dictionary<string, DialogueNode> nodeDict;

    void Awake()
    {
        LoadFromJSON();
    }

    void LoadFromJSON()
    {
        nodeDict =
            new Dictionary<string, DialogueNode>();

        // =========================
        // LOAD ALL JSON FILES
        // =========================

        TextAsset[] jsonFiles =
            Resources.LoadAll<TextAsset>(
                "Dialogues");

        // =========================
        // PARSE FILES
        // =========================

        foreach (TextAsset jsonFile in jsonFiles)
        {
            Debug.Log(
                "Loading dialogue file: " +
                jsonFile.name);

            DialogueContainer container =
                JsonUtility.FromJson<DialogueContainer>(
                    jsonFile.text);

            // safety
            if (container == null ||
                container.nodes == null)
            {
                Debug.LogWarning(
                    "Broken JSON: " +
                    jsonFile.name);

                continue;
            }

            // =========================
            // ADD NODES
            // =========================

            foreach (DialogueNode node in container.nodes)
            {
                // duplicate protection
                if (nodeDict.ContainsKey(node.id))
                {
                    Debug.LogWarning(
                        "Duplicate node id: " +
                        node.id);

                    continue;
                }

                nodeDict[node.id] = node;
            }
        }

        Debug.Log(
            "Loaded dialogue nodes: " +
            nodeDict.Count);
    }

    // =========================
    // GET NODE
    // =========================

    public DialogueNode GetNode(string id)
    {
        if (nodeDict.ContainsKey(id))
        {
            return nodeDict[id];
        }

        Debug.LogWarning(
            "Node not found: " + id);

        return null;
    }
}