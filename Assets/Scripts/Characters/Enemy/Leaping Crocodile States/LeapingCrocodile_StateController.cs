using System.Collections;
using UnityEngine;

public class LeapingCrocodile_StateController : BoatEnemyStateController
{
    /*
    * ==========================================================
    * The State Machine controller for the Leaping Crocodile Enemy
    * ==========================================================
    */

    #region Variables

    [Header("Crocodile Data")]
    [SerializeField] private SO_EnemyData_LeapingCrocodile crocData;
    public SO_EnemyData_LeapingCrocodile CrocData => crocData;
    
    #region States
    public override EnemyIdleState IdleState { get; } = new LeapingCrocodile_IdleState();
    public override EnemyEmergeState EmergeState { get; } = new LeapingCrocodile_EmergeState();
    public override EnemyMovingState MovingState { get; } = new LeapingCrocodile_MovingState();
    public override EnemyAttackState AttackState { get; } = new LeapingCrocodile_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } = new LeapingCrocodile_DefeatedState();
    #endregion
    /// <summary>
    /// Crocodile Data converted from the Enemy Data
    /// </summary>
    // public SO_EnemyData_LeapingCrocodile CrocData
    // { get
    //     {
    //         var crocData = enemydata as SO_EnemyData_LeapingCrocodile;
    //         return crocData;
    //     }
    // }

    #endregion

    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);
    }

    /// <summary> Emerges the enemy from the River </summary>
    public override void EmergeFromRiver()
    {
        base.EmergeFromRiver();
        
        ChangeState(EmergeState);
    }
}


public class LeapingCrocodile_IdleState : EnemyIdleState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

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

public class LeapingCrocodile_EmergeState : EnemyEmergeState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
    private float _currentEmergeTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        
        CrocSc.canAccessOuterBoatSides = true;
        CrocSc.canAccessBoatSpaces = true;
        
        // Go To Side Space on the Boat
        CrocSc.SetDirection(CrocSc.boatEnterData.startFacingDirection, false);
        CrocSc.GoToSideSpace(CrocSc.boatEnterData.targetBoatSide, CrocSc.boatEnterData.targetLeftSide);
        
        _currentEmergeTime = 0f;
        CrocSc.EnterBoat(false);
        
        CrocSc.Animator.SetTrigger("Emerge");
    }

    public override void UpdateState()
    {
        if (_currentEmergeTime > CrocSc.EmergeDelay)
        {
            if (!CrocSc.isJumping && CrocSc.IsGrounded)
            {
                // Trigger leap and move towards 
                CrocSc.TriggerJump();
                CrocSc.SetDirection(CrocSc.boatEnterData.boardingFacingDirection, true);
                CrocSc.MoveToSpace(CrocSc.boatEnterData.targetSideSpace, CrocSc.boatEnterData.targetSpace);
            }
            // When the Crocodile has landed on the boat, patrol
            else
            {
                CrocSc.ChangeState(CrocSc.MovingState);
            }
        }
        else
        {
            _currentEmergeTime += Time.deltaTime;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        // Revoke access to outer sides after they've landed on the boat
        CrocSc.canAccessOuterBoatSides = false;
        CrocSc.canAccessBoatSpaces = true;
        
        CrocSc.Animator.SetTrigger("Idle");

        // Set the direction of the enemy upon landing on the boat
        // TODO:
        // This doesn't work with the way ground detection works... Exit is getting triggered before the croc
        // Reaches the ground
        
        // CrocSc.SetDirection(CrocSc.boatEnterData.boardingFacingDirection, false);
    }

    public override void OnHurt()
    {

        base.OnHurt();
    }
}

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

        if (!CrocSc.canMove) return;

        if (CrocSc.IsMoving) // Time nothing if the croc is already moving
        {
            currentTimeUntilMove = 0f;
            return;
        }
        else if (stepped) // Do Cooldown if they've already moved
        {
            if (currentCooldownTime < CrocSc.CrocData.coolDownPerStep)
            {
                currentCooldownTime += Time.deltaTime;
            }
            else
            {
                // End Move Cooldown
                CrocSc.Animator.SetTrigger("Idle");
                stepped = false;
                currentCooldownTime = 0;
            }
            return;
        }

        // Progress to the next move
        currentTimeUntilMove += Time.deltaTime;

        if (currentTimeUntilMove > CrocSc.CrocData.timeUntilStep) // Move the Croc
        {
            // If blocked, swap current direction
            if (!CrocSc.CheckAvailableSpaceFromDirection((int)CrocSc.CurrentDirection))
            {
                CrocSc.Animator.SetTrigger("Idle");
                CrocSc.FlipDirection();
                // TODO: Flip Animation in FlipDirection()
            }
            else
            {
                CrocSc.Animator.SetTrigger("Move");
                CrocSc.MoveToSpaceFromDirection((int)CrocSc.CurrentDirection);
                 //TODO: Trigger Animation in method
            }
            stepped = true;

        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        if (CrocSc.IsMoving) return;

        // Detect if the player is in the space ahead of them based on the current facing directions
        Vector3 direction = (int)CrocSc.CurrentDirection * CrocSc.CrocData.attackDistance * Vector3.right + CrocSc.transform.position;
        if (CharacterSpaceChecks.ScanAreaForDamageableCharacter(direction, Vector3.one, Quaternion.identity, CrocSc.TargetableCharacterLayers))
        {
            CrocSc.ChangeState(CrocSc.AttackState);
        }
    }
}

public class LeapingCrocodile_AttackState : EnemyAttackState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

    public override void OnEnter()
    {
        base.OnEnter();
        CrocSc.canMove = false;
        CrocSc.StartCoroutine(AttackRoutine());
    }

    public override void OnHurt()
    {
        base.OnHurt();
        
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(CrocSc.CrocData.attackDelay);
        
        Sc.Animator.SetTrigger("Attack");

        yield return new WaitForSeconds(CrocSc.CrocData.attackAtTime);

        var direction = (int)CrocSc.CurrentDirection * CrocSc.CrocData.attackDistance * Vector3.right +
                        CrocSc.transform.position;
        var player = CharacterSpaceChecks.ScanAreaForDamageableCharacter
        (direction, Vector3.one, Quaternion.identity, CrocSc.TargetableCharacterLayers);

        if (player != null)
        {
            player.GetComponent<IDamageable>().TakeDamage();
            Debug.Log("Damaged Player");
        }

        yield return new WaitForSeconds(CrocSc.CrocData.attackCooldown);

        CrocSc.canMove = true;
        CrocSc.Animator.SetTrigger("Idle");
        CrocSc.ChangeState(CrocSc.MovingState);
    }
}

public class LeapingCrocodile_DefeatedState : EnemyDefeatedState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

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
}
