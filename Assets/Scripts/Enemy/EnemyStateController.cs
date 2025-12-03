using UnityEngine;

public abstract class EnemyStateController : StateController
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
    [SerializeField] bool _isMoving;
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
    [SerializeField] bool _isErupting;
    public bool IsErupting { get { return _isErupting; } set { _isErupting = value; } }

    [Header("Data")]
    [SerializeField] GameObject _enemyArtRoot;
    public GameObject EnemyArtRoot { get { return _enemyArtRoot; } }

    [Header("Components")]
    [SerializeField] Animator _animator;
    /// <summary>
    /// 0 = Idle | 1 = Emerging | 2 = Moving | 3 = Attacking | 4 = Defeated
    /// </summary>
    public Animator Animator { get { return _animator; } }

    [Space]

    [SerializeField] ParticleSystem _eruptParticles;
    public ParticleSystem EruptParticles {  get { return _eruptParticles; } }
    [SerializeField] ParticleSystem _splashParticles;
    public ParticleSystem SplashParticles { get { return _splashParticles; } }

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

    public abstract void Death();
}
