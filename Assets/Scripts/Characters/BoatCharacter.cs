using CameraShake;
using CarterGames.Assets.AudioManager;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static Boat_Space_Manager.BoatSide;

namespace GameCharacters
{
    /// <summary>
    /// The class for characters that can move on the boat
    /// </summary>
    public class BoatCharacter : Character
    {
        // Animator Cached Properties
        private static readonly int AnimatorGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimatorMoving = Animator.StringToHash("Moving");
        private static readonly int AnimatorMoved = Animator.StringToHash("Moved");
        private static readonly int AnimatorVaulting = Animator.StringToHash("Vaulting");
        private static readonly int AnimatorVaulted = Animator.StringToHash("Vaulted");
        private static readonly int AnimatorGroundPound = Animator.StringToHash("Hard Action");
        private static readonly int AnimatorGroundPounded = Animator.StringToHash("Ground Pound");
        private static readonly int AnimatorGroundPoundedPrepare = Animator.StringToHash("Ground Pound Prepare");
        private static readonly int AnimatorLanded = Animator.StringToHash("Landed");
        private static readonly int AnimatorVelocity = Animator.StringToHash("Vertical Velocity");
        private static readonly int AnimatorJump = Animator.StringToHash("Jump");

        #region Variables

        [Title("Boat Character")] [Line(GUIColor.Gray)] [Header("Vault Movement")] 
        [SerializeField]
        protected bool canVault = true;
        [SerializeField, ShowField(nameof(canVault))]
        protected float vaultTime = .5f;
        [SerializeField, ShowField(nameof(canVault))]
        protected AnimationCurve vaultCurve;
        [ReadOnly, ShowField(nameof(canVault))]
        public bool isVaulting;
        [FormerlySerializedAs("isVaultingHeavily")] [ReadOnly, ShowField(nameof(canVault))]
        public bool isGroundPounding;
        [SerializeField] protected float groundPoundTime = .25f;
        protected float GroundPoundElapsed;

        [Header("Coyote Inputs")] 
        protected bool WillMove = false;
        protected new int MoveDirection;
        protected bool WillVault = false;
        protected bool WillJump = false;
        protected float VaultTimeElapsed = 0f;

        [Header("Jump Movement")] 
        [SerializeField] protected bool canJump = true;
        [FormerlySerializedAs("jumpPower")] [SerializeField, ShowField(nameof(canJump))] protected float jumpHeight = 10f;
        [ShowField(nameof(canJump))] public bool isJumping;
        protected float JumpTimeElapsed = 0f;
        public bool isAffectedByGravity = true;
        [SerializeField] protected bool isFastFalling = false;
        [SerializeField] private float gravityScale = 1f;
        [SerializeField] protected float minimumGravity = -15f;
        [SerializeField] protected float maximumGravity = -25f;

        [ShowField(nameof(canJump))] public bool isFalling;
        [HideInInspector] public float verticalVelocity;
        [SerializeField, ReadOnly] protected float currentY;

        [Header("Head Bounce")] [ReadOnly]
        public bool isBouncing = false;
        [FormerlySerializedAs("bouncePower")] [SerializeField] protected float bounceHeight = 5f;
        private float _timeSinceLastBounce;
        [SerializeField] private bool canBounce = true;

        [Header("Space Information")]
        [Tooltip("The current space on the boat this character is on")]
        [SerializeField, ReadOnly] protected SpaceData currentSpace;
        public SpaceData CurrentSpace => currentSpace;
        [ReadOnly] public bool isOnBoat;

        /* Variables for lerping target space movement */
        /// <summary> The space on the boat this character is currently moving towards </summary>
        protected SpaceData TargetedSpace;

        /// <summary> The space on the boat this character was previously at before the next targeted space </summary>
        protected SpaceData PreviousSpace;

        /// <summary> The character to damage upon finishing a movement action </summary>
        protected Character TargetedCharacter;

        [Space] [Tooltip("Determines whether the character can move to and stand on the outer spaces of the boat")]
        public bool canAccessOuterBoatSides = false;

        [Tooltip("Determines whether the character can move to and stand on the spaces of the boat")]
        public bool canAccessBoatSpaces = true;

        [Header("Boat Interaction")] [SerializeField]
        protected bool canInteractWithBoat = true;

        [SerializeField, ShowField(nameof(canInteractWithBoat))]
        protected Character_Boat_Interactor boatInteractor;

        #endregion

        #region Space Movement Methods

        /// <summary> Set the character to target a given space </summary>
        protected void TargetSpace(SpaceData targetSpace)
        {
            // Exit previous space, only when grounded
            if (isGrounded) currentSpace?.ExitSpace();

            // First Time Targeting space null prevention
            currentSpace ??= targetSpace;

            PreviousSpace = currentSpace;

            currentSpace = targetSpace;
            // Only occupy spaces when grounded
            if (isGrounded) currentSpace.EnterSpace();

            // Assign previous and new target space for movement updates
            TargetedSpace = targetSpace;
        }

        /// <summary> Moves the character to a space on the boat via a given side and space </summary>
        public void MoveToSpace(int side, int space, bool ignoreChecks = false)
        {
            var sd = Boat_Space_Manager.Instance.GetSpace(side, space);

            // Check space access, bypass if not grounded
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess
                    (canAccessOuterBoatSides, canAccessOuterBoatSides, sd, !isGrounded || ignoreChecks))
            {
                Debug.Log($"Couldn't Move {name} to space: {sd.spaceID}, Side: {sd.sideID}");
                return;
            }

            if (isVaulting || !canMove)
            {
                Debug.Log($"Couldn't Move {name} to space. Vaulting = {isVaulting}. Can Move = {canMove}");
                return;
            }
            TargetSpace(sd);
            isMoving = true;
            OnMoved();
        }

        /// <summary> Moves the character to a space on the boat via a given direction </summary>
        public void MoveToSpaceFromDirection(int direction)
        {
            var sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID,
                direction);
            SetDirection((MoveDirection)direction, false);

            // Check space access, ignore bypass if grounded
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd, !isGrounded) &&
                !isVaulting && canMove)
            {
                TargetSpace(sd);
                isMoving = true;

                OnMove();
            }
            // print($"Moving to Space: {sd.spaceID}");
            // else Debug.Log($"{name} Couldn't access space: {sd.spaceID}");

        }

        /// <summary> Called whenever the character has started a move </summary>
        protected virtual void OnMove()
        {
            animator.SetBool(AnimatorMoving, true);
            animator.SetTrigger(AnimatorMoved);
        }

        /// <summary> Called whenever the character has finished a move </summary>
        protected virtual void OnMoved()
        {
            // Move Completed, stop moving parameter
            animator.SetBool(AnimatorMoving, false);
        }

        /// <summary> Vaults the character to a given side and space </summary>
        public void VaultToSide(SpaceData spaceData)
        {
            // Check if the character can access that space
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
                return;
            // Skip if already vaulting or moving
            if (isVaulting || isMoving || !canVault) return;
            TargetSpace(spaceData);
            isVaulting = true;

            // Trigger the OnVault Method for Animations + Sounds
            OnVault();
        }

        /// <summary> Vault to the side with a provided space data with an additional character to attack upon landing </summary>
        public void VaultToSide(SpaceData spaceData, Character victim)
        {
            VaultToSide(spaceData);
            TargetedCharacter = victim;
        }

        /// <summary> Sends the character directly to the position of the specified space on a given side </summary>
        public void GoToSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd)) return;
            TargetSpace(sd);

            if (isOnBoat) transform.localPosition = sd.t.localPosition;
            else transform.position = sd.t.position;

        }

        /// <summary> Sends the character directly to the position of the specified Side Space on a given space </summary>
        public void GoToSideSpace(int side, bool goLeftSide = true)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSideSpace(side, goLeftSide);

            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd)) return;
            // TODO: Consider this. Character might be off the boat if they're going to a side space
            if (isOnBoat) transform.localPosition = sd.t.localPosition;
            else transform.position = sd.t.position;

            TargetSpace(sd);
        }

        /// <summary> Sends the character directly to the position of the specified space on the Boat </summary>
        public void GoToBoatSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(side, space);
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd)) return;

            TargetSpace(sd);

            // TODO: Consider this. Character might be off the boat if they're going to a side space
            if (isOnBoat) transform.localPosition = sd.t.localPosition;
            else transform.position = sd.t.position;
        }

        public void GoToBoatSpace(SpaceData spaceData)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(spaceData.sideID, spaceData.spaceID);
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd)) return;

            TargetSpace(sd);

            // TODO: Consider this. Character might be off the boat if they're going to a side space
            if (isOnBoat) transform.localPosition = sd.t.localPosition;
            else transform.position = sd.t.position;
        }

        /// <summary> Returns whether the next space is available to go to </summary>
        public bool CheckAvailableSpaceFromDirection(int direction)
        {
            SpaceData sd =
                Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID, direction);
            //print($"Checked space: {sd.spaceID}");
            return Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd, false);
        }

        #endregion

        #region Time Updates

        protected override void TimeUpdate()
        {
            base.TimeUpdate();
            if (!currentSpace?.t || HealthComponent.IsDead) return;

            VerticalMovement();

            if (canMove && isMoving && !isVaulting)
            {
                SpaceMovement();
                if (isGroundPounding && isGrounded)
                {
                    GroundPoundSequence();
                }
            }
            else if (canVault && isVaulting)
            {
                VaultMovement();
            }
            else
            {
                StayOnCurrentSpace();
                if (isGroundPounding && isGrounded)
                {
                    GroundPoundSequence();
                }
            }

            if (isGrounded)
            {
                currentY = 0f;
            }
        }

        #endregion

        #region Movement Updates

        private void StayOnCurrentSpace()
        {
            if (!currentSpace?.t) return;

            Vector3 basePos = currentSpace.t.localPosition;

            transform.localPosition = new Vector3(
                basePos.x,
                basePos.y + currentY,
                basePos.z
            );
        }

        /// <summary>
        /// The movement of this character towards their targeted boat space
        /// </summary>
        protected virtual void SpaceMovement()
        {
            if (MovementTimeElapsed < 1f)
            {
                float t = isGrounded? 
                    groundedMovementCurve.Evaluate(MovementTimeElapsed) 
                    : 
                    airMovementCurve.Evaluate(MovementTimeElapsed);

                Vector3 basePos = Vector3.Lerp(
                    PreviousSpace.t.localPosition,
                    TargetedSpace.t.localPosition,
                    t
                );

                transform.localPosition = new Vector3(
                    basePos.x,
                    basePos.y + currentY,
                    basePos.z
                );
            }
            else
            {
                isMoving = false;
                // WillMove = false;
                MovementTimeElapsed = 0f;

                Vector3 basePos = TargetedSpace.t.localPosition;

                transform.localPosition = new Vector3(
                    basePos.x,
                    basePos.y + currentY,
                    basePos.z
                );

                // Trigger Finished Move Method
                OnMoved();

                // Coyote input for vaulting after moving
                if (WillVault) PerformVault();
            }

            MovementTimeElapsed += Time.deltaTime / (isGrounded? groundedMovementTime : airMovementTime);
        }

        protected void PerformVault()
        {
            if (GameManager.GameLogic.IsGamePaused) return;
            
            WillVault = true;
            isGroundPounding = false;

            var newSpace =
                Boat_Space_Manager.Instance.GetSpaceFromOppositeLane(currentSpace.sideID, currentSpace.spaceID);

            // Vault to space. Additionally, if an enemy is on the opposite side of the space, do an attack vault
            var bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                (newSpace.t.position, Vector3.one, Quaternion.identity, targetableCharacterLayers, true);
            
            if (bc) VaultToSide(newSpace, bc);
            else VaultToSide(newSpace);
        }

        protected virtual void VaultMovement()
        {
            // Do the Vault movement
            if (VaultTimeElapsed < 1f)
            {
                VaultTimeElapsed += Time.deltaTime / vaultTime;

                float t = vaultCurve.Evaluate(VaultTimeElapsed);
                Vector3 pos = Vector3.Lerp(PreviousSpace.t.localPosition, TargetedSpace.t.localPosition, t);
                transform.localPosition = new Vector3(pos.x, pos.y + currentY, pos.z);
            }
            // Vaulted Ended
            else
            {
                isVaulting = false;
                WillVault = false;

                VaultTimeElapsed = 0f;
                transform.localPosition = TargetedSpace.t.localPosition + Vector3.up * currentY;

                if (!isJumping) OnVaulted();
                else
                {
                    animator.ResetTrigger("Vaulted");
                    animator.SetBool(AnimatorVaulting, false);
                }
                
                // Damage any targeted characters
                var bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                    (currentSpace.t.position, Vector3.one, Quaternion.identity,
                        damageableCharacterLayers, true);
                if (bc)
                {
                    TargetedCharacter = bc;
                    TargetedCharacter.GetComponent<IDamageable>().TakeDamage();
                    OnTargetEliminated();
                    TargetedCharacter = null;
                }
                
                // If coyote move action true, move
                if (WillMove)
                {
                    MoveToSpaceFromDirection(MoveDirection);
                    WillMove = false;
                }

                // Trigger Vaulting Ground Pound
                if (isGroundPounding && canInteractWithBoat && isGrounded && !WillJump)
                {
                    OnGroundPound();
                    // animator.SetBool("Hard Action", true);
                }
                
                // If coyote jump action true, jump
                if (WillJump)
                {
                    // Debug.Log($"Will Jump. Current height is: {_currentY}. Grounded = {IsGrounded}");
                    TriggerJump();
                }
            }
        }

        protected virtual void OnVault()
        {
            //TODO: Flip the direction of the character based on the side of the boat they're vaulting from
            animator.SetBool(AnimatorVaulting, true);
            animator.SetTrigger(AnimatorVaulted);
            animator.SetBool(AnimatorGroundPound, isGroundPounding);
        }

        protected virtual void OnVaulted()
        {
            animator.SetBool(AnimatorVaulting, false);
            // animator.ResetTrigger("Vaulted");
            animator.SetTrigger(AnimatorLanded); //TODO: add "Vault Landed" animation!
        }

        protected virtual void TriggerGroundPound()
        {
            isGroundPounding = true;
            GroundPoundElapsed = 0f;
            animator.SetBool(AnimatorGroundPound, isGroundPounding);
            OnGroundPoundTriggered();
        }

        protected virtual void CancelGroundPound()
        {
            isGroundPounding = false;
            GroundPoundElapsed = 0f;
            animator.SetBool(AnimatorGroundPound, isGroundPounding);
        }
        
        protected virtual void GroundPoundSequence()
        {
            if (GroundPoundElapsed < groundPoundTime)
            {
                GroundPoundElapsed += Time.deltaTime;
            }
            else
            {
                OnGroundPound();
            }
        }

        protected virtual void OnGroundPoundTriggered()
        {
            if (isGrounded) animator.SetTrigger(AnimatorGroundPoundedPrepare);
        }

        protected virtual void OnGroundPound()
        {
            CameraShaker.Presets.Explosion3D();
            boatInteractor.ImpactBoat(TargetedSpace);
            GroundPoundElapsed = 0f;
            isGroundPounding = false;
            animator.SetBool(AnimatorGroundPound, isGroundPounding);
            animator.SetTrigger(AnimatorGroundPounded);
        }

        #endregion

        private void VerticalMovement()
        {
            if (isGrounded) return;

            if (isAffectedByGravity)
            {
                // Do Gravity (fall)
                verticalVelocity += (isFastFalling? maximumGravity : minimumGravity) * Time.deltaTime;
                currentY += verticalVelocity * Time.deltaTime;
            }


            // Set Vertical Falling Animation Parameter
            animator.SetFloat(AnimatorVelocity, verticalVelocity);

            // Damage any Characters below! Only when falling
            if (verticalVelocity < 0)
            {
                var character = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                    (StompPosition.position, stompSize, transform.rotation, damageableCharacterLayers, true);
                if (character)
                {
                    if (character == this) return; // Prevent Self Damage
                    var damageableCharacter = character.GetComponent<IDamageable>();
                    if (!damageableCharacter.IsInvincible)
                    {
                        // TODO
                        // Temp solution to instant kill / Constantly updating OnTargetEliminated method
                        OnTargetEliminated();
                        damageableCharacter.TakeDamage();
                    }
                    TriggerBounce();
                }
            }

            // Detect Landing
            if (!(currentY <= 0f)) return;
            currentY = 0f;
            verticalVelocity = 0f;
            isGrounded = true;
            isFastFalling = false;

            OnLanded();
        }

        protected void TriggerJump()
        {
            WillJump = false;
            // Return if already in the air
            if (!isGrounded) return;
            
            isJumping = true;
            isGrounded = false;
            verticalVelocity = Mathf.Sqrt(jumpHeight * (Physics.gravity.y * gravityScale) * -2f);
            currentY = 0.1f;

            OnJumped();
        }

        protected void StopJump()
        {
            isFastFalling = true;
        }

        /// <summary> Is called before the character jumps </summary>
        protected virtual void OnJumped()
        {
            animator.SetTrigger(AnimatorJump);
            animator.SetBool(AnimatorGrounded, isGrounded);
            
            currentSpace.ExitSpace();

            // if (isGroundPounding && canInteractWithBoat) boatInteractor.ImpactBoat(TargetedSpace);

            // Debug.Log($"PERFORMED Jump. Current height is: {_currentY}. Grounded = {IsGrounded}");
        }

        /// <summary> Called whenever this character lands </summary>
        protected virtual void OnLanded()
        {
            isJumping = false;
            isBouncing = false;
            
            currentSpace.EnterSpace();
            
            animator.SetTrigger(AnimatorLanded);
            animator.SetBool(AnimatorGrounded, true);
            animator.SetBool(AnimatorGroundPound, isGroundPounding);

            //TODO: Add landed SFX and VFX

            if (isGroundPounding)
            {
                if (canInteractWithBoat)
                {
                    OnGroundPound();
                    // TriggerBounce(); // For fun! (can be pretty inconvenient sometimes, disabling)
                }
                if (!isVaulting) isGroundPounding = false;
            }
            else CameraShaker.Presets.ShortShake3D();
            // Debug.Log($"Landed. Grounded = {IsGrounded} Height = {_currentY}");
        }

        protected virtual void OnTargetEliminated()
        {
            
        }

        public void TriggerBounce()
        {
            if (!canBounce) return;
            
            currentSpace.ExitSpace();
            isGrounded = false;
            animator.SetBool(AnimatorGrounded, isGrounded);
            verticalVelocity = Mathf.Sqrt(bounceHeight * (Physics.gravity.y * gravityScale) * -2f);;
            isBouncing = true;

            _timeSinceLastBounce = Time.time;
        }

        #region Boat Entering Methods

        /// <summary>
        /// Method to make the character enter the boats parent
        /// </summary>
        public void EnterBoat(bool goToCurrentSpace)
        {
            Boat_Space_Manager.Instance.AddPassenger(this);
            isOnBoat = true;
            transform.localScale = Vector3.one;
            SetDirection(currentDirection, false);
            if (goToCurrentSpace) GoToSpace(currentSpace.sideID, currentSpace.spaceID);
        }

        /// <summary>
        /// Method to make the character exit the boats parent
        /// </summary>
        public void ExitBoat()
        {
            Boat_Space_Manager.Instance.RemovePassenger(this);
            isOnBoat = false;
            transform.localScale = Vector3.one;
            SetDirection(currentDirection, false);
            // if (goToCurrentSpace) MoveToSpace(currentSpace.sideID, currentSpace.spaceID);
        }

        #endregion

        #region Damage Events

        public override void OnDied(DamageType damageType)
        {
            base.OnDied(damageType);
            currentSpace?.ExitSpace();
        }

        #endregion

        public void ResetCharacter()
        {
            // ExitBoat(false);

            currentSpace = null;

            isJumping = false;
            isMoving = false;
            isBouncing = false;
            
            // Reset Direction and Position
            SetDirection(currentDirection, false);
            transform.localPosition = Vector3.zero;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(StompPosition.position, stompSize);
        }
    }
} 