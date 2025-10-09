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
    [SerializeField] float _homingDelay = 2f;
    [Tooltip("The strength of the homing collect particles")]
    [SerializeField] float _homingStrength = 1f;
    [SerializeField] float _particleDespawnDistance = .5f;
    [Space]
    [SerializeField] TextMeshPro _gemRequirementText;

    public Boat_Space_Manager SpaceManager { get; set; }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        SpaceManager = bsm;
    }

    public void OverrideData(GemstoneGateData overridedData)
    {
        data = overridedData;
        print($"{name} stats were overrided");
    }

    protected override void VirtualUpdateMethod()
    {
        base.VirtualUpdateMethod();
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

    private ParticleSystem.Particle[] particles;

    IEnumerator ConsumeGemstones() //TODO: WIP
    {
        GameManager.MainGameLogic.PlayerData playerdata = GameManager.Instance.GameLogic.playerData;

        if (playerdata.PlayerTransform == null)
        {
            Debug.LogWarning("Missing Player Transform component, Cancelled Gemstone Gate Consumption Event");
            AbsorptionSucceeded(); // Callback
            yield break;
        }

        print($"{name} Started Consumption");
        _isConsuming = true;
        _isMoving = false;

        _gemstoneParticles.transform.position = playerdata.PlayerTransform.position;
        _gemstoneParticles.Emit(data.GemRequirement);

        var main = _gemstoneParticles.main;
        main.maxParticles = Mathf.RoundToInt(playerdata.CurrentGemstones);

        // Initialize particles array if null or too small
        if (particles == null || particles.Length < main.maxParticles)
            particles = new ParticleSystem.Particle[main.maxParticles];

        int aliveCount = _gemstoneParticles.GetParticles(particles);

        print($"Player Gemstone Count: {playerdata.CurrentGemstones}");

        while (aliveCount > 0)
        {
            aliveCount = _gemstoneParticles.GetParticles(particles);;

            for (int i = 0; i < aliveCount; i++)
            {
                float age = particles[i].startLifetime - particles[i].remainingLifetime;

                if (age >= _homingDelay)
                {
                    Vector3 dir = (transform.position - particles[i].position).normalized;
                    particles[i].velocity = Vector3.Lerp(particles[i].velocity, dir * _homingStrength, Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate() * 5); //Time.deltaTime * 5f); // smoothing

                    float distance = Vector3.Distance(particles[i].position, transform.position);
                    if (distance < _particleDespawnDistance)
                    {
                        particles[i].remainingLifetime = 0f;
                        _gemRequirementText.SetText(aliveCount.ToString());
                    }
                }
            }
            _gemstoneParticles.SetParticles(particles, aliveCount);
            _gemstoneParticles.Simulate(Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate(), withChildren: true, restart: false, fixedTimeStep: false);
            yield return new Animation_Frame_Rate_Manager.WaitForTick();
        }

        // Does the Player have enough gemstones to break the wall
        if (playerdata.CurrentGemstones >= data.GemRequirement) AbsorptionSucceeded();
        else AbsorptionFailed();

        yield break;
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
        GameManager.Instance.GameLogic.EndGame();
    }
}

[Serializable]
public class GemstoneGateData
{
    /// <summary>
    /// The amount of gems required to destroy the gate
    /// </summary>
    public int GemRequirement = 10;
}
