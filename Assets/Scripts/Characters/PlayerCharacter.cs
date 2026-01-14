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
            private InputAction _vaultAction;
            private InputAction _vaultHeavyAction;

            private void Awake()
            {
                var actionMap = playerInput.currentActionMap;
                _moveAction = actionMap.FindAction("Move");
                // _moveAction.performed += OnMove;

                _vaultAction = actionMap.FindAction("Vault");
                _vaultHeavyAction = actionMap.FindAction("VaultHeavy");

                // On Press trigger Vault
                _vaultHeavyAction.started += OnVaultHeavy;
                _vaultAction.started += OnVault;

                //// Jump Performed
                //vaultAction.performed += OnVaultJump;
                //vaultHeavyAction.performed += OnVaultJump;
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
                _vaultAction?.Enable();
                _vaultHeavyAction?.Enable();

                if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected += GemstoneCollected;
            }

            private void OnDisable()
            {
                _moveAction?.Disable();
                _vaultAction?.Disable();
                _vaultHeavyAction?.Disable();

                if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
            }

            private void Update()
            {
                //TODO: Temporary way of controlling timescale for playtesting purposes
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

            protected override void TimeUpdate()
            {
                // Insert player actions here
                if (GameManager.GameLogic.GamePaused) return;
                OnMove();
            }

            private void OnMove()
            {
                //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
                // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)

                // Handle movement logic here
                var direction = Mathf.RoundToInt(_moveAction.ReadValue<Vector2>().x);

                // Note: Inverting the direction since the order of the boat spaces are flipped...
                //MoveToSpace(Mathf.RoundToInt(direction * -1), _currentSpace);
                MoveToSpaceFromDirection(Mathf.RoundToInt(direction * -1));
            }

            /// <summary>
            /// The Vault Player Input Action Function
            /// </summary>
            private void OnVault(InputAction.CallbackContext context)
            {
                if (GameManager.GameLogic.GamePaused) return;

                // Vault logic
                if (isVaultingHeavily || isVaulting)
                {
                    //TODO: Trigger Jump Upon Landing Logic Here

                    return;
                }
                else
                {
                    //print("Player Vaulted");
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
                        VaultToSide(newSpace);
                    }
                    //TODO: Implement vaulting animation here?
                }
                print("Performed Light Vault");
            }

            private void OnVaultHeavy(InputAction.CallbackContext context)
            {
                if (GameManager.GameLogic.GamePaused) return;

                // Vault logic
                if (isVaultingHeavily || isVaulting)
                {
                    //TODO: Trigger Jump Upon Landing Logic Here

                    return;
                }
                else
                {
                    //print("Player Vaulted");
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
                        VaultToSide(newSpace);
                    }
                    //TODO: Implement vaulting animation here?
                }
                print("Performed Heavy Vault");
            }
            #endregion

            #region Gemstone Events

            void GemstoneCollected(int amount)
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

