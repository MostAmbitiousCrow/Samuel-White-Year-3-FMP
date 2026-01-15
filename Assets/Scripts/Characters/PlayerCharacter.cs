using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCharacters
{
    namespace Player
    {
        /// <summary>
        /// The class representing the player character
        /// </summary>
        public class PlayerCharacter : BoatCharacter
        {
            #region Variables
            [Header("Initiation")]
            [SerializeField] private int startBoatSpace;
            [SerializeField] private int startBoatSide;

            #endregion

            #region Input Actions
            [Header("Input Actions")]
            public PlayerInput playerInput;
            private InputAction _moveAction;
            private InputAction _vaultLightAction;
            private InputAction _vaultHeavyAction;

            private void Awake()
            {
                var actionMap = playerInput.currentActionMap;
                _moveAction = actionMap.FindAction("Move");

                _vaultLightAction = actionMap.FindAction("Vault");
                _vaultHeavyAction = actionMap.FindAction("VaultHeavy");
            }

            private void Start()
            {
                GoToBoatSpace(startBoatSide, startBoatSpace);
                EnterBoat(true);

                GoToSpace(startBoatSide, startBoatSpace);
            }

            private void OnEnable()
            {
                _moveAction?.Enable();
                _vaultLightAction?.Enable();
                _vaultHeavyAction?.Enable();

                if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected += GemstoneCollected;
            }

            private void OnDisable()
            {
                _moveAction?.Disable();
                _vaultLightAction?.Disable();
                _vaultHeavyAction?.Disable();

                if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
            }

            protected override void TimeUpdate()
            {
                // Insert player actions here
                OnMove();
                OnLightVault();
                OnHeavyVault();
                
                //TODO: Temporary way of triggering the players bounce, for testing purposes. Remove on Build
                if (Input.GetKeyDown(KeyCode.Alpha0))
                    TriggerBounce();
                
                //TODO: Temporary way of controlling timescale for playtesting purposes. Remove on Build
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    Time.timeScale = 0f;
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    Time.timeScale = .25f;
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    Time.timeScale = .5f;
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    Time.timeScale = 1f;
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    Time.timeScale = 2f;
            }

            private void OnMove()
            {
                //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
                // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)
                
                // Handle movement logic here
                var direction = Mathf.RoundToInt(_moveAction.ReadValue<Vector2>().x);
                if (direction == 0) return;

                // Note: Inverting the direction since the order of the boat spaces are flipped...

                if (!isMoving || isMoving && coyoteTriggered) MoveToSpaceFromDirection(Mathf.RoundToInt(direction * -1));
            }

            /// <summary>
            /// The Vault Player Input Action Function
            /// </summary>
            private void OnLightVault()
            {
                if (_vaultLightAction.WasPressedThisFrame()) PerformVault(false);
                if (_vaultLightAction.WasPerformedThisFrame()) TriggerJump();
            }

            private void OnHeavyVault()
            {
                if (_vaultHeavyAction.WasPressedThisFrame()) PerformVault(true);
                if (_vaultHeavyAction.WasPerformedThisFrame()) TriggerJump();
            }

            private void PerformVault(bool isHeavy)
            {
                if (GameManager.GameLogic.GamePaused) return;

                if (isVaulting)
                {
                    //TODO: Trigger Jump Upon Landing Logic Here
                    
                    // Trigger Jump if Vault Button is held
                    if (isHeavy ? _vaultHeavyAction.WasPerformedThisFrame() : _vaultLightAction.WasPerformedThisFrame())
                    {
                        TriggerJump();
                    }
                    
                    // coyoteTriggered = true;
                    return;
                }
                else
                {
                    var newSpace = Boat_Space_Manager.Instance.GetSpaceFromOppositeLane(currentSpace.sideID, currentSpace.spaceID);

                    // Vault to space. Additionally, if an enemy is on the opposite side of the space, do an attack vault
                    var bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                        (newSpace.t.position, Vector3.one, Quaternion.identity, targetableCharacterLayers, true, false);
                    if (bc)
                    {
                        //VaultToSide(newSpace, bc); // TODO: Modify to scan for damageable characters with the Character component
                    }
                    else
                    {
                        VaultToSide(newSpace, isHeavy);
                    }
                    //TODO: Implement vaulting animation here?
                }
                // print($"Performed Vault. Heavy Vault = {isHeavy}");
            }
            #endregion

            #region Gemstone Events

            private void GemstoneCollected(int amount)
            {
                // TODO: Gemstone Collected, Trigger some sort of effect
            }

            #endregion
            public override void RecieveDied()
            {
                Debug.Log("PLAYER DIED");

            } 

            public override void RecieveHealthRestored()
            {
                Debug.Log("Player Health Restored");
            }

            public override void RecieveTookDamage()
            {
                Debug.Log("Player Took Damage");
            }
        }
    }
    
}

