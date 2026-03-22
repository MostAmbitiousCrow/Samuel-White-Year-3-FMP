using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource MusicSource { get; private set; }
    public static MusicManager Instance;

    [SerializeField] private AudioClip[] sewerMusic = new AudioClip[4];
    
    [Header("Dependencies")]
    private GameLevelManager _gameLevelManager;

    private void Awake()
    {
        MusicSource = GetComponent<AudioSource>();
        _gameLevelManager = FindFirstObjectByType<GameLevelManager>();
        
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

    public void PlayMusic()
    {
        if (_gameLevelManager.GameCompleted) return;
        var data = _gameLevelManager.Levels[_gameLevelManager.CurrentLevel].environmentType;
        var audioClip = sewerMusic[(int)data];
        MusicSource.clip = audioClip;
        Debug.Log($"Playing Music: {audioClip} From the {data} Level");
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
