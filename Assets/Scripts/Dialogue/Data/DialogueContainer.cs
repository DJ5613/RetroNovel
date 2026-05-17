using System;
using System.Collections.Generic;

[Serializable]
public class DialogueContainer
{
    public List<GraphComment> comments;

    public List<DialogueNode> nodes =
        new List<DialogueNode>();
}