using System;
using System.Collections;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class Gemstone_Gate : River_Object, ITargetsBoat
{
    [Header("Components")]
    [Tooltip("The voxels forming the gate that will explode")]
    [SerializeField] Transform[] _gateBlocks;
    [SerializeField] GameObject _art; // TODO: Temporary. Replace once an explosion animation is implemented

    [Header("Stats")]
    /// <summary>
    /// The distance of the gate to the players boat until the gate begins consuming the players gemstones
    /// </summary>
    [SerializeField] float _distanceUntilConsumption = 10f;
    [Space(10)]
    [SerializeField] float _explodeDelay = .45f;
    [SerializeField] float _explodeForce = 1f;
    [SerializeField] float _explodeDuration = 2.5f;
    [SerializeField] GemstoneGateData data;
    [Space(10)]
    [SerializeField] bool _isConsuming;
    [Space(10)]
    [Header("Destroy Phase Stats")]
    [Tooltip("")]
    [SerializeField, MinMaxSlider(1f, 3.5f)] private Vector2 _insufficientGemsPauseDelay;
    [Tooltip("Start positions of the destroy lasers")]
    [SerializeField] Transform[] _laserPositions = new Transform[2];

    [Header("Components")]
    [SerializeField] ParticleSystem _gemstoneParticles;
    [Space(10)]
    [Header("Particle Settings")]
    [Tooltip("The time during the collect particle phase before the particles begin to home in on the target")]
    [SerializeField] float _emmisionRate = .1f;
    [SerializeField] float _emmisionMultiplier = .9f;
    [SerializeField] float _homingDelay = 2f;
    [Tooltip("The strength of the homing collect particles")]
    [SerializeField] float _homingStrength = 1f;
    [SerializeField] float _particleDespawnDistance = .5f;
    [Space]
    [SerializeField] TextMeshPro _gemRequirementText;

    #region Injection
    public Boat_Space_Manager SpaceManager { get; set; }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        SpaceManager = bsm;
    }
    #endregion

    public void OverrideData(GemstoneGateData overridedData)
    {
        data = overridedData;
        print($"{name} stats were overrided");
    }

    #region Override Methods
    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (_isConsuming) return;
        if (SpaceManager.GetDistanceToBoat(_distance) < _distanceUntilConsumption)
        {
            StartCoroutine(ConsumeGemstones());
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        _isMoving = true;

        _art.SetActive(true);

        _gemRequirementText.SetText(data.GemRequirement.ToString());
    }
    #endregion

    #region Consume Gemstones Process
    private ParticleSystem.Particle[] particles;

    IEnumerator ConsumeGemstones() //TODO: WIP
    {
        GameManager.MainGameLogic.PlayerData playerdata = GameManager.GameLogic.playerData;

        if (playerdata.PlayerTransform == null)
        {
            Debug.LogWarning("Missing Player Transform component, Cancelled Gemstone Gate Consumption Event");
            AbsorptionSucceeded(); // Callback
            yield break;
        }

        print($"{name} Started Consumption");
        _isConsuming = true;
        _isMoving = false;

        int savedGemstones = playerdata.CurrentGemstones;

        if (savedGemstones <= 0) AbsorptionFailed();

        _gemstoneParticles.transform.position = playerdata.PlayerTransform.position;

        ParticleSystem.EmissionModule emission = _gemstoneParticles.emission;
        emission.rateOverTime = data.GemRequirement;
        var main = _gemstoneParticles.main;

        ParticleSystem.LimitVelocityOverLifetimeModule limitVelocity = _gemstoneParticles.limitVelocityOverLifetime;

        if (savedGemstones > data.GemRequirement)
            main.maxParticles = Mathf.RoundToInt(data.GemRequirement);
        else
            main.maxParticles = Mathf.RoundToInt(savedGemstones);

        StartCoroutine(EmitParticlesOverTime(main.maxParticles, _emmisionRate, _emmisionMultiplier));

        yield return new WaitUntil(() => _gemstoneParticles.particleCount > 1);

        // Initialize particles array if null or too small
        if (particles == null || particles.Length < main.maxParticles)
            particles = new ParticleSystem.Particle[main.maxParticles];

        int aliveCount = _gemstoneParticles.GetParticles(particles);
        int particlesRemaining = main.maxParticles;
        int amountRemaining = data.GemRequirement;

        while (particlesRemaining > 0)
        {
            aliveCount = _gemstoneParticles.GetParticles(particles); ;

            // Make sure buffer is large enough (If using Emission)
            if (particles == null || particles.Length < _gemstoneParticles.main.maxParticles)
                particles = new ParticleSystem.Particle[_gemstoneParticles.main.maxParticles];

            for (int i = 0; i < aliveCount; i++)
            {
                float age = particles[i].startLifetime - particles[i].remainingLifetime;

                if (age >= _homingDelay)
                {
                    limitVelocity.drag = 0f;

                    Vector3 dir = (transform.position - particles[i].position).normalized;
                    particles[i].velocity = Vector3.Lerp(particles[i].velocity, dir * _homingStrength, Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate() * 5); //Time.deltaTime * 5f); // smoothing

                    float distance = Vector3.Distance(particles[i].position, transform.position);
                    if (distance < _particleDespawnDistance)
                    {
                        particles[i].remainingLifetime = 0f;
                        particlesRemaining--;
                        amountRemaining--;
                        _gemRequirementText.SetText(amountRemaining.ToString());
                        if (amountRemaining == 0)
                            _gemstoneParticles.Stop();
                    }
                }
            }
            _gemstoneParticles.transform.position = playerdata.PlayerTransform.position;
            _gemstoneParticles.SetParticles(particles, aliveCount);
            _gemstoneParticles.Simulate(Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate(), withChildren: true, restart: false, fixedTimeStep: false);
            yield return new Animation_Frame_Rate_Manager.WaitForTick();
        }
        _gemstoneParticles.Clear();

        // Does the Player have enough gemstones to break the wall
        if (savedGemstones >= data.GemRequirement) AbsorptionSucceeded();
        else AbsorptionFailed();

        yield break;
    }
    #endregion

    IEnumerator EmitParticlesOverTime(int amount, float time, float multiplierPerEmit = 1f)
    {
        int count = 0;
        float t = time;
        int c = Mathf.RoundToInt(amount * .9f);

        while (_isConsuming)
        {
            if (count >= c)
            {
                int g = amount - count;
                _gemstoneParticles.Emit(g);
                GameManager.GameLogic.AddGemstones(-g);
                yield break;
            }
            else
            {
                _gemstoneParticles.Emit(1);
                GameManager.GameLogic.AddGemstones(-1);
                count++;
            }
            if (count >= amount) yield break;

            yield return new WaitForSeconds(t);
            t = Mathf.Round((t *= multiplierPerEmit) * 1000f) / 1000f;
            print(t);
        }
    }

    IEnumerator ExplodeWall()
    {
        float t = 0f;

        while (t < _explodeDuration)
        {
            t += Time.deltaTime;

            // TODO: Animate gate explosion here
            yield return null;
        }
        yield break;
    }

    #region Absorption Events
    void AbsorptionSucceeded()
    {
        print("Absorption Succeeded!");
        _isMoving = true;
        _gemRequirementText.SetText(0.ToString());

        _art.SetActive(false);

        // StartCoroutine(ExplodeWall()); //TODO
    }

    void AbsorptionFailed()
    {
        print("Absorption Failed...");
        // Trigger Freeze Time, except for this object, here
        // Trigger Laser Player Boat Animation here

        // TODO: Ideally there should be three ways of ending the game, forcing it to end, killing the player or destroying their boat. This is temporary
        _isMoving = false;
        GameManager.GameLogic.EndGame();
    }
    #endregion
}

[Serializable]
public class GemstoneGateData
{
    /// <summary>
    /// The amount of gems required to destroy the gate
    /// </summary>
    public int GemRequirement = 10;
}
