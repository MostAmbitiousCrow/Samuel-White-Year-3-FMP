using System;
using CameraShake;
using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(AudioSource))]
public class TsunamiController : MonoBehaviour
{
    #region Variables
    
    [Header("Data")]
    [Space]
    [Tooltip("Multiplier for the speed of the Tsunami")]
    [SerializeField] private float progressSpeedMultiplier = .01f;
    [Tooltip("The current speed of the Tsunami, based on the speed of the River")]
    [SerializeField, ReadOnly] private float progressSpeed = 0;
    [Tooltip("The speed of the river until the Tsunami begins to catchup with the player")]
    [SerializeField] private int speedUntilProgress = 3;
    [Tooltip("The speed of the smoothness for progression. Affects variables such as Audio.")]
    [SerializeField] private float smoothSpeed = .1f;
    
    [Header("Tsunami Effects")]
    [SerializeField] private PerlinShake.Params shakeParams;
    private PerlinShake _shake;
    [Space]
    [SerializeField] private Material waterMaterial;
    [SerializeField] private TsunamiStepEffects[] stepEffects;
    [Serializable]
    public struct TsunamiStepEffects
    {
        [Tooltip("The value representing the step before the maximum steps of which this effect will be applied")]
        public int stepBefore;
        [Tooltip("How much amplitude with the camera shake be during this step")]
        public float shakeAmplitude;
        [Tooltip("How fast will the Sewer River move during this step")]
        public float waveSpeed;
    }

    [Header("Status")]
    [Tooltip("Can the Tsunami progress towards the boat?")]
    [SerializeField] private bool canProgress = true;
    [Tooltip("Check for if the Tsunami has reached the boat and washed over it")]
    [SerializeField, ReadOnly] private bool hasReachedBoat;
    [Tooltip("Check for whether the boat is one step from reaching the boat")]
    [SerializeField, ReadOnly] private bool hasReachedDangerMark;
    public bool HasReachedBoat => hasReachedBoat;
    public bool HasReachedDangerMark => hasReachedDangerMark;
    [Tooltip("A visual progress bar visualising the Tsunamis distance from the boat")]
    [SerializeField, ProgressBar(1f), Range(0f, 1f)]
    private float visualProgress;
    [Tooltip("The current step from its distance to the boat")]
    [SerializeField, ReadOnly] private int currentStep;
    [Tooltip("The maximum amount of steps the Tsunami can take before reaching the boat")]
    [SerializeField] private int maximumSteps = 4;

    [Header("Components")]
    //[SerializeField] Transform _shadow; // TODO: The shadow that looms over the camera (since the game doesn't use any lighting)
    [SerializeField] private GameObject artwork;
    [SerializeField] private RiverSplineObject riverSplineObject;
    [SerializeField] private ParticleSystem[] particles;
    [SerializeField] private AudioSource ambienceAudio;
    [SerializeField] private BoxCollider smashBox;
    [Space]
    [SerializeField]
    private Tsunami_Shadow_Controller tsunamiShadowController;
    [SerializeField] private River_Manager riverManager;
    [SerializeField] private Boat_Controller boatController;
    
    // Events
    public static event Action<int> OnTsunamiUpdated;
    #endregion

    #region Initialisers
    private void Awake()
    {
        if (!ambienceAudio) ambienceAudio = GetComponent<AudioSource>();
        if (!riverManager) riverManager = FindFirstObjectByType<River_Manager>();
        if (!boatController) boatController = FindFirstObjectByType<Boat_Controller>();
        if (!smashBox) smashBox = GetComponent<BoxCollider>();
        
        _onRegress = () => Regress(1);
    }

    private void Start()
    {
        // Setup Tsunami Camera Shake Control
        shakeParams.envelope.sustain = 999999f;
        _shake = new PerlinShake(shakeParams);
        CameraShaker.Shake(_shake);
        shakeParams.noiseModes[0].amplitude = 0f;
        
        ResetProgress();
    }

    private Action _onRegress;

    private void OnEnable()
    {
        GameManager.GameLogic.OnGameStarted += EnableControl;
        GameManager.MainGameLogic.OnGameOver += DisableControl;
        GameLevelManager.OnLevelLoaded += _onRegress;
        // SewerEnvironmentArtManager.OnEnvironmentUpdated += ResetProgress;
        
    }
    private void OnDisable()
    {
        GameManager.GameLogic.OnGameStarted -= EnableControl;
        GameManager.MainGameLogic.OnGameOver -= DisableControl;
        GameLevelManager.OnLevelLoaded -= _onRegress;
        // SewerEnvironmentArtManager.OnEnvironmentUpdated -= ResetProgress;
    }
    #endregion
    private void Update()
    {
        if (!canProgress) return;

        ProgressTsunami();
    }

    #region Controls

    public void DisableControl(GameManager.MainGameLogic.GameOverType gameOverType)
    {
        if (gameOverType != GameManager.MainGameLogic.GameOverType.Tsunami) canProgress = false;
    }

    public void EnableControl()
    {
        canProgress = true;
    }
    
    /// <summary> Resets the Tsunamis progress towards the boat </summary>
    public void ResetProgress()
    {
        currentStep = 0;
        artwork.SetActive(false);
        smashBox.enabled = false;
        shakeParams.noiseModes[0].amplitude = 0f;
        
        UpdateTsunamiProgress();
        // foreach (var particle in particles) particle.Stop();
    }

    /// <summary> Sends the Tsunami forward towards the boat under a given amount of steps </summary>
    /// <param name="steps"> The amount of steps to progress the Tsunami</param>
    public void Progress(int steps = 1)
    {
        currentStep += steps;
        UpdateTsunamiProgress();
    }
    
    /// <summary> Sends the Tsunami backwards from the boat under a given amount of steps </summary>
    /// <param name="steps"> The amount of steps to regress the Tsunami </param>
    public void Regress(int steps = 1)
    {
        // This is checking the Danger Mark step effects
        if (currentStep >= maximumSteps + 1) currentStep = maximumSteps - steps;
        else currentStep -= steps;
        UpdateTsunamiProgress();
    }

    private void UpdateTsunamiProgress()
    {
        // Check if the current step has already reached the maximum steps requirement
        if (currentStep >= maximumSteps)
        {
            // If so, set the current step as the maximum step.
            // If the Danger Mark is true, trigger the wash. Otherwise, set the Danger Mark as true
            // currentStep = maximumSteps;
            if (hasReachedDangerMark) TriggerWash();
            else hasReachedDangerMark = true;
        }
        // Otherwise, uncheck the Danger Mark
        else
        {
            hasReachedDangerMark = false;
            if (currentStep < 0) currentStep = 0;
        }
        OnTsunamiUpdated?.Invoke(currentStep);
    }

    private float _smooth;
    private float _velocity;
    private bool _isDropping;

    private void ProgressTsunami()
    {
        // Smooth progress update based on the current step compared to the maximum steps.
        var lerp = Mathf.InverseLerp(0, maximumSteps, currentStep);
        _smooth = Mathf.SmoothDamp(_smooth, lerp, ref _velocity, Time.deltaTime * smoothSpeed);
        
        visualProgress = _smooth;
        ambienceAudio.volume = _smooth;

        //TODO: Improve:
        //Perhaps base the distance and amplitude distance when dropping based on the
        //players boat distance the the tsunamis distance on the river
        
        // Check if the danger mark has been reached.
        if (hasReachedDangerMark)
        {
            // Drop if the tsunami has already reached the boat
            if (_isDropping)
            {
                // Reduce the amplitude of the tsunami overtime.
                shakeParams.noiseModes[0].amplitude -= Time.deltaTime * 0.05f;
                shakeParams.noiseModes[0].amplitude = Mathf.Clamp(shakeParams.noiseModes[0].amplitude, 0f, 10f);
                // Debug.Log("Tsunami is dropping!");

                // If the amplitude has fully dropped, stop the tsunami.
                if (shakeParams.noiseModes[0].amplitude <= 0f)
                {
                    riverSplineObject.ignorePause = false;
                    canProgress = false;
                    // Debug.Log("Tsunami drop completed");
                }

                return;
            }

            // If the tsunami has reached the boat and is not dropping, start the drop.
            if (hasReachedBoat)
            {
                if (!_isDropping)
                {
                    _isDropping = true;
                    shakeParams.noiseModes[0].amplitude = 0.5f;  // Set a high amplitude when tsunami reaches the boat.
                }
                else
                {
                    shakeParams.noiseModes[0].amplitude = 0.03f;  // Lower amplitude once tsunami starts dropping.
                }
            }
        }
        // Handle cases where the danger mark has not been reached yet.
        else
        {
            // Check if the current step has reached the maximum step requirement.
            if (currentStep >= maximumSteps)
            {
                shakeParams.noiseModes[0].amplitude = 0.02f;  // Lower amplitude once maximum steps are reached.
            }
            // Otherwise, check if the current step is just before the maximum.
            else if (currentStep == maximumSteps - 1)
            {
                shakeParams.noiseModes[0].amplitude = 0.01f;  // Minimal amplitude when almost at max steps.
            }
        }
    }
    #endregion

    #region Events
    /// <summary>The event that causes the Tsunami to speed ahead of the boat and destroy everything in its way</summary>
    private void TriggerWash()
    {
        ambienceAudio.volume = 1f;
        ambienceAudio.Play();
        
        artwork.SetActive(true);
        smashBox.enabled = true;

        riverSplineObject.ignoreRiverSpeed = true;
        riverSplineObject.speedMultiplier = 3.5f;
        riverSplineObject.ignorePause = true;
        hasReachedBoat = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Destroy anything in the Tsunamis path!
        if (!other.TryGetComponent(out IDamageable obj)) return;
        
        obj.TakeDamage(DamageType.Tsunami, 100);
        Debug.Log($"Tsunami destroyed {other.name}");
    }

    #endregion
}
