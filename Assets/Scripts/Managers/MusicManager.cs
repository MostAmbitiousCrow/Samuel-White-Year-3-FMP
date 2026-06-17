using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource MusicSource { get; private set; }
    public static MusicManager Instance;

    [SerializeField] private AudioClip[] sewerMusic = new AudioClip[4];
    
    // [Header("Dependencies")]
    // private GameLevelManager _gameLevelManager;

    private void Awake()
    {
        MusicSource = GetComponent<AudioSource>();
        
        Instance = this;
    }

    private void OnEnable()
    {
        GameLevelManager.OnLevelLoaded += PlayMusic;
    }

    private void OnDisable()
    {
        GameLevelManager.OnLevelLoaded -= PlayMusic;
    }
    
    private Environments _lastEnvironment;

    public void PlayMusic()
    {
        // if (GameLevelManager.Instance.GameCompleted) return;
        var data = GameLevelManager.CurrentEnvironment;
        
        // Skip reseting the same music
        if (_lastEnvironment == data) return;
        _lastEnvironment = data;
        var audioClip = sewerMusic[(int)data];
        MusicSource.clip = audioClip;
        // Debug.Log($"Playing Music: {audioClip} From the {data} Level");
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
