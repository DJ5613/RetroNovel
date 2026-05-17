using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    public AudioSource musicSource;

    private string currentMusic = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // PLAY MUSIC
    // =========================

    public void PlayMusic(string musicName)
    {
        // already playing

        if (currentMusic == musicName)
            return;

        // load from Resources/Music/

        AudioClip clip =
            Resources.Load<AudioClip>(
                "Music/" + musicName);

        // not found

        if (clip == null)
        {
            Debug.LogWarning(
                "Music not found: " +
                musicName);

            return;
        }

        currentMusic = musicName;

        musicSource.clip = clip;

        musicSource.Play();
    }

    // =========================
    // STOP MUSIC
    // =========================

    public void StopMusic()
    {
        musicSource.Stop();

        currentMusic = "";
    }
}