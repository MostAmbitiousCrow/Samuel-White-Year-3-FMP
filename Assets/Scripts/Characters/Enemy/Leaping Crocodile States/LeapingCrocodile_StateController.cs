using UnityEngine;

public class LeapingCrocodile_StateController : BoatEnemyStateController
{
    /*
    * ==========================================================
    * The State Machine controller for the Leaping Crocodile Enemy
    * ==========================================================
    */

    #region Variables
    public override SO_EnemyData EnemyData { get; set; }
    
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
    public SO_EnemyData_LeapingCrocodile CrocData
    { get
        {
            var crocData = EnemyData as SO_EnemyData_LeapingCrocodile;
            return crocData;
        }
    }

    #endregion

    private void Awake()
    {
        EnemyData = ScriptableObject.CreateInstance<SO_EnemyData_LeapingCrocodile>();
        
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);

        CrocData.LeapSpeedMultiplier = 0f;
    }

    /// <summary> Emerges the enemy from the River </summary>
    public override void EmergeFromRiver()
    {
        Debug.Log($"{name} has emerged!");

        // MoveToSpace(boatEnterData.targetBoatSide, boatEnterData.targetSpace);
        SetDirection(boatEnterData.boardingMoveDirection);
        
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
        _currentEmergeTime = 0f;

        CrocSc.EnterBoat(false);

        // TEMP: Forcing the Crocodile to a space until leap and target values are available
        CrocSc.GoToSideSpace
            (CrocSc.boatEnterData.targetSideSpace, CrocSc.boatEnterData.targetLeftSide); // TODO: Add variables to enemies to define what side they should emerge from
    }

    public override void OnExit()
    {
        base.OnExit();
        // Revoke access to outer sides after they've landed on the boat
        CrocSc.canAccessOuterBoatSides = false;

        // Set the direction of the enemy upon landing on the boat
        CrocSc.SetDirection(CrocSc.boatEnterData.boardingMoveDirection);

        // Set the enemy to the targeted boat space upon landed
        CrocSc.GoToBoatSpace(CrocSc.CurrentSpace);
    }

    public override void OnHurt()
    {

        base.OnHurt();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        _currentEmergeTime += Time.deltaTime;
        //Debug.Log(_currentEmergeTime);

        if (_currentEmergeTime > CrocSc.EnemyData.TimeToEmerge)
        {
            /* After the emerge wait time is complete, 
             * enter the boats parent and begin leaping towards 
             * the targetted space on the boat
             */

            // TODO: Make the Crocodile leap towards the a targeted space on the boat

            //TODO: Once landed, swap to Moving State
            CrocSc.GoToBoatSpace
                (CrocSc.boatEnterData.targetSideSpace, CrocSc.boatEnterData.targetSpace);

            CrocSc.ChangeState(CrocSc.MovingState);
        }

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
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

        if (!CrocSc.CanMove) return;

        if (CrocSc.IsMoving) // Time nothing if the croc is already moving
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
            if (!CrocSc.CheckAvailableSpaceFromDirection((int)CrocSc.CurrentDirection))
            {
                CrocSc.FlipDirection();
                // TODO: Flip Animation in FlipDirection()
            }
            else
            {
                CrocSc.MoveToSpaceFromDirection((int)CrocSc.CurrentDirection);
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
        if (CharacterSpaceChecks.ScanAreaForDamageableCharacter(direction, Vector3.one, Quaternion.identity, CrocSc.TargetableCharacterLayers))
        {
            CrocSc.ChangeState(CrocSc.AttackState);
        }
    }
}

public class LeapingCrocodile_AttackState : EnemyAttackState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;

    private float _currentWaitTime;
    private bool _hasAttacked;

    public override void OnEnter()
    {
        base.OnEnter();
        _currentWaitTime = 0f;
        _hasAttacked = false;
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

        if (!_hasAttacked)
        {
            _currentWaitTime += Time.deltaTime;

            if (_currentWaitTime > CrocSc.CrocData.AttackDelay)
            {
                var direction = (int)CrocSc.CurrentDirection * -1 * CrocSc.EnemyData.AttackDistance * Vector3.right + CrocSc.transform.position;
                var player = CharacterSpaceChecks.ScanAreaForDamageableCharacter(direction, Vector3.one, Quaternion.identity, CrocSc.TargetableCharacterLayers);

                _hasAttacked = true;
                Sc.Animator.SetTrigger("Attack");

                if (player != null)
                {
                    _currentWaitTime = 0;
                    player.GetComponent<IDamageable>().TakeDamage();
                    Debug.Log("Damaged Player");
                }
            }
        }
        else // Has attacked. Do cooldown and move back to Moving State
        {
            _currentWaitTime += Time.deltaTime;

            if (_currentWaitTime > CrocSc.CrocData.AttackCooldown)
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
