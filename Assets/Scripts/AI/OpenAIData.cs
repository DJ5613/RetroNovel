using System;

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatRequest
{
    public bool stream;

    public string model;

    public Message[] messages;

    public float temperature;

    public int max_tokens;
}

[Serializable]
public class ChatChoice
{
    public Message message;
}

[Serializable]
public class ChatResponse
{
    public ChatChoice[] choices;
}