using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern
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

    // Play background music
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        //setup volume
        musicSource.volume = 0.75f; // Set volume to 50% (adjust as needed)
        musicSource.Play();
    }

    // Play a sound effect
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // Stop music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Stop all SFX
    public void StopSFX()
    {
        sfxSource.Stop();
    }
}
