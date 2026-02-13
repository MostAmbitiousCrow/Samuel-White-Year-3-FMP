using UnityEngine;
using EditorAttributes;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class Tsunami_Controller : MonoBehaviour, IAffectedByRiver
{
    [Header("Data")]
    [Tooltip("Can the Tsunami progress towards the player?")]
    [SerializeField] bool _canProgress = true;
    /// <summary> Indicates whether the Tsunami is currently losing progress </summary>
    public bool IsReversing { get; private set; }
    /// <summary> Indicates whether the Tsunami has made no progress </summary>
    public bool IsIdle { get; private set; }
    /// <summary> Check for if the Tsunami has passed the player and has essentially ended the game </summary>
    public bool HasReachedPlayer { get { return _hasReachedPlayer; } }
    public bool HasReachedDangerMark { get { return _hasReachedDangerMark; } }

    [Space]
    [Tooltip("Multiplier for the speed of the Tsunami")]
    [SerializeField] float _progressSpeedMultiplier = .01f;
    [Tooltip("The current speed of the Tsunami, based on the speed of the River")]
    [SerializeField, ReadOnly] int _progressSpeed = 0;
    [Tooltip("The speed of the river until the Tsunami begins to catchup with the player")]
    [SerializeField] int _speedUntilProgress = 3;

    [Space]

    [Tooltip("The percentage quota of the tsunami meter until activating the danger mark")]
    [SerializeField, Range(0f, 1f)] float _dangerMark = 95f;
    [Tooltip("The multiplier applied to the river progress upon reaching the danger mark")]
    [SerializeField, Range(0f, 1f)] float _dangerMarkSpeedDrop = .6f;

    [Header("Status")]
    [SerializeField, ReadOnly] bool _hasReachedPlayer;
    [SerializeField, ReadOnly] bool _hasReachedDangerMark;
    [SerializeField, ProgressBar(1f), Range(0f, 1f)] float _visualProgress = 0f;
    [SerializeField, ProgressBar(1f), Range(0f, 1f)] float _actualProgress = 0f;
    public float ActualProgress { get { return _actualProgress; } }

    [Header("Components")]
    //[SerializeField] Transform _shadow; // TODO: The shadow that looms over the camera (since the game doesn't use any lighting)
    [SerializeField] ParticleSystem _splashParticles;
    [SerializeField] AudioSource _audio;
    [Space]
    [SerializeField] Tsunami_Shadow_Controller _tsunamiShadowController;
    [SerializeField] River_Manager _riverManager;

    #region Initialisers
    private void Awake()
    {
        if (!_audio) _audio = GetComponent<AudioSource>();
        if (!_riverManager) _riverManager = FindObjectOfType<River_Manager>();
        //GameManager.GameLogic.onGameStarted += StartProgressing;
        //_riverManager.OnRiverSpeedUpdate += OnRiverUpdated;
    }
    private void OnEnable()
    {
        GameManager.GameLogic.onGameStarted += StartProgressing;
        _riverManager.OnRiverSpeedUpdate += OnRiverUpdated;
    }
    private void OnDisable()
    {
        GameManager.GameLogic.onGameStarted -= StartProgressing;
        _riverManager.OnRiverSpeedUpdate -= OnRiverUpdated;
    }
    #endregion
    private void Update()
    {
        if (!_canProgress) return;

        UpdateProgress();
    }

    #region Injection
    public void InjectRiverManager(River_Manager manager) => _riverManager = manager;
    #endregion

    #region Controls
    void StartProgressing()
    {
        _canProgress = true;
        ResetProgress();
    }

    public void Pause()
    {
        _canProgress = false;
        UpdateProgressElements();
    }

    public void Resume()
    {
        _canProgress = true;
        UpdateProgressElements();
    }
    #endregion

    #region Progress Methods

    /*
    IEnumerator ReverseProgress(float time, bool bypassDangerMark)
    {
        float t = 0f;
        IsReversing = true;


        while (t < time)
        {
            if (bypassDangerMark)
                _actualProgress -= Time.deltaTime * _progressSpeedMultiplier * GameManager.GameLogic.GamePauseInt;
            else
            {
                if (CheckDangerMark()) _actualProgress -= Time.deltaTime * _progressSpeed * _dangerMarkSpeedDrop * GameManager.GameLogic.GamePauseInt;
                else _actualProgress -= Time.deltaTime * _progressSpeed * GameManager.GameLogic.GamePauseInt;
            }

        }

        IsReversing = false;

        yield break;
    }
    */

    // Called whenever the River Managers speed value is updated
    void OnRiverUpdated()
    {
        RecalculateProgression();
    }

    void RecalculateProgression() //TODO
    {
        if (_riverManager.currentRiverSpeed <= _speedUntilProgress) IsReversing = false;
        else IsReversing = true;

        _progressSpeed = (_riverManager.minMaxSpeed.y - _speedUntilProgress) - _riverManager.currentRiverSpeed;

    }

    void UpdateProgress()
    {
        if(HasReachedPlayer)
        {
            MoveTsunami();
            return;
        }

        (_actualProgress, _visualProgress) = CalculateProgress();
        UpdateProgressElements();
        if (_hasReachedDangerMark) UpdateShadow();
        else _decalProjector.pivot = new(0f, 0f, _shadowMinMaxOffset.x);
    }

    void ResetProgress()
    {
        _visualProgress = 0f;
        _actualProgress = 0f;
        // TODO: Update bool checks
        UpdateProgressElements();
    }

    void UpdateProgressElements() => Game_UI.Instance.UpdateTsunamiMeter(_visualProgress);

    (float, float) CalculateProgress() // TODO: polish calculation
    {
        // Calculate actual progress
        float a = _actualProgress + Time.deltaTime * _progressSpeed *
            (CheckDangerMark()? _dangerMarkSpeedDrop : 1f) 
            * GameManager.GameLogic.GamePauseInt * _progressSpeedMultiplier;

        // Limit progress to 1f
        a = Mathf.Clamp(a, 0f, 1f);

        // Actual Progress rounded for visual simplicity
        float v = Mathf.Round(a * 1000f) / 1000f;

        if (a == 1f) _hasReachedPlayer = true;
        else _hasReachedPlayer = false;
        if (a == 0f) return (0f, 0f);
        else return (a, v);
    }

    bool CheckDangerMark()
    {
        bool b = _visualProgress > _dangerMark;
        _hasReachedDangerMark = b;
        return b;
    }

    #endregion

    #region Visual Methods

    [Header("Shadow Controls")]
    [SerializeField] DecalProjector _decalProjector;
    [SerializeField, MinMaxSlider(-10f, 0f)] Vector2 _shadowMinMaxOffset;

    void UpdateShadow()
    {
        float l = Mathf.InverseLerp(_dangerMark, 1f, _visualProgress);
        _decalProjector.pivot = new(0f, 0f, Mathf.Lerp(_shadowMinMaxOffset.x,
            _shadowMinMaxOffset.y, l));

        //TODO Apply screen shake
    }

    [Header("Tsunami Animation")]
    [SerializeField] Transform _tsunamiArt;
    //[ReadOnly, MinMaxSlider(0f, 60f)] 
    readonly Vector2 _tsunamiPath = new (0f, 60f);
    private float _tsunamiProgress = 0f;
    void MoveTsunami()
    {
        if (_tsunamiProgress > 1f)
        {
            //TODO: Make the Tsunami End the game upon hitting the player!

            return;
        }

        _tsunamiProgress += Time.deltaTime * .2f * GameManager.GameLogic.GamePauseInt;

        _tsunamiArt.localPosition = new Vector3(0f, 0f, Mathf.Lerp(_tsunamiPath.x, 
            _tsunamiPath.y, _tsunamiProgress));

    }

    #endregion
}
