using System;
using System.Collections;
using EditorAttributes;
using UnityEngine;

public class Gemstone_Gate : River_Object, ITargetsBoat
{
    [Header("Components")]
    [Tooltip("The voxels forming the gate that will explode")]
    [SerializeField] Transform[] _gateBlocks;

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
    [SerializeField] bool _consuming;

    [Header("Components")]
    [SerializeField] ParticleSystem _gemstoneParticles;
    [Space(10)]
    [Header("Particle Settings")]
    [Tooltip("The time during the collect particle phase before the particles begin to home in on the target")]
    [SerializeField] float homingDelay = 2f;
    [Tooltip("The strength of the homing collect particles")]
    [SerializeField] float homingStrength = 1f;
    [SerializeField] float particleDespawnDistance = .5f;

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
        if (_consuming) return;

        if (SpaceManager.GetDistanceToBoat(_distance) < _distanceUntilConsumption)
        {
            StartCoroutine(ConsumeGemstones());
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        _isMoving = true;
    }

    private ParticleSystem.Particle[] particles;

    IEnumerator ConsumeGemstones() //TODO: WIP
    {
        _consuming = true;

        GameManager.MainGameLogic.PlayerData playerdata = GameManager.Instance.GameLogic.playerData;

        if (playerdata.PlayerTransform == null) yield break;

        _gemstoneParticles.Play();

        var main = _gemstoneParticles.main;
        main.maxParticles = Mathf.RoundToInt(playerdata.CurrentGemstones);

        int aliveCount = _gemstoneParticles.GetParticles(particles);

        while (aliveCount < 0)
        {
            aliveCount = _gemstoneParticles.GetParticles(particles);

            // Make sure buffer is large enough
            if (particles == null || particles.Length < _gemstoneParticles.main.maxParticles)
            particles = new ParticleSystem.Particle[_gemstoneParticles.main.maxParticles];

            for (int i = 0; i < aliveCount; i++)
            {
                if (particles[i].totalVelocity == Vector3.zero)
                {
                    print("Stopped");
                    yield return null;
                }

                float age = particles[i].startLifetime - particles[i].remainingLifetime;

                if (age >= homingDelay)
                {
                    Vector3 dir = (transform.position - particles[i].position).normalized;
                    particles[i].velocity = Vector3.Lerp(particles[i].velocity, dir * homingStrength, Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate() * 5); //Time.deltaTime * 5f); // smoothing

                    float distance = Vector3.Distance(particles[i].position, transform.position);
                    if (distance < particleDespawnDistance)
                    {
                        particles[i].velocity = Vector3.zero;
                        particles[i].startColor = Color.clear;
                        GameManager.Instance.GameLogic.AddGemstones();
                    }
                }
            }
            _gemstoneParticles.Simulate(Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate(), withChildren: true, restart: false, fixedTimeStep: false);
            yield return new Animation_Frame_Rate_Manager.WaitForTick();
        }

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
}

[Serializable]
public class GemstoneGateData
{
    public int GemRequirement = 10;
}
