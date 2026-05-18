using System;
using System.Collections.Generic;

[Serializable]
public class EventData
{
    public int day;

    public List<string> requiredFlags;

    public string nodeID;

    public int priority;
}