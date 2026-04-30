using System;
using CarterGames.Assets.AudioManager;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BoxCollider))]
public class Gemstone : River_Collectible
{
    /// <summary>
    /// This object's art object that will be animated
    /// </summary>
    [Header("Art Animation Control")]
    [Tooltip("This object's art object that will be animated")]
    [SerializeField] protected Transform _artObject;
    /// <summary>
    /// The rotation speed of the art object
    /// </summary>
    [Tooltip("The rotation speed of the art object")]
    [SerializeField] protected float _rotateSpeed = 1f;

    /// <summary>
    /// The hover speed of the art object
    /// </summary>
    [Tooltip("The hover speed of the art object")]
    [SerializeField] protected float _hoverSpeed = 1f;

    /// <summary>
    /// The animation curve for the art object's hover effect
    /// </summary>
    [Tooltip("The animation curve for the art object's hover effect")]
    [SerializeField] protected AnimationCurve _hoverCurve;

    /// <summary>
    /// Particles that will play upon being collected
    /// </summary>
    [Header("Effects")]
    [Tooltip("Particles that will play upon being collected")]
    [SerializeField] ParticleSystem _collectParticles;
    /// <summary>
    /// The Standard Particles that play on this object
    /// </summary>
    [Tooltip("The Standard Particles that play on this object")]
    [SerializeField] ParticleSystem _idleParticles;
    /// <summary>
    /// The amount of particles that will appear upon collection
    /// </summary>
    [Tooltip("The amount of particles that will appear upon collection")]
    [SerializeField] int _collectParticlesAmount = 30;
    [SerializeField] Transform _particleHomeTarget;

    [Tooltip("The time during the collect particle phase before the particles begin to home in on the target")]
    [SerializeField] float homingDelay = 2f;
    [Tooltip("The strength of the homing collect particles")]
    [SerializeField] float homingStrength = 1f;
    [SerializeField] float particleDespawnDistance = .5f;

    #region Collection Event
    protected override void OnCollected()
    {
        base.OnCollected();

        _collectParticles.Emit(_collectParticlesAmount);
        _artObject.gameObject.SetActive(false);
        GameManager.GameLogic.AddGemstones(_collectParticlesAmount * Data.BankValue);
        AudioManager.Play(Clip.Gem_Smash);

        _idleParticles.Stop();
        isMoving = false; //TODO: Temp
    }
    #endregion

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();
        _artObject.gameObject.SetActive(true);
        // TODO
    }

    // #region FrameRateManager Subscription
    //
    // private void OnEnable()
    // {
    //     Animation_Frame_Rate_Manager.OnTick += HandleOnTick;
    //
    //     if(GameManager.Instance) _particleHomeTarget = GameManager.GameLogic.playerData.PlayerTransform;
    // }
    //
    // private void OnDisable()
    // {
    //     Animation_Frame_Rate_Manager.OnTick -= HandleOnTick;
    // }
    //
    // private void HandleOnTick(object sender, Animation_Frame_Rate_Manager.OnTickEvent tickEvent)
    // {
    //     AnimateArtObject();
    //     TickParticles();
    //     ParticleAnimation();
    // }
    // #endregion

    private void Start()
    {
        if(GameManager.Instance) _particleHomeTarget = GameManager.GameLogic.playerData.PlayerTransform;
    }

    protected override void TimeUpdate() //TODO: Make gemstone stop upon being smashed
    {
        AnimateArtObject();
        TickParticles();
        ParticleAnimation();
    }

    #region Animation
    private void AnimateArtObject()
    {
        // Rotate the art object
        _artObject.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime); // Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate());

        // Animate the hover effect
        float hoverY = _hoverCurve.Evaluate(Mathf.PingPong(Time.time * _hoverSpeed, 1));
        _artObject.localPosition = new Vector3(_artObject.localPosition.x, hoverY, _artObject.localPosition.z);
    }

    private void TickParticles()
    {
        float step = Time.deltaTime;
        _collectParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
        _idleParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
    }

    private ParticleSystem.Particle[] _particles;

    private void ParticleAnimation() // TODO: Turn into a coroutine
    {
        // Debug.Log($"Target = {_particleHomeTarget}. Is Collected = {IsCollected}");
        if (!_particleHomeTarget || !IsCollected) return;
        // Debug.Log("Doing The Particle Animation");

        // Make sure buffer is large enough
        if (_particles == null || _particles.Length < _collectParticles.main.maxParticles)
            _particles = new ParticleSystem.Particle[_collectParticles.main.maxParticles];

        int aliveCount = _collectParticles.GetParticles(_particles);

        for (int i = 0; i < aliveCount; i++)
        {
            if (_particles[i].totalVelocity == Vector3.zero)
            {
                print("Stopped");
                return;
            }

            float age = _particles[i].startLifetime - _particles[i].remainingLifetime;

            if (age >= homingDelay)
            {
                Vector3 dir = (_particleHomeTarget.position - _particles[i].position).normalized;
                _particles[i].velocity = Vector3.Lerp(_particles[i].velocity, dir * homingStrength, Time.deltaTime * 5f); //Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate() * 5); // smoothing

                float distance = Vector3.Distance(_particles[i].position, _particleHomeTarget.position);
                if (distance < particleDespawnDistance)
                {
                     GameManager.GameLogic.AddGemstones(Data.BankValue); // Replace to update the Players visual Gemstone Count
                    _particles[i].remainingLifetime = 0f;
                    AudioManager.Play(Clip.Gem_Collect);
                }
            }
        }
        if (aliveCount > 0) _collectParticles.SetParticles(_particles, aliveCount);
        else
        {
            IsCollected = false;
            isMoving = true;
        }
    }
    #endregion
}
