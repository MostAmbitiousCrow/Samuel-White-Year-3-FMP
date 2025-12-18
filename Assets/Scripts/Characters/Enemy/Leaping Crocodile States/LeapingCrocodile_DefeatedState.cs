using UnityEngine;

public class LeapingCrocodile_DefeatedState : EnemyDefeatedState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

    public override void OnEnter()
    {
        base.OnEnter();

        // TODO: Add a delay to the enemy being defeated
        CrocSc.ReturnEnemy();
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
