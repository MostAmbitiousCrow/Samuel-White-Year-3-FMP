using UnityEngine;
using EditorAttributes;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class TsunamiController : MonoBehaviour, IAffectedByRiver
{
    [Header("Data")]
    [Tooltip("Can the Tsunami progress towards the player?")]
    [SerializeField] bool canProgress = true;
    /// <summary> Indicates whether the Tsunami is currently losing progress </summary>
    public bool IsReversing { get; private set; }
    /// <summary> Indicates whether the Tsunami has made no progress </summary>
    public bool IsIdle { get; private set; }
    /// <summary> Check for if the Tsunami has passed the player and has essentially ended the game </summary>
    public bool HasReachedPlayer => hasReachedPlayer;
    public bool HasReachedDangerMark => hasReachedDangerMark;

    [Space]
    [Tooltip("Multiplier for the speed of the Tsunami")]
    [SerializeField] float progressSpeedMultiplier = .01f;
    [Tooltip("The current speed of the Tsunami, based on the speed of the River")]
    [SerializeField, ReadOnly] float progressSpeed = 0;
    [Tooltip("The speed of the river until the Tsunami begins to catchup with the player")]
    [SerializeField] int speedUntilProgress = 3;

    [Space]

    [Tooltip("The percentage quota of the tsunami meter until activating the danger mark")]
    [SerializeField, Range(0f, 1f)] float dangerMark = 95f;
    [Tooltip("The multiplier applied to the river progress upon reaching the danger mark")]
    [SerializeField, Range(0f, 1f)] float dangerMarkSpeedDrop = .6f;

    [Header("Status")]
    [SerializeField, ReadOnly] bool hasReachedPlayer;
    [SerializeField, ReadOnly] bool hasReachedDangerMark;
    [SerializeField, ProgressBar(1f), Range(0f, 1f)] float visualProgress = 0f;
    [SerializeField, ProgressBar(1f), Range(0f, 1f)] float actualProgress = 0f;
    public float ActualProgress => actualProgress;

    [Header("Components")]
    //[SerializeField] Transform _shadow; // TODO: The shadow that looms over the camera (since the game doesn't use any lighting)
    [SerializeField] ParticleSystem splashParticles;
    [SerializeField] AudioSource audio;
    [Space]
    [SerializeField] Tsunami_Shadow_Controller tsunamiShadowController;
    [SerializeField] River_Manager riverManager;

    #region Initialisers
    private void Awake()
    {
        if (!audio) audio = GetComponent<AudioSource>();
        if (!riverManager) riverManager = FindFirstObjectByType<River_Manager>();
        //GameManager.GameLogic.onGameStarted += StartProgressing;
        //_riverManager.OnRiverSpeedUpdate += OnRiverUpdated;
    }
    private void OnEnable()
    {
        GameManager.GameLogic.OnGameStarted += StartProgressing;
        riverManager.OnRiverSpeedUpdate += OnRiverUpdated;
    }
    private void OnDisable()
    {
        GameManager.GameLogic.OnGameStarted -= StartProgressing;
        riverManager.OnRiverSpeedUpdate -= OnRiverUpdated;
    }
    #endregion
    private void Update()
    {
        if (!canProgress) return;

        UpdateProgress();
    }

    #region Injection
    public void InjectRiverManager(River_Manager manager) => riverManager = manager;
    #endregion

    #region Controls

    private void StartProgressing()
    {
        canProgress = true;
        ResetProgress();
    }

    public void Pause()
    {
        canProgress = false;
        UpdateProgressElements();
    }

    public void Resume()
    {
        canProgress = true;
        UpdateProgressElements();
    }
    #endregion

    #region Progress Methods
    // Called whenever the River Managers speed value is updated
    private void OnRiverUpdated()
    {
        RecalculateProgression();
    }

    private void RecalculateProgression() // TODO
    {
        if (riverManager.currentRiverSpeed <= speedUntilProgress) IsReversing = false;
        else IsReversing = true;

        progressSpeed = (riverManager.minMaxSpeed.y - speedUntilProgress) - riverManager.riverFlowSpeed;

    }

    private void UpdateProgress()
    {
        if(HasReachedPlayer)
        {
            MoveTsunami();
            return;
        }

        (actualProgress, visualProgress) = CalculateProgress();
        UpdateProgressElements();
        if (hasReachedDangerMark) UpdateShadow();
        else decalProjector.pivot = new Vector3(0f, 0f, shadowMinMaxOffset.x);
    }

    private void ResetProgress()
    {
        visualProgress = 0f;
        actualProgress = 0f;
        // TODO: Update bool checks
        UpdateProgressElements();
    }

    private void UpdateProgressElements() => Game_UI.Instance.UpdateTsunamiMeter(visualProgress);

    private (float, float) CalculateProgress() // TODO: polish calculation
    {
        // Calculate actual progress
        var a = actualProgress + Time.deltaTime * progressSpeed *
            (CheckDangerMark()? dangerMarkSpeedDrop : 1f) 
            * GameManager.GameLogic.GamePauseInt * progressSpeedMultiplier;

        // Limit progress to 1f
        a = Mathf.Clamp(a, 0f, 1f);

        // Actual Progress rounded for visual simplicity
        var v = Mathf.Round(a * 1000f) / 1000f;

        // Check if the player distance has been reached by the Tsunami
        hasReachedPlayer = Mathf.Approximately(a, 1f);
        return a == 0f ? (0f, 0f) : (a, v);
    }

    private bool CheckDangerMark()
    {
        bool b = visualProgress > dangerMark;
        hasReachedDangerMark = b;
        return b;
    }

    #endregion

    #region Visual Methods

    [Header("Shadow Controls")]
    [SerializeField] DecalProjector decalProjector;
    [SerializeField, MinMaxSlider(-10f, 0f)] Vector2 shadowMinMaxOffset;

    private void UpdateShadow()
    {
        float l = Mathf.InverseLerp(dangerMark, 1f, visualProgress);
        decalProjector.pivot = new(0f, 0f, Mathf.Lerp(shadowMinMaxOffset.x,
            shadowMinMaxOffset.y, l));

        //TODO Apply screen shake
    }

    [Header("Tsunami Animation")]
    [SerializeField] Transform tsunamiArt;
    //[ReadOnly, MinMaxSlider(0f, 60f)] 
    readonly Vector2 _tsunamiPath = new (0f, 60f);
    private float _tsunamiProgress = 0f;

    private void MoveTsunami()
    {
        if (_tsunamiProgress > 1f)
        {
            //TODO: Make the Tsunami End the game upon hitting the player!

            return;
        }

        _tsunamiProgress += Time.deltaTime * .2f * GameManager.GameLogic.GamePauseInt;

        tsunamiArt.localPosition = new Vector3(0f, 0f, Mathf.Lerp(_tsunamiPath.x, _tsunamiPath.y, _tsunamiProgress));
    }
    #endregion
}
