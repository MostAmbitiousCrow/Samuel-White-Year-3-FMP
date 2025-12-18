using UnityEngine;

public class LeapingCrocodile_AttackState : EnemyAttackState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

    private float currentWaitTime;
    private bool hasAttacked;

    public override void OnEnter()
    {
        base.OnEnter();
        currentWaitTime = 0f;
        hasAttacked = false;
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

        if (!hasAttacked)
        {
            currentWaitTime += Time.deltaTime;

            if (currentWaitTime > CrocSc.CrocData.AttackDelay)
            {
                Vector3 direction = (int)CrocSc.CurrentDirection * -1 * CrocSc.EnemyData.AttackDistance * Vector3.right + CrocSc.transform.position;
                CharacterStateController player = CharacterSpaceChecks.ScanAreaForDamageableCharacter(direction, Vector3.one, Quaternion.identity, CrocSc.PlayerMask);

                hasAttacked = true;
                Sc.Animator.SetTrigger(3);

                if (player != null)
                {
                    currentWaitTime = 0;
                    player.GetComponent<IDamageable>().TakeDamage();
                    Debug.Log("Damaged Player");
                }
            }
        }
        else // Has attacked. Do cooldown and move back to Moving State
        {
            currentWaitTime += Time.deltaTime;

            if (currentWaitTime > CrocSc.CrocData.AttackCooldown)
            {
                CrocSc.ChangeState(CrocSc.MovingState);
            }
        }

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

    }
}
