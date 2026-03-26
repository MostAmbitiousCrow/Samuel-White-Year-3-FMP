using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public override void OnEnter()
    {
        Sc.isAttacking = true;
    }

    public override void OnExit()
    {
        Sc.isAttacking = false;

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
