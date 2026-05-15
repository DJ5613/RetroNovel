using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OpenAIManager : MonoBehaviour
{
    [Header("LOCAL AI")]
    private string apiUrl =
        "http://localhost:11434/api/chat";

    // =========================
    // SEND REQUEST
    // =========================

    public IEnumerator SendRequest(
        string prompt,
        Action<string> onResponse)
    {
        // =========================
        // REQUEST BODY
        // =========================

        var requestData =
            new
            {
                model = "qwen2.5:7b",

                messages = new[]
                {
                    new
                    {
                        role = "system",

                        content =
                        "You are a character from a Japanese psychological horror visual novel.\n" +

                        "Speak naturally like a real human being.\n" +

                        "The atmosphere may sometimes feel emotionally tense or slightly unsettling,\n" +
                        "but the character should still sound believable and grounded.\n" +

                        "Always answer ONLY in Russian.\n" +

                        "Keep responses short.\n" +
                        "Usually 1-3 natural sentences.\n" +

                        "Use casual human dialogue.\n" +
                        "Avoid theatrical horror behavior.\n" +
                        "Avoid exaggerated anime speech.\n" +

                        "Never repeat words or sounds.\n" +
                        "Never output random noises.\n" +
                        "Never mix languages.\n" +
                        "Never output timestamps or metadata.\n" +

                        "Never explain reasoning.\n" +
                        "Never explain thoughts.\n" +
                        "Never analyze the request.\n" +

                        "Output ONLY the final spoken dialogue.\n" +

                        "Stay fully in character."
                    },

                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },

                stream = false,

                options = new
                {
                    temperature = 0.4,
                    num_predict = 40
                }
            };

        // =========================
        // JSON
        // =========================

        string json =
            JsonConvert.SerializeObject(
                requestData);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        // =========================
        // REQUEST
        // =========================

        UnityWebRequest request =
            new UnityWebRequest(
                apiUrl,
                "POST");

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json");

        // =========================
        // SEND
        // =========================

        yield return request.SendWebRequest();

        // =========================
        // ERROR
        // =========================

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);

            Debug.LogError(
                request.downloadHandler.text);

            yield break;
        }

        // =========================
        // RESPONSE JSON
        // =========================

        string responseJson =
            request.downloadHandler.text;

        // =========================
        // PARSE RESPONSE
        // =========================

        JObject jsonResponse =
            JObject.Parse(responseJson);

        string aiText = "";

        if (jsonResponse["message"] != null &&
            jsonResponse["message"]["content"] != null)
        {
            aiText =
                jsonResponse["message"]["content"]
                .ToString();

            // =========================
            // REMOVE THINKING
            // =========================

            if (aiText.Contains("<think>"))
            {
                int endThink =
                    aiText.IndexOf("</think>");

                if (endThink != -1)
                {
                    aiText =
                        aiText.Substring(endThink + 8);
                }
            }

            aiText = aiText.Trim();
        }

        // =========================
        // FALLBACK
        // =========================

        if (string.IsNullOrEmpty(aiText))
        {
            aiText = "...";
        }

        // =========================
        // CLEAN RESPONSE
        // =========================

        aiText =
            CleanResponse(aiText);

        Debug.Log("AI TEXT: " + aiText);

        // =========================
        // CALLBACK
        // =========================

        onResponse?.Invoke(aiText);
    }

    // =========================
    // CLEAN RESPONSE
    // =========================

    string CleanResponse(string text)
    {
        // переносы

        text = text.Replace("\n", " ");

        // formatting

        text = text.Replace("*", "");
        text = text.Replace("#", "");
        text = text.Replace("\"", "");

        // emotion tags

        text = text.Replace("[happy]", "");
        text = text.Replace("[sad]", "");
        text = text.Replace("[nervous]", "");
        text = text.Replace("[angry]", "");
        text = text.Replace("[neutral]", "");

        // relationship tags

        text = text.Replace("[relationship:+1]", "");
        text = text.Replace("[relationship:-1]", "");

        // bad phrases

        string[] badPhrases =
        {
            "The user wants",
            "I need to",
            "I should",
            "Let's",
            "We need",
            "First,",
            "Okay,"
        };

        foreach (string phrase in badPhrases)
        {
            int index =
                text.IndexOf(phrase);

            if (index != -1)
            {
                text =
                    text.Substring(
                        index + phrase.Length);
            }
        }

        // double spaces

        while (text.Contains("  "))
        {
            text =
                text.Replace("  ", " ");
        }

        return text.Trim();
    }
}