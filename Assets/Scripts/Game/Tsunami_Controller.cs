using UnityEngine;
using EditorAttributes;

public class Tsunami_Controller : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] bool _canProgress = true;
    /// <summary> Can the Tsunami progress towards the player? </summary>
    public bool CanProgress { get { return _canProgress; } }
    [SerializeField, ProgressBar, Range(0f, 100f)] float _progress = 0f;
    [Space]
    [SerializeField] float _speed = 1f;
    public float Speed { get { return _speed; } }
    [Space]
    [SerializeField] AudioSource _audio;

    [Header("Status")]
    [SerializeField, ReadOnly] bool _hasReachedPlayer;

    [Header("Components")]
    [SerializeField] Transform _artTransform;
    [SerializeField] Transform _shadow; // TODO: The shadow that looms over the camera (since the game doesn't use any lighting)

    private void OnEnable()
    {
        GameManager.GameLogic.onGameStarted += StartProgressing;
    }
    private void OnDisable()
    {
        GameManager.GameLogic.onGameStarted += StartProgressing;
    }

    private void Update()
    {
        if (_canProgress)
        {
            Progress();
        }
    }

    #region Controls
    void StartProgressing()
    {
        _canProgress = true;
        _progress = 0f;
    }

    void Pause()
    {

    }

    void Resume()
    {

    }
    #endregion

    void Progress()
    {
        _progress += Time.deltaTime * _speed * GameManager.GameLogic.GamePauseInt;

    }
}
