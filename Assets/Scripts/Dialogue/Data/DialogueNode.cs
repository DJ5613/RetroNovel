using System;
using System.Collections.Generic;

[Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeID;

    // изменение отношений
    public int sadakoChange;
    public int sumikoChange;
    public int terukoChange;

    // условия
    public int requiredSadako;
    public int requiredSumiko;
    public int requiredTeruko;

    // если не прошёл проверку
    public string failNodeID;
}


[Serializable]
public class DialogueNode
{
    public string id;
    public string character;
    public string emotion;
    public string position;
    public bool hideCharacter;
    public string text;

    public string nextNodeID;
    public List<DialogueChoice> choices;
}