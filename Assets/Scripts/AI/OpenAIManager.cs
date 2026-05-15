using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

using Newtonsoft.Json.Linq;

public class OpenAIManager : MonoBehaviour
{
    [Header("API")]
    [TextArea]
    public string apiKey;

    private const string API_URL =
        "https://openrouter.ai/api/v1/chat/completions";

    // =========================
    // SEND REQUEST
    // =========================

    public IEnumerator SendRequest(
        string prompt,
        Action<string> onResponse)
    {
        ChatRequest requestData =
            new ChatRequest
            {
                model = "deepseek/deepseek-v4-flash:free",

                messages = new Message[]
{
    new Message
    {
        role = "system",
        content =
"You are a character from a Japanese psychological horror visual novel.\n" +

"Speak naturally like a real human being.\n" +

"The world may sometimes feel slightly unsettling or emotionally tense,\n" +
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

"Stay fully in character.",
    },

    new Message
    {
        role = "user",
        content = prompt
    }
},

                temperature = 0.3f,

                max_tokens = 80
            };

        string json =
            JsonUtility.ToJson(requestData);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest request =
            new UnityWebRequest(
                API_URL,
                "POST");

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json");

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + apiKey);

        request.SetRequestHeader(
    "HTTP-Referer",
    "https://localhost");

        request.SetRequestHeader(
            "X-Title",
            "AI Horror VN");
        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);

            Debug.LogError(
                request.downloadHandler.text);

            yield break;
        }

        string responseJson =
            request.downloadHandler.text;
        string CleanResponse(string text)
        {
            // удаляем переносы
            text = text.Replace("\n", " ");

            // плохие фразы
            string[] badStarts =
            {
        "time:",
        "Time:",
        "Scene:",
        "Emotion:",
        "System:",
        "User:",
        "Assistant:",
        "Character:"
    };

            foreach (string bad in badStarts)
            {
                if (text.StartsWith(bad))
                {
                    int dots =
                        text.IndexOf("...");

                    if (dots != -1)
                    {
                        text =
                            text.Substring(dots + 3);
                    }
                }
            }

            // удаляем meta мусор
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
                int index = text.IndexOf(phrase);

                if (index != -1)
                {
                    text =
                        text.Substring(index + phrase.Length);
                }
            }

            // удаляем кавычки
            text = text.Replace("\"", "");

            // двойные пробелы
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            return text.Trim();
        }
        Debug.Log(responseJson);

        JObject jsonResponse =
    JObject.Parse(responseJson);

        string aiText = "";

        var message =
            jsonResponse["choices"][0]["message"];

        if (message["content"] != null &&
            message["content"].ToString() != "")
        {
            aiText =
                message["content"].ToString();
        }
        else if (message["reasoning"] != null)
        {
            aiText =
                message["reasoning"].ToString();
        }

        Debug.Log("AI TEXT: " + aiText);

        onResponse?.Invoke(aiText);
    }
}