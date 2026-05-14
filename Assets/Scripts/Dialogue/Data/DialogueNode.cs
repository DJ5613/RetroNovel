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

    // условия отношений
    public int requiredSadako;
    public int requiredSumiko;
    public int requiredTeruko;

    // если проверка не пройдена
    public string failNodeID;
}

[Serializable]
public class DialogueNode
{
    public string id;

    public string speaker;
    public string text;

    public string nextNodeID;

    public List<DialogueChoice> choices;

    // =========================
    // BACKGROUND
    // =========================

    public string background;

    // =========================
    // CHARACTER SYSTEM
    // =========================

    public List<CharacterState> setCharacters;

    public List<CharacterEmotionChange> emotionChanges;

    public List<string> hideCharacters;

    // =========================
    // FLAGS SYSTEM
    // =========================

    // установить флаги
    public List<string> setFlags;

    // требуемые флаги
    public List<string> requiredFlags;
}

[Serializable]
public class CharacterState
{
    public string name;
    public string emotion;
    public string position;
}

[Serializable]
public class CharacterEmotionChange
{
    public string name;
    public string emotion;
}