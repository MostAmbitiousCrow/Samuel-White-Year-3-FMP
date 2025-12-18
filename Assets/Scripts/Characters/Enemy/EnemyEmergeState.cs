using UnityEngine;
public class EnemyEmergeState : EnemyState
{
    public override void OnEnter()
    {
        Debug.Log($"{Sc.name} entered {this}");
        Sc.IsErupting = true;
        Sc.Animator.SetTrigger(1);

    }

    public override void OnExit()
    {
        Sc.IsErupting = false;

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
