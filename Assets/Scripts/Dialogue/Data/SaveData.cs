using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // текущая нода
    public string currentNodeID;

    // отношения
    public int sadakoRelationship;
    public int sumikoRelationship;
    public int terukoRelationship;

    // флаги
    public List<string> flags = new List<string>();

    public List<CharacterSaveData> characters =
    new List<CharacterSaveData>();

    public int currentDay;

    public string saveTime;

    public string routeName;
}

[System.Serializable]
public class CharacterSaveData
{
    public string name;
    public string emotion;
    public string position;
}