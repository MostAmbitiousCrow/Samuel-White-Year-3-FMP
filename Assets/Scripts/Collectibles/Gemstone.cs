using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BoxCollider))]
public class Gemstone : River_Collectible
{
    [Header("Art Animation Control")]
    /// <summary>
    /// This object's art object that will be animated
    /// </summary>
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

    [Header("Effects")]
    /// <summary>
    /// Particles that will play upon being collected
    /// </summary>
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
        _collectParticles.Play();
        _artObject.gameObject.SetActive(false);
        GameManager.Instance.GameLogic.AddGemstones(_collectParticlesAmount * Data.BankValue);

        _idleParticles.Stop();
        _isMoving = false; //TODO: Temp
    }
    #endregion

    protected override void Reset()
    {
        base.Reset();
        _artObject.gameObject.SetActive(true);
        // TODO
    }

    #region FrameRateManager Subscription
    void OnEnable()
    {
        Animation_Frame_Rate_Manager.OnTick += HandleOnTick;

        if(GameManager.Instance != null)
            _particleHomeTarget = GameManager.Instance.GameLogic.playerData.PlayerTransform;
    }

    void OnDisable()
    {
        Animation_Frame_Rate_Manager.OnTick -= HandleOnTick;
    }

    private void HandleOnTick(object sender, Animation_Frame_Rate_Manager.OnTickEvent tickEvent)
    {
        AnimateArtObject();
        TickParticles();
        ParticleAnimation();
    }
    #endregion

    #region Animation
    private void AnimateArtObject()
    {
        // Rotate the art object
        _artObject.Rotate(Vector3.up, _rotateSpeed * Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate());

        // Animate the hover effect
        float hoverY = _hoverCurve.Evaluate(Mathf.PingPong(Time.time * _hoverSpeed, 1));
        _artObject.localPosition = new Vector3(_artObject.localPosition.x, hoverY, _artObject.localPosition.z);
    }

    private void TickParticles()
    {
        float step = Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate();
        _collectParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
        _idleParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
    }

    private ParticleSystem.Particle[] particles;

    private void ParticleAnimation() // TODO: Turn into a coroutine
    {
        if (_particleHomeTarget == null || !IsCollected) return;

        // Make sure buffer is large enough
        if (particles == null || particles.Length < _collectParticles.main.maxParticles)
            particles = new ParticleSystem.Particle[_collectParticles.main.maxParticles];

        int aliveCount = _collectParticles.GetParticles(particles);

        for (int i = 0; i < aliveCount; i++)
        {
            if (particles[i].totalVelocity == Vector3.zero)
            {
                print("Stopped");
                return;
            }

            float age = particles[i].startLifetime - particles[i].remainingLifetime;

            if (age >= homingDelay)
            {
                Vector3 dir = (_particleHomeTarget.position - particles[i].position).normalized;
                particles[i].velocity = Vector3.Lerp(particles[i].velocity, dir * homingStrength, Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate() * 5); //Time.deltaTime * 5f); // smoothing

                float distance = Vector3.Distance(particles[i].position, _particleHomeTarget.position);
                if (distance < particleDespawnDistance)
                {
                    // GameManager.Instance.GameLogic.AddGemstones(Data.BankValue); // Replace to update the Players visual Gemstone Count
                    particles[i].remainingLifetime = 0f;
                }
            }
        }
        if (aliveCount > 0) _collectParticles.SetParticles(particles, aliveCount);
        else
        {
            IsCollected = false;
            _isMoving = true;
        }
    }
    #endregion
}
