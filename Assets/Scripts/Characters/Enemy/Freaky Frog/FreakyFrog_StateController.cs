using System.Collections;
using Autodesk.Fbx;
using UnityEngine;

public class FreakyFrog_StateController : BoatEnemyStateController
{
    /*
     * ==========================================================
     * The State Machine controller for the Freaky Frog Enemy
     * ==========================================================
     */
    
    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;

        ChangeState(IdleState);
        
        AttackDelay = new WaitForSeconds(FrogData.attackAtTime);
        AttackAtTime = new WaitForSeconds(FrogData.attackCooldown);
        AttackDelay =  new WaitForSeconds(FrogData.attackDelay);
        StepCooldown = new WaitForSeconds(FrogData.coolDownPerStep);
        DelayBeforeStep = new WaitForSeconds(FrogData.delayBeforeStep);

        GroundDelay = new WaitUntil(() => isGrounded);
    }
    
    [Header("Freaky Frog Data")]
    [SerializeField] protected SO_EnemyData_FreakyFrog frogData;
    public SO_EnemyData_FreakyFrog FrogData => frogData;
    
    public override EnemyIdleState IdleState { get; } = new FreakyFrog_IdleState();
    public override EnemyEmergeState EmergeState { get; } =  new FreakyFrog_EmergeState();
    public override EnemyMovingState MovingState { get; } =  new FreakyFrog_MovingState();
    public override EnemyAttackState AttackState { get; } = new FreakyFrog_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } =  new FreakyFrog_DefeatedState();

    /// <summary> Determines whether the frog will attack on the lane opposite to its current lane to attack its
    /// target. </summary>
    protected bool AttackLane;
    /// <summary> The Space Data the Frog will attack </summary>
    protected Boat_Space_Manager.BoatSide.SpaceData AttackTargetSD;
    
    // Time:
    protected WaitForSeconds AttackDelay;
    protected WaitForSeconds AttackAtTime;
    protected WaitForSeconds AttackCooldown;
    protected WaitForSeconds StepCooldown;
    protected WaitForSeconds DelayBeforeStep;
    
    protected WaitUntil GroundDelay;
    
    public override void EmergeFromRiver()
    {
        base.EmergeFromRiver();
        ChangeState(EmergeState);
    }
    
    public class FreakyFrog_IdleState : EnemyIdleState
     {
         public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;
     
         public override void OnEnter()
         {
             base.OnEnter();
             
         }
     
         public override void OnExit()
         {
             base.OnExit();
             FrogSc.AttackLane = false;
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
     
     public class FreakyFrog_EmergeState : EnemyEmergeState
     {
         public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;
         private float _currentEmergeTime = 0f;
     
         public override void OnEnter()
         {
             base.OnEnter();
             
             FrogSc.canAccessOuterBoatSides = true;
             FrogSc.canAccessBoatSpaces = true;
             
             // Go To Side Space on the Boat
             FrogSc.SetDirection(FrogSc.boatEnterData.startFacingDirection, false);
             FrogSc.GoToSideSpace(FrogSc.boatEnterData.targetBoatSide, FrogSc.boatEnterData.targetLeftSide);
             
             _currentEmergeTime = 0f;
             FrogSc.EnterBoat(false);
             
             FrogSc.Animator.SetTrigger("Emerge");

             FrogSc.StartCoroutine(EmergeRoutine());
         }
         
         private IEnumerator EmergeRoutine()
         {
             yield return new WaitForSeconds(FrogSc.frogData.timeToEmerge);

             // Trigger jump and move towards the space ahead of it
             // FrogSc.SetDirection(FrogSc.boatEnterData.boardingFacingDirection, false);
             FrogSc.MoveToSpaceFromDirection((int)FrogSc.boatEnterData.boardingFacingDirection);
             FrogSc.TriggerJump();

             yield return FrogSc.GroundDelay;
             
             FrogSc.ChangeState(FrogSc.MovingState);
         }
     
         public override void OnExit()
         {
             base.OnExit();
             // Revoke access to outer sides after they've landed on the boat
             FrogSc.canAccessOuterBoatSides = false;
             FrogSc.canAccessBoatSpaces = true;
             
             FrogSc.Animator.SetTrigger("Idle");
     
             // Set the direction of the enemy upon landing on the boat
             // TODO:
             // This doesn't work with the way ground detection works... Exit is getting triggered before the croc
             // Reaches the ground
             
             // FrogSc.SetDirection(FrogSc.boatEnterData.boardingFacingDirection, false);
         }
     
         public override void OnHurt()
         {
     
             base.OnHurt();
         }
     }
     
     public class FreakyFrog_MovingState : EnemyMovingState
     {
         public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;
         private float _currentTimeUntilMove = 0f;
         private float _currentCooldownTime = 0f;
         private bool _stepped;
     
         public override void OnEnter()
         {
             base.OnEnter();
             _currentTimeUntilMove = _currentCooldownTime;
         }

         public override void OnHurt()
         {
             base.OnHurt();
         }
     
         public override void UpdateState()
         {
             base.UpdateState();
     
             if (!FrogSc.canMove) return;
     
             if (FrogSc.IsMoving) // Time nothing if the croc is already moving
             {
                 _currentTimeUntilMove = 0f;
                 return;
             }
             if (_stepped) // Do Cool down if they've already moved
             {
                 if (_currentCooldownTime < FrogSc.frogData.coolDownPerStep)
                 {
                     _currentCooldownTime += Time.deltaTime;
                 }
                 else
                 {
                     // End Move Cooldown
                     _stepped = false;
                     _currentCooldownTime = 0;
                 }
                 return;
             }
     
             // Progress to the next move
             _currentTimeUntilMove += Time.deltaTime;
     
             if (_currentTimeUntilMove > FrogSc.FrogData.timeUntilStep) // Move the Frog
             {
                 // If blocked, swap current direction
                 if (!FrogSc.CheckAvailableSpaceFromDirection((int)FrogSc.CurrentDirection))
                 {
                     FrogSc.Animator.SetTrigger("Idle");
                     FrogSc.FlipDirection();
                     // TODO: Flip Animation in FlipDirection()
                 }
                 else
                 {
                     FrogSc.Animator.SetTrigger("Move");
                     FrogSc.TriggerJump();
                     FrogSc.MoveToSpaceFromDirection((int)FrogSc.CurrentDirection);
                      //TODO: Trigger Animation in method
                 }
                 _stepped = true;
             }
         }
     
         public override void FixedUpdateState()
         {
             base.FixedUpdateState();
             if (FrogSc.IsMoving) return;
     
             // Detect if the player is in the space ahead of them based on the current facing directions
             var space = Boat_Space_Manager.Instance.GetSpaceFromDirection(FrogSc.CurrentSpace.sideID,
                 FrogSc.CurrentSpace.spaceID, (int)FrogSc.CurrentDirection);
             if (CharacterSpaceChecks.ScanAreaForDamageableCharacter(space.t.position, Vector3.one, 
                     Quaternion.identity, FrogSc.TargetableCharacterLayers))
             {
                 // If target found, move to attack state and target space ahead
                 FrogSc.AttackLane = false;
                 FrogSc.AttackTargetSD = space;
                 FrogSc.ChangeState(FrogSc.AttackState);
                 return;
             }
     
             // Check for the opposite lane of the frog for targets.
             space = Boat_Space_Manager.Instance.GetSpaceFromOppositeLane(FrogSc.CurrentSpace.sideID,
                 FrogSc.CurrentSpace.spaceID);
             if (!CharacterSpaceChecks.ScanAreaForDamageableCharacter(space.t.position, Vector3.one,
                     Quaternion.identity, FrogSc.TargetableCharacterLayers)) return; // Return if no target found
             
             // If target found, move to attack state and target opposite lane
             FrogSc.AttackLane = true;
             FrogSc.AttackTargetSD = space;
             FrogSc.ChangeState(FrogSc.AttackState);
         }
     }
     
     public class FreakyFrog_AttackState : EnemyAttackState
     {
         public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;
         private Boat_Space_Manager.BoatSide.SpaceData _targetSpace;
         private Coroutine _attackRoutine;
         
         public override void OnEnter()
         {
             base.OnEnter();
             // FrogSc.canMove = false;
             _attackRoutine = FrogSc.StartCoroutine(AttackRoutine());
         }

         public override void OnExit()
         {
             base.OnExit();
             FrogSc.StopCoroutine(_attackRoutine);
         }

         // The Frog will first wait to attack, then jump either opposite its side, or forwards depending on where its
         // target was provided 
         private IEnumerator AttackRoutine()
         {
             Sc.Animator.SetTrigger("Prepare Attack");
             yield return FrogSc.AttackDelay;

             // Attack animation will be the 'prepare' animation
             Sc.Animator.SetTrigger("Attack");
             
             // Jump!
             FrogSc.TriggerJump();
             
             // If the frog should attack the opposite lane, vault. Else, just move forward normally
             if (FrogSc.AttackLane) FrogSc.PerformVault(true); // Heavy impact for the sake of juice
             else
             {
                 FrogSc.TriggerJump();
                 FrogSc.MoveToSpaceFromDirection((int)FrogSc.CurrentDirection);
             }
             
             // Note: The frog has its bounce value set to zero, so it will just damage the player if it lands on them.

             // Wait until the frog has become grounded
             yield return FrogSc.GroundDelay;

             // Cooldown after the landing process
             yield return FrogSc.AttackCooldown;

             FrogSc.canMove = true;
             FrogSc.ChangeState(FrogSc.MovingState);
         }
     }
     
     public class FreakyFrog_DefeatedState : EnemyDefeatedState
     {
         public FreakyFrog_StateController FrogSc => Sc as FreakyFrog_StateController;
     
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
}

