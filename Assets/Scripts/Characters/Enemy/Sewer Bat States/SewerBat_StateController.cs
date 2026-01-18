using UnityEngine;

public class SewerBat_StateController : BoatEnemyStateController
{
    /*
     * ==========================================================
     * The State Machine controller for the Sewer Bat Enemy
     * ==========================================================
     */

    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);
    }

    [Header("Sewer Bat Data")]
    [SerializeField] private SO_EnemyData_SewerBat batData;
    public SO_EnemyData_SewerBat BatData => batData;

    public override EnemyIdleState IdleState { get; } = new SewerBat_IdleState();
    public override EnemyEmergeState EmergeState { get; } = new SewerBat_EmergeState();
    public override EnemyMovingState MovingState { get; } =  new SewerBat_MovingState();
    public override EnemyAttackState AttackState { get; } = new SewerBat_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } =  new SewerBat_DefeatedState();
    
    public override void EmergeFromRiver()
    {
        
    }
}

public class SewerBat_IdleState : EnemyIdleState
{
    public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();

    }

    public override void UpdateState()
    {
        base.UpdateState();

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}

public class SewerBat_EmergeState : EnemyEmergeState
{
    public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();

    }

    public override void UpdateState()
    {
        base.UpdateState();

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}

public class SewerBat_MovingState : EnemyMovingState
{
    public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();

    }

    public override void UpdateState()
    {
        base.UpdateState();

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}

public class SewerBat_AttackState : EnemyAttackState
{
    public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();

    }

    public override void UpdateState()
    {
        base.UpdateState();

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}

public class SewerBat_DefeatedState : EnemyDefeatedState
{
    public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();

    }

    public override void UpdateState()
    {
        base.UpdateState();

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}