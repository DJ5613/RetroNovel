using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Day System")]
    public int currentDay = 1;

    [Header("Route Flags")]
    public bool sadakoRoute;
    public bool sumikoRoute;
    public bool terukoRoute;

    [Header("World Flags")]
    public bool cassetteFound;
    public bool worldBroken;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextDay()
    {
        currentDay++;

        Debug.Log("Наступил день: " + currentDay);
    }
}