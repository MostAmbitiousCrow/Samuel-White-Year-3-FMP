using UnityEngine;
public class EnemyEmergeState : EnemyState
{
    public override void OnEnter()
    {
        //Debug.Log($"{Sc.name} entered Emerge State");
        Sc.isErupting = true;
        Sc.Animator.SetTrigger("Erupting");

    }

    public override void OnExit()
    {
        Sc.isErupting = false;

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
