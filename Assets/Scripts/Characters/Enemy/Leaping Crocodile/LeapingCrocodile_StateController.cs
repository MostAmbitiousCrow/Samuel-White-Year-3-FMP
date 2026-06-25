using System.Collections;
using CarterGames.Assets.AudioManager;
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

    #endregion
    
    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);
        
        AttackDelay = new WaitForSeconds(CrocData.attackAtTime);
        AttackAtTime = new WaitForSeconds(CrocData.attackCooldown);
        AttackDelay =  new WaitForSeconds(CrocData.attackDelay);
        StepCooldown = new WaitForSeconds(CrocData.coolDownPerStep);
        DelayBeforeStep = new WaitForSeconds(CrocData.delayBeforeStep);

        GroundDelay = new WaitUntil(() => isGrounded);
    }
    
    // Time:
    protected WaitForSeconds AttackDelay;
    protected WaitForSeconds AttackAtTime;
    protected WaitForSeconds AttackCooldown;
    protected WaitForSeconds StepCooldown;
    protected WaitForSeconds DelayBeforeStep;
    
    protected WaitUntil GroundDelay;

    /// <summary> Emerges the enemy from the River </summary>
    public override void EmergeFromRiver()
    {
        base.EmergeFromRiver();
        
        ChangeState(EmergeState);
    }

    protected override void OnMove()
    {
        base.OnMove();
        AudioManager.Play(Clip.Croc_Step_0); //TODO: Add some randomisation
    }

    protected override void OnMoved()
    {
        base.OnMoved();
        AudioManager.Play(Clip.Croc_Step_1); //TODO: Add some randomisation
    }

    protected override void OnLanded()
    {
        base.OnLanded();
        AudioManager.Play(Clip.Croc_Land); //TODO: Add some randomisation
    }

    public class LeapingCrocodile_IdleState : EnemyIdleState
     {
         public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
     }
     
     public class LeapingCrocodile_EmergeState : EnemyEmergeState
     {
         public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
         // private float _currentEmergeTime = 0f;
     
         private Coroutine _emergeRoutine;
         public override void OnEnter()
         {
             base.OnEnter();
             
             CrocSc.canAccessOuterBoatSides = true;
             CrocSc.canAccessBoatSpaces = true;
             
             // Go To Side Space on the Boat
             CrocSc.SetDirection(CrocSc.boatEnterData.startFacingDirection, false);
             CrocSc.GoToSideSpace(CrocSc.boatEnterData.targetSideSpace, CrocSc.boatEnterData.doTargetLeftSide);
             
             // _currentEmergeTime = 0f;
             CrocSc.EnterBoat(false);
             
             CrocSc.Animator.SetTrigger("Emerge");

             // Start the Emerge Routine
             _emergeRoutine = CrocSc.StartCoroutine(EmergeRoutine());
         }

         private IEnumerator EmergeRoutine()
         {
             // Disable Art and Hitbox
             CrocSc.artRoot.gameObject.SetActive(false);
             CrocSc.CharacterCollider.enabled = false;
             yield return new WaitForSeconds(CrocSc.crocData.timeToEmerge);
             // Enable Art and Hitbox
             CrocSc.artRoot.gameObject.SetActive(true);
             CrocSc.CharacterCollider.enabled = true;

             // vvv Forcing this vvv
             CrocSc.canAccessOuterBoatSides = true;
             CrocSc.canAccessBoatSpaces = true;
             CrocSc.canMove = true;
             
             // Trigger leap and move towards 
             CrocSc.MoveToSpace(CrocSc.boatEnterData.targetBoatSide, CrocSc.boatEnterData.targetSpace, true);
             CrocSc.TriggerJump();
             CrocSc.SetDirection(CrocSc.boatEnterData.boardingFacingDirection, true);
             
             AudioManager.Play(Clip.Croc_Leap);

             yield return CrocSc.GroundDelay;
             
             CrocSc.SetDirection(CrocSc.boatEnterData.boardingFacingDirection, false);
             CrocSc.ChangeState(CrocSc.MovingState);
         }
     
         // Scrapping in favour of a coroutine since the movement logic is being inconsistent...
         // public override void UpdateState()
         // {
         //     if (_currentEmergeTime > CrocSc.EmergeDelay)
         //     {
         //         if (!CrocSc.IsGrounded && !CrocSc.isJumping)
         //         {
         //             // Trigger leap and move towards 
         //             CrocSc.MoveToSpace(CrocSc.boatEnterData.targetBoatSide, CrocSc.boatEnterData.targetSpace);
         //             CrocSc.TriggerJump();
         //             CrocSc.SetDirection(CrocSc.boatEnterData.boardingFacingDirection, false);
         //         }
         //         // When the Crocodile has landed on the boat, patrol
         //         else if (CrocSc.isGrounded)
         //         {
         //             CrocSc.ChangeState(CrocSc.MovingState);
         //         }
         //     }
         //     else
         //     {
         //         _currentEmergeTime += Time.deltaTime;
         //     }
         // }
     
         public override void OnExit()
         {
             base.OnExit();
             
             // Revoke access to outer sides after they've landed on the boat
             CrocSc.canAccessOuterBoatSides = false;
             CrocSc.canAccessBoatSpaces = true;
             
             CrocSc.StopCoroutine(_emergeRoutine);
             
             CrocSc.Animator.SetTrigger("Idle");
         }
     }
     
     public class LeapingCrocodile_MovingState : EnemyMovingState
     {
         public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
     
         private float _currentTimeUntilMove = 0f;
         private float _currentCooldownTime = 0f;
         private float _currentDelayTime = 0f;
         private bool _stepped;
     
         public override void OnEnter()
         {
             base.OnEnter();
     
             _currentTimeUntilMove = _currentCooldownTime = _currentDelayTime = 0f;
             _stepped = false;
         }

         public override void UpdateState()
         {
             base.UpdateState();
     
             if (!CrocSc.canMove) return;
     
             if (CrocSc.IsMoving) // Time nothing if the croc is already moving
             {
                 _currentTimeUntilMove = 0f;
                 return;
             }

             if (_stepped) // Do Cooldown if they've already moved
             {
                 if (_currentCooldownTime < CrocSc.CrocData.coolDownPerStep)
                 {
                     _currentCooldownTime += Time.deltaTime;
                 }
                 else
                 {
                     // End Move Cooldown
                     CrocSc.Animator.SetTrigger("Idle");
                     _stepped = false;
                     _currentCooldownTime = 0;
                 }
                 return;
             }

             // Progress to the next move
             _currentTimeUntilMove += Time.deltaTime;
     
             if (_currentTimeUntilMove > CrocSc.CrocData.timeUntilStep) // Move the Croc
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
                 _stepped = true;
     
             }
         }
     
         public override void FixedUpdateState()
         {
             base.FixedUpdateState();
             if (CrocSc.IsMoving) return;
     
             // Detect if the player is in the space ahead of them based on the current facing directions
             var space = Boat_Space_Manager.Instance.GetSpaceFromDirection(CrocSc.CurrentSpace.sideID,
                 CrocSc.CurrentSpace.spaceID, (int)CrocSc.CurrentDirection);
             if (CharacterSpaceChecks.ScanAreaForDamageableCharacter(space.t.position, 
                     Vector3.one, Quaternion.identity, CrocSc.targetableCharacterLayers))
             {
                 CrocSc.ChangeState(CrocSc.AttackState);
             }
         }
     }
     
     public class LeapingCrocodile_AttackState : EnemyAttackState
     {
         public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
         private Coroutine _attackRoutine;
         
         public override void OnEnter()
         {
             base.OnEnter();
             _attackRoutine = CrocSc.StartCoroutine(AttackRoutine());
         }

         public override void OnExit()
         {
             base.OnExit();
             CrocSc.StopCoroutine(_attackRoutine);
         }

         public override void OnHurt()
         {
             base.OnHurt();
             CrocSc.ChangeState(CrocSc.MovingState);
         }

         private IEnumerator AttackRoutine()
         {
             Sc.Animator.SetTrigger("Prepare Attack");
             AudioManager.Play(Clip.Croc_Attack);
             yield return new WaitForSeconds(CrocSc.CrocData.attackDelay);
             
             Sc.Animator.SetTrigger("Attack");
     
             yield return new WaitForSeconds(CrocSc.CrocData.attackAtTime);
             
             var space = Boat_Space_Manager.Instance.GetSpaceFromDirection(CrocSc.CurrentSpace.sideID,
                 CrocSc.CurrentSpace.spaceID, (int)CrocSc.CurrentDirection);
             var target = CharacterSpaceChecks.ScanAreaForDamageableCharacter
             (space.t.position, Vector3.one, Quaternion.identity, CrocSc.damageableCharacterLayers);
     
             if (target)
             {
                 target.GetComponent<IDamageable>().TakeDamage();
                 // Debug.Log("Damaged Player");
             }
     
             yield return new WaitForSeconds(CrocSc.CrocData.attackCooldown);
     
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
     }

}


