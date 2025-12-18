using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public override void OnEnter()
    {
        Sc.IsAttacking = true;
    }

    public override void OnExit()
    {

    }

    public override void OnHurt()
    {

    }

    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
}
