using CarterGames.Assets.AudioManager;
using UnityEngine;

public class SewerBat_StateController : BoatEnemyStateController
{
    /*
     * ==========================================================
     * The State Machine controller for the Sewer Bat Enemy
     * ==========================================================
     */

    private void Awake()
    {
        IdleState.Sc = this;
        EmergeState.Sc = this;
        MovingState.Sc = this;
        AttackState.Sc = this;
        DefeatedState.Sc = this;
    }

    [Header("Sewer Bat Data")]
    [SerializeField] private SO_EnemyData_SewerBat batData;
    public SO_EnemyData_SewerBat BatData => batData;

    public override EnemyIdleState IdleState { get; } = new SewerBat_IdleState();
    public override EnemyEmergeState EmergeState { get; } = new SewerBat_EmergeState();
    public override EnemyMovingState MovingState { get; } =  new SewerBat_MovingState();
    public override EnemyAttackState AttackState { get; } = new SewerBat_AttackState();
    public override EnemyDefeatedState DefeatedState { get; } =  new SewerBat_DefeatedState();
    
    public override void EmergeFromRiver()
    {
        ChangeState(EmergeState);
    }

    public override void OnDied()
    {
        base.OnDied();
        AudioManager.Play(Clip.Bat_Crash);
    }

    protected override void OnJumped()
    {
        base.OnJumped();
        AudioManager.Play(Clip.Bat_Dive);
    }

    #region States

    public class SewerBat_IdleState : EnemyIdleState
    {
        public SewerBat_StateController BatSc => Sc as SewerBat_StateController;

        public override void OnEnter()
        {
            base.OnEnter();
            BatSc.gravity = 0f;
            // BatSc.currentY = BatSc.jumpPower;
            BatSc.isAffectedByGravity = false;
            // Temp to prevent Fall/Rise Blend Animation from getting stuck...
            BatSc.animator.SetBool("Grounded", true);
        }

        public override void FixedUpdateState()
        {
            base.FixedUpdateState();
            var space = BatSc.currentSpace;
            if (CharacterSpaceChecks.ScanAreaForDamageableCharacter
                (space.t.position, Vector3.one, Quaternion.identity, BatSc.TargetableCharacterLayers))
                BatSc.ChangeState(BatSc.AttackState);
        }
    }

    public class SewerBat_EmergeState : EnemyEmergeState
    {
        public SewerBat_StateController BatSc => Sc as SewerBat_StateController;
        private float _storedTime;
        private bool _emerged;

        public override void OnEnter()
        {
            base.OnEnter();
            // Disable Artwork
            BatSc.artRoot.gameObject.SetActive(false);
            
            BatSc.isAffectedByGravity = false;
            BatSc.EnterBoat(false);
            BatSc.GoToSideSpace(BatSc.boatEnterData.targetSideSpace, BatSc.boatEnterData.doTargetLeftSide);
            _storedTime = Time.time;
            _emerged = false;
        }

        public override void FixedUpdateState()
        {
            if (Time.time - _storedTime > BatSc.batData.timeToEmerge && !_emerged)
            {
                // Enable Artwork
                BatSc.artRoot.gameObject.SetActive(true);
                BatSc.isAffectedByGravity = true;
                var enterData = BatSc.boatEnterData;
                BatSc.TriggerJump();
                BatSc.MoveToSpace(enterData.targetSideSpace, enterData.targetSpace, true);
                _emerged = true;
            }
            
            // This is the height the bat is expected to reach when it jumps
            // TODO: Rework how flying enemies (the bat) should emerge. Or something like that...
            if (BatSc.currentY > 7f)
            {
                BatSc.ChangeState(BatSc.IdleState);
            }
        }
    }

    public class SewerBat_MovingState : EnemyMovingState
    {
        public SewerBat_StateController BatSc => Sc as SewerBat_StateController;
    }

    public class SewerBat_AttackState : EnemyAttackState
    {
        public SewerBat_StateController BatSc => Sc as SewerBat_StateController;
        private float _storedTime;

        public override void OnEnter()
        {
            base.OnEnter();
            _storedTime = Time.time;
            BatSc.Animator.SetTrigger(PrepareAttackHash);
            // Revert the ground parameter from the Idle State to fix falling animation
            BatSc.animator.SetBool("Grounded", false);
            AudioManager.Play(Clip.Bat_Alert);
        }

        public override void UpdateState()
        {
            base.UpdateState();

            // Once the delay is finished, enable gravity as if the Bat is Diving!
            if (Time.time - _storedTime > BatSc.BatData.attackDelay)
            {
                BatSc.gravity = BatSc.BatData.diveGravity;
                BatSc.isAffectedByGravity = true;
            }

            // Once the Bat has slammed onto the ground, Die
            if (BatSc.IsGrounded) BatSc.HealthComponent.Die();
        }
    }

    public class SewerBat_DefeatedState : EnemyDefeatedState
    {
        public SewerBat_StateController BatSc => Sc as SewerBat_StateController;
    }
    #endregion
}