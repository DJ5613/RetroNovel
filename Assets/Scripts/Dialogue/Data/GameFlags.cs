using System.Collections.Generic;
using UnityEngine;

public class GameFlags : MonoBehaviour
{
    public static GameFlags Instance;

    private HashSet<string> flags = new HashSet<string>();

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

    // =========================
    // SET FLAG
    // =========================

    public void SetFlag(string flag)
    {
        if (!flags.Contains(flag))
        {
            flags.Add(flag);

            Debug.Log("Флаг установлен: " + flag);
        }
    }

    // =========================
    // REMOVE FLAG
    // =========================

    public void RemoveFlag(string flag)
    {
        if (flags.Contains(flag))
        {
            flags.Remove(flag);

            Debug.Log("Флаг удалён: " + flag);
        }
    }

    // =========================
    // CHECK FLAG
    // =========================

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public List<string> GetAllFlags()
    {
        return new List<string>(flags);
    }

    public void LoadFlags(List<string> loadedFlags)
    {
        flags.Clear();

        foreach (string flag in loadedFlags)
        {
            flags.Add(flag);
        }
    }
}