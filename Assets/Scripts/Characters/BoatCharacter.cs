using System.Collections;
using CameraShake;
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
        
        [SerializeField] private float gravity = -25f;

        private float _verticalVelocity;
        private float _currentY;
        
        [Header("Head Bounce")]
        [ReadOnly] public bool isBouncing = false;
        [SerializeField] protected float bouncePower = 5f;
        private float _timeSinceLastBounce;
        private bool _canBounce;

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
            currentSpace ??= targetSpace;
            
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

            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd) ||
                isVaulting || !canMove) return;
            // print($"Moving to Space: {sd.spaceID}");
            TargetSpace(sd);
            isMoving = true;
            // else print($"Couldn't access space: {sd.spaceID}");
        }

        /// <summary> Vaults the character to a given side and space </summary>
        public void VaultToSide(SpaceData spaceData, bool isHeavy)
        {
            // Check if the character can access that space
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
                return;
            if (isVaulting || isMoving || !canVault) return;
            TargetSpace(spaceData);
            isVaulting = true;
            isVaultingHeavily = isHeavy;
        }

        /// <summary>
        /// Vault to the side with a provided space data with an additional character to attack upon landing
        /// </summary>
        public void VaultToSide(SpaceData spaceData, bool isHeavy, Character victim)
        {
            // Check if the character can access that space
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
                return;
            if (isVaulting || isMoving || !canVault) return;
            TargetSpace(spaceData);
            isVaulting = true;
            isVaultingHeavily = isHeavy;
            TargetedCharacter = victim;
        }

        /// <summary> Sends the character directly to the position of the specified space on a given side </summary>
        public void GoToSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else transform.position = sd.t.position;
            }

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
            rb.isKinematic = true;
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
            SpaceData sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID, direction);
            //print($"Checked space: {sd.spaceID}");
            return Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd);
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
            }
            else if (canVault && isVaulting)
            {
                VaultMovement();
            }
            else
            {
                StayOnCurrentSpace();
            }

            if (isGrounded)
            {
                _currentY = 0f;
            }

            // Reset Bounce Cooldown
            if (_timeSinceLastBounce - Time.time < .5f) _canBounce = true;

        }
        #endregion

        #region Movement Updates
        
        private void StayOnCurrentSpace()
        {
            if (!currentSpace?.t) return;

            Vector3 basePos = currentSpace.t.localPosition;

            transform.localPosition = new Vector3(
                basePos.x,
                basePos.y + _currentY,
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
                float t = groundedMovementCurve.Evaluate(MovementTimeElapsed);

                Vector3 basePos = Vector3.Lerp(
                    PreviousSpace.t.localPosition,
                    TargetedSpace.t.localPosition,
                    t
                );

                transform.localPosition = new Vector3(
                    basePos.x,
                    basePos.y + _currentY,
                    basePos.z
                );
            }
            else
            {
                isMoving = false;
                MovementTimeElapsed = 0f;

                Vector3 basePos = TargetedSpace.t.localPosition;

                transform.localPosition = new Vector3(
                    basePos.x,
                    basePos.y + _currentY,
                    basePos.z
                );
            }

            MovementTimeElapsed += Time.deltaTime / groundedMovementTime;
        }



        protected virtual void VaultMovement()
        {
            if (VaultTimeElapsed < 1f)
            {
                VaultTimeElapsed += Time.deltaTime / vaultTime;

                float t = vaultCurve.Evaluate(VaultTimeElapsed);

                Vector3 pos = Vector3.Lerp(PreviousSpace.t.localPosition, TargetedSpace.t.localPosition, t);

                transform.localPosition = new Vector3(pos.x, pos.y + _currentY, pos.z);
            }
            else
            {
                isVaulting = false;
                VaultTimeElapsed = 0f;
                transform.localPosition = TargetedSpace.t.localPosition + Vector3.up * _currentY;

                if (!isJumping) OnVaulted(); // Prevent clashing hit-stop with the player
                
                if (TargetedCharacter != null) TargetedCharacter.GetComponent<IDamageable>().TakeDamage();
                TargetedCharacter = null;

                if (!isVaultingHeavily || !canInteractWithBoat) return;
                
                // Impact Boat
                if (!isGrounded)
                {
                    if (_waitGroundedRoutine != null) StopCoroutine(_waitGroundedRoutine);
                    _waitGroundedRoutine = StartCoroutine(WaitUntilGroundedRoutine());
                    print("Not Grounded, waiting");
                }
                else
                {
                    boatInteractor.ImpactBoat(TargetedSpace);
                    isVaultingHeavily = false;
                }
            }
        }

        protected virtual void OnVaulted()
        {
            animator.SetTrigger("Vaulted"); //TODO: add "Vault Landed" animation!
        }

        #endregion

        private void VerticalMovement()
        {
            if (isGrounded) return;
            // Do Gravity (fall)
            _verticalVelocity += gravity * Time.deltaTime;
            _currentY += _verticalVelocity * Time.deltaTime;

            // Damage any Characters below!
            var character = CharacterSpaceChecks.ScanAreaForDamageableCharacter
            (StompPosition.position, Vector3.one, StompPosition.rotation, TargetableCharacterLayers, true);
            if (character != null)
            {
                character.GetComponent<IDamageable>().TakeDamage();
                TriggerBounce();
            }

            // Detect Landing
            if (!(_currentY <= 0f)) return;
            _currentY = 0f;
            _verticalVelocity = 0f;
            isGrounded = true;
            OnLanded();
        }

        public void TriggerJump()
        {
            if (!isGrounded) return;
            
            isJumping = true;
            isGrounded = false;
            _verticalVelocity = jumpPower;
            _currentY = 0f;

            OnJumped();
        }

        /// <summary> Is called before the character jumps </summary>
        protected virtual void OnJumped()
        {
            animator.SetTrigger("Jump");
            
            if (isVaultingHeavily) boatInteractor.ImpactBoat(TargetedSpace);
            //TODO: Add Jump SFX and VFX
            
            print($"{gameObject.name} Jumped!");
        }

        /// <summary> Called whenever this character lands </summary>
        protected virtual void OnLanded()
        {
            isJumping = false;
            isBouncing = false;
            // rb.isKinematic = true; // TODO: Consider
            animator.SetTrigger("Landed");
            //TODO: Add landed SFX and VFX

            if (isVaultingHeavily) CameraShaker.Presets.Explosion3D();
            else CameraShaker.Presets.ShortShake3D();
        }

        public void TriggerBounce()
        {
            isGrounded = false;
            _verticalVelocity = bouncePower;
            isBouncing = true;
            _canBounce = false;

            _timeSinceLastBounce = Time.time;
        }

        
        private Coroutine _waitGroundedRoutine;

        private IEnumerator WaitUntilGroundedRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => isGrounded);
            
            isJumping = false;
            isBouncing = false;

            if (isVaultingHeavily && canInteractWithBoat)
            {
                isVaultingHeavily = false;
                boatInteractor.ImpactBoat(TargetedSpace);
                print("Grounded, Impacted Boat");
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
            Boat_Space_Manager.Instance.AddPassenger(this);
            isOnBoat = true;
            if (goToCurrentSpace) GoToSpace(currentSpace.sideID, currentSpace.spaceID);
        }

        /// <summary>
        /// Method to make the character exit the boats parent
        /// </summary>
        public void ExitBoat(bool goToCurrentSpace)
        {
            Boat_Space_Manager.Instance.RemovePassenger(this);
            isOnBoat = false;
            if (goToCurrentSpace) MoveToSpace(currentSpace.sideID, currentSpace.spaceID);
        }
        #endregion
        
        public void ResetCharacter()
        {
            ExitBoat(false);

            currentSpace = null;
            
            isJumping = false;
            isMoving = false;
            isBouncing = false;
        }
    }
}

