using System.Collections;
using EditorAttributes;
using UnityEngine;
using static Boat_Space_Manager.BoatSide;

namespace GameCharacters
{
    /// <summary>
    /// The class for characters that can move on the boat
    /// </summary>
    public class BoatCharacter : Character
    {
        #region Variables
        [Title("Boat Character")]
        [Line(GUIColor.Gray)]
        
        [Header("Vault Movement")]
        [SerializeField] protected bool canVault = true;
        [SerializeField, ShowField(nameof(canVault))] protected float vaultTime = .5f;
        [SerializeField, ShowField(nameof(canVault))] protected AnimationCurve vaultCurve;
        [ReadOnly, ShowField(nameof(canVault))] public bool isVaulting;
        [ReadOnly, ShowField(nameof(canVault))] public bool isVaultingHeavily;

        protected float VaultTimeElapsed = 0f;

        [Header("Jump Movement")]
        [SerializeField] protected bool canJump = true;
        [SerializeField, ShowField(nameof(canJump))] protected float jumpPower = 10f;
        [ShowField(nameof(canJump))] public bool isJumping; 
        
        protected float JumpTimeElapsed = 0f;
        
        [Header("Head Bounce")]
        [ReadOnly] public bool isBouncing = false;
        [SerializeField] protected float bouncePower = 5f;

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

        [Space]

        [Tooltip("Determines whether the character can move to and stand on the outer spaces of the boat")]
        public bool canAccessOuterBoatSides = false;
        [Tooltip("Determines whether the character can move to and stand on the spaces of the boat")]
        public bool canAccessBoatSpaces = true;
        
        [Header("Boat Interaction")]
        [SerializeField] protected bool canInteractWithBoat = true;
        [SerializeField, ShowField(nameof(canInteractWithBoat))] protected Character_Boat_Interactor boatInteractor;

        #endregion

        #region Space Movement Methods

        /// <summary> Set the character to target a given space </summary>
        protected void TargetSpace(SpaceData targetSpace)
        {
            if (currentSpace != null) currentSpace.isOccupied = false;

            // First Time Targeting space null prevention
            if (currentSpace.t == null) currentSpace = targetSpace;
            
            PreviousSpace = currentSpace;
            
            currentSpace = targetSpace;
            currentSpace.isOccupied = true;

            // Assign previous and new target space for movement updates
            TargetedSpace = targetSpace;
        }

        /// <summary> Moves the character to a space on the boat via a given side and space </summary>
        public void MoveToSpace(int side, int space)
        {
            var sd = Boat_Space_Manager.Instance.GetSpace(side, space);

            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessOuterBoatSides, sd))
            {
                return;
            }
            if (!isVaulting && canMove)
            {
                TargetSpace(sd);
                isMoving = true;
            }
        }

        /// <summary> Moves the character to a space on the boat via a given direction </summary>
        public void MoveToSpaceFromDirection(int direction)
        {
            var sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID, direction);

            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd) && !isVaulting && canMove)
            {
                // print($"Moving to Space: {sd.spaceID}");
                TargetSpace(sd);
                isMoving = true;
            }
            // else print($"Couldn't access space: {sd.spaceID}");
        }

        /// <summary> Vaults the character to a given side and space </summary>
        public void VaultToSide(SpaceData spaceData, bool isHeavy)
        {
            // Check if the character can access that space
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
            {
                if (!isVaulting && !isMoving && canVault)
                {
                    TargetSpace(spaceData);
                    isVaulting = true;
                    isVaultingHeavily = isHeavy;
                }
            }
        }

        /// <summary>
        /// Vault to the side with a provided space data with an additional character to attack upon landing
        /// </summary>
        public void VaultToSide(SpaceData spaceData, bool isHeavy, Character victim)
        {
            // Check if the character can access that space
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
            {
                if (!isVaulting && !isMoving && canVault)
                {
                    TargetSpace(spaceData);
                    isVaulting = true;
                    isVaultingHeavily = isHeavy;
                }
            }
        }

        /// <summary> Sends the character directly to the position of the specified space on a given side </summary>
        public void GoToSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else rb.MovePosition(sd.t.position);
            }

        }

        /// <summary> Sends the character directly to the position of the specified Side Space on a given space </summary>
        public void GoToSideSpace(int side, bool goLeftSide = true)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSideSpace(side, goLeftSide);
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                // TODO: Consider this. Character might be off the boat if they're going to a side space
                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else rb.MovePosition(sd.t.position);
                
                TargetSpace(sd);
                rb.isKinematic = true;
            }
        }

        /// <summary> Sends the character directly to the position of the specified space on the Boat </summary>
        public void GoToBoatSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(side, space);
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                // TODO: Consider this. Character might be off the boat if they're going to a side space
                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else rb.MovePosition(sd.t.position);
            }
        }
        
        public void GoToBoatSpace(SpaceData spaceData)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(spaceData.sideID, spaceData.spaceID);
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                // TODO: Consider this. Character might be off the boat if they're going to a side space
                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else rb.MovePosition(sd.t.position);
            }
        }

        /// <summary> Returns whether the next space is available to go to </summary>
        public bool CheckAvailableSpaceFromDirection(int direction)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID, direction);
            //print($"Checked space: {sd.spaceID}");
            return Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd);
        }
        #endregion

        #region Time Updates
        protected override void FixedTimeUpdate()
        {
            if (!currentSpace.t)  return;

            // Check for grounded
            GetVerticalDistanceFromSpace();

            // If falling, check for characters underneath this character to stomp!
            if (!isGrounded)
            {
                var targetCharacter = 
                    CharacterSpaceChecks.ScanAreaForDamageableCharacter
                    (
                        StompPosition.position,
                        new Vector3 (1f, .25f, 1f),
                        Quaternion.identity,
                        TargetableCharacterLayers
                    );

                if (targetCharacter)
                {
                    TriggerBounce();
                    targetCharacter.GetComponent<IDamageable>().TakeDamage(DamageType.Stomp);
                }
            }
            
            // Check if Moving. Do Movement!
            if (canMove && isMoving && !isVaulting)
            {
                SpaceMovement();
                return;
            }
            
            // Check if Vaulting. Do Vaulting!
            if (canVault && isVaulting) //TODO: Implement the ability to vault whilst jumping
            {
                VaultMovement();
                return;
            }
            // Is just grounded. Stay at current space.
            var pos = currentSpace.t.position;
            rb.MovePosition(isGrounded && !isBouncing ? pos : new Vector3(pos.x, rb.position.y, pos.z));
        }

        #endregion

        #region Movement Updates
        
        /// <summary>
        /// The movement of this character towards their targeted boat space
        /// </summary>
        protected virtual void SpaceMovement()
        {
            // Whilst moving
            if (MovementTimeElapsed < 1f)
            {
                // Lerp from the previous space to the new target space
                Vector3 pos;
                if (isGrounded)
                {
                    // Grounded Movement
                    pos = Vector3.Lerp
                        (PreviousSpace.t.position, TargetedSpace.t.position,
                            groundedMovementCurve.Evaluate(MovementTimeElapsed)
                        );
                    rb.MovePosition(pos);
                    
                }
                else
                {
                    // Air Movement
                    pos = Vector3.Lerp
                        (PreviousSpace.t.position, TargetedSpace.t.position,
                            airMovementCurve.Evaluate(MovementTimeElapsed)
                        );
                    rb.MovePosition(new Vector3(pos.x, rb.position.y, pos.z));
                }

                // Coyote Time
                if (MovementTimeElapsed < coyoteTime && coyoteTriggered)
                {
                    //TODO:
                    //establish a movement override method for quickly
                    //changing Boat Space targets if it's changed during the movement cycle
                
                    coyoteTriggered = false;
                
                    return;
                }
            }
            else
            {
                isMoving = false;
                MovementTimeElapsed = 0f;

                rb.MovePosition
                    (isGrounded ?
                        TargetedSpace.t.position
                        :
                        new Vector3(TargetedSpace.t.position.x, rb.position.y, TargetedSpace.t.position.z
                    ));
                
                return;
            }
            
            MovementTimeElapsed += Time.fixedDeltaTime / (isGrounded? groundedMovementTime : airMovementTime);
        }

        protected virtual void VaultMovement()
        {
            //TODO:

            if (VaultTimeElapsed < 1f)
            {
                VaultTimeElapsed += Time.fixedDeltaTime / vaultTime;

                Vector3 pos;
                if (isGrounded)
                {
                    pos = Vector3.Lerp
                        (PreviousSpace.t.position, TargetedSpace.t.position,
                        vaultCurve.Evaluate(VaultTimeElapsed)
                        );
                    rb.MovePosition(pos);
                }
                else
                {
                    pos = Vector3.Lerp
                    (PreviousSpace.t.position, TargetedSpace.t.position,
                        vaultCurve.Evaluate(VaultTimeElapsed)
                    );
                    rb.MovePosition(new Vector3(pos.x, rb.position.y, pos.z));
                }

                //TODO: An optional art thing you could do is attach the characters hands to the wall as they vault?

                // Coyote Time
                if (VaultTimeElapsed < coyoteTime && coyoteTriggered)
                {
                    //TODO:
                    //establish a vault override method for quickly
                    //changing Boat Space targets if it's changed during the vault cycle

                    coyoteTriggered = false;

                    return;
                }
            }
            else
            {
                isVaulting = false;
                
                VaultTimeElapsed = 0f;
                var pos = currentSpace.t.position;
                rb.MovePosition(isGrounded? pos : new Vector3(pos.x, rb.position.y, pos.z));

                if (!isGrounded) return;

                if (isVaultingHeavily && canInteractWithBoat)
                {
                    isVaultingHeavily = false;
                    boatInteractor.ImpactBoat(TargetedSpace);
                }
            }
        }
        #endregion

        private float GetVerticalDistanceFromSpace()
        {
            var dist = rb.position.y - currentSpace.t.position.y;
            isGrounded = Mathf.Approximately(dist, 0f);
            verticalDistance = dist;

            return dist;
        }

        public void TriggerJump()
        {
            if (!isGrounded) return;
            rb.isKinematic = false;
            StartCoroutine(JumpWaitRoutine());
        }

        private IEnumerator JumpWaitRoutine()
        {
            yield return PauseWait; // Time Behaviour Pause
            yield return new WaitUntil(() => !isVaulting);
            
            isJumping = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            
            OnJumped();
            
            if (_waitGroundedRoutine != null) yield break;
            _waitGroundedRoutine = StartCoroutine(WaitUntilGroundedRoutine());
        }

        /// <summary> Is called before the character jumps </summary>
        protected virtual void OnJumped()
        {
            animator.SetTrigger("Jump");
            //TODO: Add Jump SFX and VFX
        }

        /// <summary> Called whenever this character lands </summary>
        protected virtual void OnLanded()
        {
            rb.isKinematic = true; // TODO: Consider
            animator.SetTrigger("Landed");
            //TODO: Add landed SFX and VFX
        }

        public void TriggerBounce()
        {
            TriggerHitStop();
            isBouncing = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * bouncePower, ForceMode.Impulse);

            if (_waitGroundedRoutine != null) return;
            _waitGroundedRoutine = StartCoroutine(WaitUntilGroundedRoutine());
        }
        
        private Coroutine _waitGroundedRoutine;

        private IEnumerator WaitUntilGroundedRoutine()
        {
            yield return PauseWait; // Time Behaviour Pause
            yield return new WaitForSeconds(.1f);
            yield return new WaitUntil(() => isGrounded);

            // Unnecessary?
            if (isJumping)
            {
                isJumping = false;
            }
            if (isBouncing)
            {
                isBouncing = false;
            }

            if (isVaultingHeavily && canInteractWithBoat)
            {
                isVaultingHeavily = false;
                boatInteractor.ImpactBoat(TargetedSpace);
            }
            
            OnLanded();
            _waitGroundedRoutine = null;
        }

        #region Boat Entering Methods
        /// <summary>
        /// Method to make the character enter the boats parent
        /// </summary>
        public void EnterBoat(bool goToCurrentSpace)
        {
            //Boat_Space_Manager.Instance.AddPassenger(this); //TODO: Intergrate BoatCharacter to the BoatSpaceManager
            isOnBoat = true;
            if (goToCurrentSpace) GoToSpace(currentSpace.sideID, currentSpace.spaceID);
        }

        /// <summary>
        /// Method to make the character exit the boats parent
        /// </summary>
        public void ExitBoat(bool goToCurrentSpace)
        {
            //Boat_Space_Manager.Instance.RemovePassenger(this); //TODO: Intergrate BoatCharacter to the BoatSpaceManager
            isOnBoat = false;
            if (goToCurrentSpace) MoveToSpace(currentSpace.sideID, currentSpace.spaceID);
        }
        #endregion
    }
}

