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

        [Header("Vault Movement")]
        [SerializeField] protected bool canVault = true;
        [SerializeField, ShowField(nameof(canVault))] protected float vaultTime = .5f;
        [SerializeField, ShowField(nameof(canVault))] protected AnimationCurve vaultCurve;
        [SerializeField, ReadOnly, ShowField(nameof(canVault))] protected bool isVaulting;
        [SerializeField, ReadOnly, ShowField(nameof(canVault))] protected bool isVaultingHeavily;

        protected float VaultTimeElapsed = 0f;

        [Header("Jump Movement")]
        [SerializeField] protected bool canJump = true;
        [SerializeField, ShowField(nameof(canJump))] private float jumpHeight = 5f;

        [Header("Space Information")]
        [Tooltip("The current space on the boat this character is on")]
        [SerializeField, ReadOnly] protected SpaceData currentSpace;
        [SerializeField] protected bool isOnBoat;

        /* Variables for lerping target space movement */
        /// <summary> The space on the boat this character is currently moving towards </summary>
        protected SpaceData _targetedSpace;
        /// <summary> The space on the boat this character was previously at before the next targeted space </summary>
        protected SpaceData _previousSpace;

        [Space]

        [Tooltip("Determines whether the character can move to and stand on the outer spaces of the boat")]
        [SerializeField] protected bool canAccessOuterBoatSides = false;
        [Tooltip("Determines whether the character can move to and stand on the spaces of the boat")]
        [SerializeField] protected bool canAccessBoatSpaces = true;

        #endregion

        #region Space Movement Methods

        /// <summary> Set the character to target a given space </summary>
        protected void TargetSpace(SpaceData targetSpace)
        {
            if (currentSpace != null) currentSpace.isOccupied = false;

            currentSpace = targetSpace;
            currentSpace.isOccupied = true;

            // Assign previous and new target space for movement updates
            _previousSpace = currentSpace;
            _targetedSpace = targetSpace;

            //Debug.Log("Got New Space Data!");
        }

        /// <summary> Moves the character to a space on the boat via a given side and space </summary>
        protected void MoveToSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);

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
        protected void MoveToSpaceFromDirection(int direction)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(currentSpace.sideID, currentSpace.spaceID, direction);

            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd) && !isVaulting && canMove)
            {
                //print($"Moving to Space: {sd.spaceID}");
                TargetSpace(sd);
                isMoving = true;
            }
            //else print($"Couldn't access space: {sd.spaceID}");
        }

        /// <summary> Vaults the character to a given side and space </summary>
        protected void VaultToSide(SpaceData spaceData)
        {
            // Check if the character can access that space
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
            {
                if (!isVaulting && !isMoving && canVault && isGrounded)
                {
                    TargetSpace(spaceData);
                    isVaulting = true;
                }
            }
        }

        /// <summary>
        /// Vault to the side with a provided space data with an additional character to attack upon landing
        /// </summary>
        protected void VaultToSide(SpaceData spaceData, Character victim)
        {
            // Check if the character can access that space
            if (Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, spaceData))
            {
                if (!isVaulting && !isMoving && canVault && isGrounded)
                {
                    TargetSpace(spaceData);
                    isVaulting = true;
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
                else transform.position = sd.t.position;
            }

        }

        /// <summary> Sends the character directly to the position of the specified Side Space on a given space </summary>
        public void GoToSideSpace(int side, bool goLeftSide = true)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetSideSpace(side, goLeftSide);
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                // TODO: Consider this. Character might be off the boat if they're going to a side space
                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else transform.position = sd.t.position;
            }
        }

        /// <summary> Sends the character directly to the position of the specified space on the Boat </summary>
        public void GoToBoatSpace(int side, int space)
        {
            SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(side, space);
            if (!Boat_Space_Manager.Instance.CheckSpaceAccess(canAccessOuterBoatSides, canAccessBoatSpaces, sd))
            {
                TargetSpace(sd);

                // TODO: Consider this. Character might be off the boat if they're going to a side space
                if (isOnBoat) transform.localPosition = sd.t.localPosition;
                else transform.position = sd.t.position;
            }
        }

        /// <summary> Get this characters current space in the boat </summary>
        public SpaceData GetCurrentSpaceData()
        {
            return currentSpace;
        }

        /// <summary> Returns whether the next space is available to go to </summary>
        protected bool CheckAvailableSpaceFromDirection(int direction)
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
            
            // Set vertical distance to distance from current boat space
            verticalDistance = GetVerticalDistanceFromSpace();

            if (canMove && isMoving)
            {
                SpaceMovement();
            }
            else if (canVault && isVaulting)
            {
                VaultMovement();
            }
            else
            {
                Vector3 pos = currentSpace.t.position;
                rb.MovePosition(isGrounded ? pos : new(pos.x, rb.position.y, pos.z));
            }
        }

        #endregion

        #region Movement Updates
        /// <summary>
        /// The movement of this character towards their targeted boat space
        /// </summary>
        protected virtual void SpaceMovement()
        {
            _movementTimeElapsed += Time.fixedDeltaTime / groundedMovementTime;

            // Whilst moving
            if (_movementTimeElapsed < 1f)
            {
                // Lerp from the previous space to the new target space
                Vector3 pos;
                if (isGrounded)
                {
                    pos = Vector3.Lerp
                        (_previousSpace.t.position, _targetedSpace.t.position,
                            groundedMovementCurve.Evaluate(_movementTimeElapsed)
                        );
                    rb.MovePosition(pos);

                }
                else
                {
                    pos = Vector3.Lerp
                        (_previousSpace.t.position, _targetedSpace.t.position,
                            airMovementCurve.Evaluate(_movementTimeElapsed)
                        );
                    rb.MovePosition(new (pos.x, verticalDistance, pos.z));

                }

                // Coyote Time
                if (_movementTimeElapsed < coyoteTime && coyoteTriggered)
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
                groundedMovementTime = 0f;

                rb.MovePosition
                    (isGrounded ?
                        _targetedSpace.t.position
                        :
                        new(_targetedSpace.t.position.x, rb.position.y, _targetedSpace.t.position.z
                    ));
            }

        }

        protected void VaultMovement()
        {
            //TODO:

            if (VaultTimeElapsed < 1f)
            {
                VaultTimeElapsed += Time.fixedDeltaTime / vaultTime;

                Vector3 pos = Vector3.Lerp
                    (_previousSpace.t.position, _targetedSpace.t.position,
                    vaultCurve.Evaluate(VaultTimeElapsed)
                    );
                rb.MovePosition(pos);

                //TODO: An optional art thing you could do is attatch the characters hands to the wall as they vault?

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
                rb.MovePosition(_targetedSpace.t.position);
            }
        }
        #endregion

        protected float GetVerticalDistanceFromSpace()
        {
            var dist = rb.position.y - currentSpace.t.position.y;
            isGrounded = Mathf.Approximately(dist, 0f);

            return dist;
        }

        protected void TriggerJump()
        {
            OnJumped();
            ExitBoat(false);
            rb.AddForce(Vector2.up * jumpHeight, ForceMode.Impulse);
        }

        /// <summary> Is called before the character jumps </summary>
        protected virtual void OnJumped()
        {
            anim.SetTrigger("Jump");
            //TODO: Add Jump SFX
        }

        #region Boat Entering Methods
        /// <summary>
        /// Method to make the character enter the boats parent
        /// </summary>
        protected void EnterBoat(bool goToCurrentSpace)
        {
            //Boat_Space_Manager.Instance.AddPassenger(this); //TODO: Intergrate BoatCharacter to the BoatSpaceManager
            isOnBoat = true;
            if (goToCurrentSpace) MoveToSpace(currentSpace.sideID, currentSpace.spaceID);
        }

        /// <summary>
        /// Method to make the character exit the boats parent
        /// </summary>
        protected void ExitBoat(bool goToCurrentSpace)
        {
            //Boat_Space_Manager.Instance.RemovePassenger(this); //TODO: Intergrate BoatCharacter to the BoatSpaceManager
            isOnBoat = false;
            if (goToCurrentSpace) MoveToSpace(currentSpace.sideID, currentSpace.spaceID);
        }
        #endregion
    }
}

