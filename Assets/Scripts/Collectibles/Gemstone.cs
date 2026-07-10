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
    [SerializeField] protected Transform artObject;
    /// <summary>
    /// The rotation speed of the art object
    /// </summary>
    [Tooltip("The rotation speed of the art object")]
    [SerializeField] protected float rotateSpeed = 1f;

    /// <summary>
    /// The hover speed of the art object
    /// </summary>
    [Tooltip("The hover speed of the art object")]
    [SerializeField] protected float hoverSpeed = 1f;

    /// <summary>
    /// The animation curve for the art object's hover effect
    /// </summary>
    [Tooltip("The animation curve for the art object's hover effect")]
    [SerializeField] protected AnimationCurve hoverCurve;

    /// <summary>
    /// Particles that will play upon being collected
    /// </summary>
    [Header("Effects")]
    [Tooltip("Particles that will play upon being collected")]
    [SerializeField] private ParticleSystem collectParticles;
    /// <summary>
    /// The Standard Particles that play on this object
    /// </summary>
    [Tooltip("The Standard Particles that play on this object")]
    [SerializeField] private ParticleSystem idleParticles;
    /// <summary>
    /// The amount of particles that will appear upon collection
    /// </summary>
    [Tooltip("The amount of particles that will appear upon collection")]
    [SerializeField] private int collectParticlesAmount = 30;
    [SerializeField] private Transform particleHomeTarget;

    [Tooltip("The time during the collect particle phase before the particles begin to home in on the target")]
    [SerializeField] private float homingDelay = 2f;
    [Tooltip("The strength of the homing collect particles")]
    [SerializeField] private float homingStrength = 1f;
    [SerializeField] private float particleDespawnDistance = .2f;

    #region Collection Event
    protected override void OnCollected()
    {
        base.OnCollected();

        collectParticles.transform.position = particleHomeTarget.position;

        collectParticles.Emit(collectParticlesAmount);
        artObject.gameObject.SetActive(false);
        GameManager.GameLogic.AddGemstones(collectParticlesAmount * Data.BankValue);
        AudioManager.Play(Clip.Gem_Smash);

        idleParticles.Stop();
        isMoving = false; //TODO: Temp
    }
    #endregion

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();
        artObject.gameObject.SetActive(true);
        
        //Reset Particle Collect Position
        collectParticles.transform.position = Vector3.zero;
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
        if(GameManager.Instance) particleHomeTarget = GameManager.GameLogic.playerData.PlayerTransform;
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
        artObject.Rotate(Vector3.up, rotateSpeed * Time.deltaTime); // Animation_Frame_Rate_Manager.GetDeltaAnimationFrameRate());

        // Animate the hover effect
        float hoverY = hoverCurve.Evaluate(Mathf.PingPong(Time.time * hoverSpeed, 1));
        artObject.localPosition = new Vector3(artObject.localPosition.x, hoverY, artObject.localPosition.z);
    }

    private void TickParticles()
    {
        float step = Time.deltaTime;
        collectParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
        idleParticles.Simulate(step, withChildren: true, restart: false, fixedTimeStep: false);
    }

    private ParticleSystem.Particle[] _particles;
    
    private void ParticleAnimation()
    {
        if (!particleHomeTarget || !IsCollected) return;

        // Make sure buffer is large enough
        if (_particles == null || _particles.Length < collectParticles.main.maxParticles) 
            _particles = new ParticleSystem.Particle[collectParticles.main.maxParticles];
        var aliveCount = collectParticles.GetParticles(_particles);

        var pos = collectParticles.transform.position;
        collectParticles.transform.position =
            Vector3.Lerp(particleHomeTarget.position, pos, Time.deltaTime * (River_Manager.Instance.currentRiverSpeed * .1f));

        for (int i = 0; i < aliveCount; i++)
        {
            if (_particles[i].totalVelocity == Vector3.zero)
            {
                // print("Stopped");
                return;
            }

            float age = _particles[i].startLifetime - _particles[i].remainingLifetime;

            if (age >= homingDelay) 
            {
                Vector3 dir = (Vector3.zero - _particles[i].position).normalized;
                _particles[i].velocity = Vector3.Lerp(_particles[i].velocity, dir * homingStrength,
                    Time.deltaTime);

                /*_particles[i].position = Vector3.Lerp(_particles[i].position, particleHomeTarget.position,
                        Time.deltaTime * (River_Manager.Instance.TargetRiverSpeed / 2f));*/

                float distance = Vector3.Distance(_particles[i].position, Vector3.zero);
                if (distance < particleDespawnDistance)
                {
                    GameManager.GameLogic.AddGemstones(Data.BankValue);
                    _particles[i].remainingLifetime = 0f;
                    AudioManager.Play(Clip.Gem_Collect);
                }
            }
        }
        if (aliveCount > 0) collectParticles.SetParticles(_particles, aliveCount);
        else
        {
            IsCollected = false;
            isMoving = true;
        }
    }
    #endregion
}
