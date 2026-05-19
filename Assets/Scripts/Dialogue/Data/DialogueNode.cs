using System;
using System.Collections.Generic;
using UnityEngine;

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

    // =========================
    // FLAG CONDITIONS
    // =========================

    // обязательные флаги
    public List<string> requiredFlags;

    // запрещённые флаги
    public List<string> blockedFlags;

    // скрывать choice если условия не выполнены
    public bool hideIfLocked;

    // =========================
    // FLAG REWARDS
    // =========================

    // установить флаги после выбора
    public List<string> setFlags;

}

[Serializable]
public class DialogueNode
{
    public string id;

    public Vector2 editorPosition;
    public string speaker;
    public string text;

    public string nextNodeID;

    public List<DialogueChoice> choices;

    public int priority;
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

    public bool clearCharacters;

    // =========================
    // FLAGS SYSTEM
    // =========================

    // установить флаги
    public List<string> setFlags;

    // требуемые флаги
    public List<string> requiredFlags;

    // =========================
    // RELATIONSHIP CONDITIONS
    // =========================

    public int requiredSadako;
    public int requiredSumiko;
    public int requiredTeruko;

    // =========================
    // BLOCKED FLAGS
    // =========================

    public List<string> blockedFlags;

    // =========================
    // FAIL NODE
    // =========================

    public string failNodeID;


    public string music;

   

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