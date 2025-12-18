using UnityEngine;

public class LeapingCrocodile_MovingState : EnemyMovingState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

    private float currentTimeUntilMove = 0f;
    private float currentCooldownTime = 0f;
    private float currentDelayTime = 0f;
    private bool stepped;

    public override void OnEnter()
    {
        base.OnEnter();

        //CrocSc.BoatCharacterController.CanAccessOuterSides = false; // Disable Outer Boat Side Access

        currentTimeUntilMove = currentCooldownTime = currentDelayTime = 0f;
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    public override void OnHurt()
    {
        base.OnHurt();
        CrocSc.ChangeState(CrocSc.DefeatedState);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (!CrocSc.BoatCharacterController.CanMove) return;

        if (CrocSc.BoatCharacterController.IsMoving) // Time nothing if the croc is already moving
        {
            currentTimeUntilMove = 0f;
            return;
        }
        else if (stepped) // Do Cooldown if they've already moved
        {
            if (currentCooldownTime < CrocSc.EnemyData.CoolDownPerStep)
            {
                currentCooldownTime += Time.deltaTime;
            }
            else
            {
                // End Move Cooldown
                stepped = false;
                currentCooldownTime = 0;
            }
            return;
        }

        // Progress to the next move
        currentTimeUntilMove += Time.deltaTime;

        if (currentTimeUntilMove > CrocSc.EnemyData.TimeUntilStep) // Move the Croc
        {
            // If blocked, swap current direciton
            if (!CrocSc.BoatCharacterController.CheckAvailableSpaceFromDirection((int)CrocSc.CurrentDirection))
            {
                CrocSc.FlipDirection();
                // TODO: Flip Animation in FlipDirection()
            }
            else
            {
                CrocSc.BoatCharacterController.MoveToSpaceInDirection((int)CrocSc.CurrentDirection);
                 //TODO: Trigger Animation in method
            }
            stepped = true;

        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();

        // Detect if the player is in the space ahead of them based on the current facing directions
        Vector3 direction = (int)CrocSc.CurrentDirection * -1 * CrocSc.EnemyData.AttackDistance * Vector3.right + CrocSc.transform.position;
        if (CharacterSpaceChecks.ScanAreaForDamageableCharacter(direction, Vector3.one, Quaternion.identity, CrocSc.PlayerMask))
        {
            CrocSc.ChangeState(CrocSc.AttackState);
        }
    }
}
