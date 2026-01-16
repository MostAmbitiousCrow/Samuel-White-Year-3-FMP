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
    public BoatEnemy_Data boatEnterData;
    public abstract SO_EnemyData EnemyData { get; set; }

    [Header("Components")] 
    [SerializeField] protected River_Enemy riverObject;

    [Space]

    [SerializeField] protected ParticleSystem eruptParticles;
    [SerializeField] protected ParticleSystem splashParticles;

    // TODO: Sounds



    #endregion

    #region States
    
    protected IState CurrentState { get; private set; }
    protected IState StoredState { get; private set; }
    
    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        print($"New State: {CurrentState}");
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
    #endregion

    /// <summary> Emerges the enemy from the River </summary>
    public abstract void EmergeFromRiver();

    /// <summary> Method to call upon this enemy appearing in the level. Additional data can be provided </summary>
    public virtual void InitialiseEnemy(BoatEnemy_Data data)
    {
        HealthComponent.RestoreHealth();
        boatEnterData = data;

        // TODO: Add any additional initialisation processes here. Likely after enemy pooling
    }

    /// <summary> Returns the enemy back to the pooling system | NOTE: Pool System Currently not Implemented </summary>
    public virtual void ReturnEnemy()
    {
        // TODO: Temporary. Replace to disable the object and return to its River Object Root
        transform.parent = riverObject.transform;
        Destroy(riverObject.gameObject);
    }

    public override void OnTookDamage()
    {
        base.OnTookDamage();
        CurrentState.OnHurt();
        StoreState(CurrentState);

        ChangeState(DefeatedState);
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
