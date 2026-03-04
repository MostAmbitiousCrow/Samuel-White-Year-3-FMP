using UnityEngine;

public class FreakyFrog_StateController : BoatEnemyStateController
{
    /*
     * ==========================================================
     * The State Machine controller for the Freaky Frog Enemy
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
    
    [Header("Freaky Frog Data")]
    [SerializeField] private SO_EnemyData_FreakyFrog frogData;
    public SO_EnemyData_FreakyFrog FrogData => frogData;
    
    public override EnemyIdleState IdleState { get; } = new FreakyFrog_IdleState();
    public override EnemyEmergeState EmergeState { get; } =  new FreakyFrog_EmergeState();
    public override EnemyMovingState MovingState { get; } =  new FreakyFrog_MovingState();
    public override EnemyAttackState AttackState { get; } = new FreakyFrog_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } =  new FreakyFrog_DefeatedState();
    
    public override void EmergeFromRiver()
    {
        throw new System.NotImplementedException();
    }
}

public class FreakyFrog_IdleState : EnemyIdleState
{
    public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;

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

public class FreakyFrog_EmergeState : EnemyEmergeState
{
    public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;

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

public class FreakyFrog_MovingState : EnemyMovingState
{
    public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;

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

public class FreakyFrog_AttackState : EnemyAttackState
{
    public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;

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

public class FreakyFrog_DefeatedState : EnemyDefeatedState
{
    public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;

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