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
        TextAsset json = Resources.Load<TextAsset>("dialogue");

        DialogueContainer container = JsonUtility.FromJson<DialogueContainer>(json.text);

        nodeDict = new Dictionary<string, DialogueNode>();

        foreach (var node in container.nodes)
        {
            nodeDict[node.id] = node;
        }
    }

    public DialogueNode GetNode(string id)
    {
        return nodeDict.ContainsKey(id) ? nodeDict[id] : null;
    }
}