using UnityEngine;

public abstract class EnemyStateController : CharacterStateController
{
    /*
     * ==========================================================
     * The State Machine controller for enemies.
     * Just to note, since I'll probably forget-
     * The Enemy objects root contains the River_Enemy component,
     * responsible for moving the enemy along the River.
     * The River_Enemy script will trigger EmergeFromRiver() when
     * close to the players boat.
     * ==========================================================
     */

    #region Variables
    [Header("State")]
    [SerializeField] bool _isDead;
    public bool IsDead { get { return _isDead; } set { _isDead = value; } }
    //[SerializeField] bool _isMoving;
    //public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
    [SerializeField] bool _isErupting;
    public bool IsErupting { get { return _isErupting; } set { _isErupting = value; } }
    [SerializeField] bool _isAttacking;
    public bool IsAttacking { get { return _isAttacking; } set { _isAttacking = value; } }

    public MoveDirection CurrentDirection { get; private set; } = MoveDirection.Left;
    /// <summary> Left = 1 | Right = -1 </summary>
    public enum MoveDirection { Right = -1, Left = 1 }

    [Header("Data")]
    [SerializeField] GameObject _enemyArtRoot;
    public GameObject EnemyArtRoot { get { return _enemyArtRoot; } }
    public LayerMask PlayerMask { get { return _playerLayerMask; } }
    [SerializeField] LayerMask _playerLayerMask;

    public abstract SO_EnemyData EnemyData { get; }
    public BoatEnemy_Data BoatData { get; private set; }

    [Header("Components")]
    [SerializeField] Animator _animator;
    /// <summary>
    /// 0 = Idle | 1 = Emerge | 2 = Move | 3 = Attack | 4 = Defeated | 5 = Damaged
    /// </summary>
    public Animator Animator { get { return _animator; } }
    [SerializeField] River_Enemy _riverObject;
    public River_Enemy RiverObject { get { return _riverObject; } }

    //[Tooltip("The Characters Health Component")]
    //[SerializeField] CharacterHealth _characterHealth;
    //public CharacterHealth HealthComponent { get { return _characterHealth; } }

    [Space]

    [SerializeField] ParticleSystem _eruptParticles;
    public ParticleSystem EruptParticles {  get { return _eruptParticles; } }
    [SerializeField] ParticleSystem _splashParticles;
    public ParticleSystem SplashParticles { get { return _splashParticles; } }

    [Space]

    [Tooltip("Reference to the Boat Character Controller for boat movement")]
    [SerializeField] CharacterStateController _stateController;
    public CharacterStateController BoatCharacterController { get { return _stateController; } }

    // TODO: Sounds



    #endregion

    #region States
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

        BoatData = data;

        // TODO: Add any additional initialsiation processes here. Likely after enemy pooling
    }

    /// <summary> Returns the enemy back to the pooling system | NOTE: Pool System Currently not Implemented </summary>
    public virtual void ReturnEnemy()
    {
        // TODO: Temporary. Replace to disable the object and return to its River Object Root
        transform.parent = RiverObject.transform;
        Destroy(RiverObject.gameObject);
    }

    /// <summary> Reverses the current direction of the enemy </summary>
    public void FlipDirection() // TODO: Flip the enemy character art to face current direction
    {
        if (CurrentDirection == MoveDirection.Left) SetDirection(MoveDirection.Right);
        else if (CurrentDirection == MoveDirection.Right) SetDirection(MoveDirection.Left);
    }

    /// <summary> Explicitly sets the direction of the enemy with a given parameter </summary>
    public void SetDirection(MoveDirection direction)
    {
        CurrentDirection = direction;

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
