using UnityEngine;
using TMPro;

public class RelationshipSystem : MonoBehaviour
{
    public int sadako = 0;
    public int sumiko = 0;
    public int teruko = 0;

    public bool debugMode = true;
    public TextMeshProUGUI sadakoText;
    public TextMeshProUGUI sumikoText;
    public TextMeshProUGUI terukoText;

    void Start()
    {
        UpdateUI();
    }

    public void ApplyChoice(DialogueChoice choice)
    {
        sadako += choice.sadakoChange;
        sumiko += choice.sumikoChange;
        teruko += choice.terukoChange;

        Debug.Log($"Sadako: {sadako}, Sumiko: {sumiko}, Teruko: {teruko}");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (!debugMode)
        {
            sadakoText.gameObject.SetActive(false);
            sumikoText.gameObject.SetActive(false);
            terukoText.gameObject.SetActive(false);
            return;
        }

        sadakoText.gameObject.SetActive(true);
        sumikoText.gameObject.SetActive(true);
        terukoText.gameObject.SetActive(true);

        sadakoText.text = "Садако: " + sadako;
        sumikoText.text = "Сумико: " + sumiko;
        terukoText.text = "Тэруко: " + teruko;
    }

    void Update()
    {
        if (debugMode)
        {
            UpdateUI();
        }
    }

}