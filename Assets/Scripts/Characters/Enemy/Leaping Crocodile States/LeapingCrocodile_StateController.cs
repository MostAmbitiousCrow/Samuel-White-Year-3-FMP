using UnityEngine;

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
    public override EnemyIdleState IdleState { get; } = new LeapingCrocodile_IdleState();
    public override EnemyEmergeState EmergeState { get; } = new LeapingCrocodile_EmergeState();
    public override EnemyMovingState MovingState { get; } = new LeapingCrocodile_MovingState();
    public override EnemyAttackState AttackState { get; } = new LeapingCrocodile_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } = new LeapingCrocodile_DefeatedState();
    #endregion

    [SerializeField] SO_EnemyData_LeapingCrocodile _enemyData;
    public override SO_EnemyData EnemyData => _enemyData;
    /// <summary>
    /// Crocodile Data converted from the Enemy Data
    /// </summary>
    public SO_EnemyData_LeapingCrocodile CrocData
    { get
        {
            SO_EnemyData_LeapingCrocodile crocData = EnemyData as SO_EnemyData_LeapingCrocodile;
            return crocData;
        }
    }

    #endregion

    private void Start()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);

        CrocData.LeapSpeedMultiplier = 0f;
    }

    /// <summary> Emerges the enemy from the River </summary>
    public override void EmergeFromRiver()
    {
        Debug.Log($"{name} has emerged!");

        BoatCharacterController.CanAccessOuterSides = true; // Enable Outer Boat Side Access

        BoatCharacterController.GoToSideSpace(BoatData.TargetSideSpace, BoatData.TargetLeftSide); // TODO: Update to move the croc to its targetted space


        ChangeState(EmergeState);
    }

    public override void InitialiseEnemy(BoatEnemy_Data data)
    {
        base.InitialiseEnemy(data);
    }

    public override void TookDamage()
    {
        CurrentState.OnHurt();
        StoreState(CurrentState);

        ChangeState(DefeatedState);
    }

    public override void Died()
    {
        ChangeState(DefeatedState);
    }

    public override void HealthRestored()
    {

    }

#if UNITY_EDITOR
    //private void OnDrawGizmos()
    //{
    //    if (IsAttacking)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireCube(tempTargetPos, Vector3.one + (Vector3.up * 2));
    //    }
    //}
#endif
}
