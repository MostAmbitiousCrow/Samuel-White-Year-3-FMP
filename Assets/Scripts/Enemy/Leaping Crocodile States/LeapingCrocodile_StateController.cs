using UnityEngine;
using EditorAttributes;

public class LeapingCrocodile_StateController : EnemyStateController
{
    /*
    * ==========================================================
    * The State Machine controller for the Leaping Crocodile
    * Enemy
    * ==========================================================
    */

    #region Variables
    #region States
    //public LeapingCrocodile_IdleState IdleState { get; } = new LeapingCrocodile_IdleState();
    //public LeapingCrocodile_EmergeState EmergeState { get; } = new LeapingCrocodile_EmergeState();
    //public LeapingCrocodile_MovingState MovingState { get; } = new LeapingCrocodile_MovingState();
    //public LeapingCrocodile_AttackState AttackState { get; } = new LeapingCrocodile_AttackState();
    //public LeapingCrocodile_DefeatedState DefeatedState { get; } = new LeapingCrocodile_DefeatedState();
    public override EnemyIdleState IdleState { get; } = new LeapingCrocodile_IdleState();
    public override EnemyEmergeState EmergeState { get; } = new LeapingCrocodile_EmergeState();
    public override EnemyMovingState MovingState { get; } = new LeapingCrocodile_MovingState();
    public override EnemyAttackState AttackState { get; } = new LeapingCrocodile_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } = new LeapingCrocodile_DefeatedState();
    #endregion

    [SerializeField] SO_EnemyData_LeapingCrocodile _enemyData;
    public SO_EnemyData_LeapingCrocodile EnemyData { get { return _enemyData; } }

    #endregion

    private void Start()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);
    }

    /// <summary> Emerges the enemy from the River </summary>
    public override void EmergeFromRiver()
    {
        ChangeState(EmergeState);
    }

    public override void Death()
    {
        ChangeState(DefeatedState);
    }
}
