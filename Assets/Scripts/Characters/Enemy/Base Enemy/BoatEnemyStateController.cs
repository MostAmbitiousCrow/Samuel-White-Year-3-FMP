using UnityEngine;
using EditorAttributes;
using GameCharacters;

public abstract class BoatEnemyStateController : BoatCharacter
{
    /*
     * ==========================================================
     * The State Machine controller for enemies.
     * Just to note, since I'll probably forget...
     * The Enemy objects root contains the River_Enemy component,
     * responsible for moving the enemy along the River.
     * The River_Enemy script will trigger EmergeFromRiver() when
     * close to the players boat.
     * ==========================================================
     */

    #region Variables
    [Title("Enemy")]
    [Line(GUIColor.Red)]
    
    [Header("State")]
    [ReadOnly] public bool isErupting;
    [ReadOnly] public bool isAttacking;

    [Header("Data")]
    [ReadOnly] public BoatEnemy_Data boatEnterData;
    [Space]
    [SerializeField] private float emergeDelay = 2f;
    public float EmergeDelay => emergeDelay;

    [Header("Components")] 
    [SerializeField] protected River_Enemy riverObject;

    [Space]

    [SerializeField] protected ParticleSystem eruptParticles;
    [SerializeField] protected ParticleSystem splashParticles;

    // TODO: Sounds

    protected static readonly int PrepareAttackHash = Animator.StringToHash("Prepare Attack");

    #endregion

    #region States
    
    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;
        ChangeState(IdleState);
    }
    
    protected IState CurrentState { get; private set; }
    protected IState StoredState { get; private set; }
    
    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        Debug.Log($"{name}: New State = {CurrentState}");
        CurrentState.OnEnter();
    }
    
    public void StoreState(IState newState)
    {
        StoredState = newState;
    }
    
    public abstract EnemyIdleState IdleState { get; }
    public abstract EnemyEmergeState EmergeState { get; }
    public abstract EnemyMovingState MovingState { get; }
    public abstract EnemyAttackState AttackState { get; }
    public abstract EnemyDefeatedState DefeatedState { get; }

    protected override void TimeUpdate()
    {
        base.TimeUpdate();
        CurrentState.UpdateState();
    }

    protected override void FixedTimeUpdate()
    {
        base.FixedTimeUpdate();
        CurrentState.FixedUpdateState();
    }

    #endregion

    /// <summary> Emerges the enemy from the River </summary>
    public virtual void EmergeFromRiver()
    {
        SetDirection(currentDirection, false);
    }

    /// <summary> Method to call upon this enemy appearing in the level. Additional data can be provided </summary>
    public virtual void InitialiseEnemy(BoatEnemy_Data data)
    {
        // Assign data, restore health and change to idle state
        boatEnterData = data;
        HealthComponent.RestoreHealth();
        
        // Reset Position and Rotation
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        characterCollider.enabled = true;

        canMove = true;
        isMoving = false;
        isJumping = false;
        isVaulting = false;
        // isGrounded = true;

        canAccessBoatSpaces = true;
        canAccessOuterBoatSides = true;

        // TODO: Add any additional initialisation processes here. Likely after enemy pooling
    }

    /// <summary> Returns the enemy back to the pooling system </summary>
    public virtual void ReturnToPool()
    {
        ExitBoat();
        
        ChangeState(IdleState);
        riverObject.ReturnEnemy(); // Return this enemy to its River Object
        riverObject.ReturnToPool();
    }

    public override void OnTookDamage()
    {
        base.OnTookDamage();
        CurrentState.OnHurt();
        StoreState(CurrentState);
    }

    public override void OnDied()
    {
        base.OnDied();
        ChangeState(DefeatedState);
    }

    public override void OnHealthRestored()
    {
        base.OnHealthRestored();
    }

    #region Injection
    //public Boat_Space_Manager BoatSpaceManager { get; private set; }

    //public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    //{
    //    BoatSpaceManager = bsm;
    //}

    //public River_Manager RiverManager { get; private set; }

    //public void InjectRiverManager(River_Manager manager)
    //{
    //    RiverManager = manager;
    //}
    #endregion
}
