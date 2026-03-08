using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource MusicSource { get; private set; }
    public static MusicManager Instance;

    private void Awake()
    {
        MusicSource = GetComponent<AudioSource>();
        
        Instance = this;
    }

    public void PlayMusic()
    {
        MusicSource.Play();
    }

    public void PauseMusic()
    {
        MusicSource.Pause();
    }

    public void ResumeMusic()
    {
        MusicSource.UnPause();
    }

    public void StopMusic()
    {
        MusicSource.Stop();
    }
}
