using UnityEngine;
using UnityEngine.InputSystem;
using EditorAttributes;
using Game;

namespace GameCharacters
{
    /// <summary>
    /// The class representing the player character
    /// </summary>
    public class PlayerCharacter : BoatCharacter
    {
        #region Variables
        [Title("Player")]
        [Line(GUIColor.Green)]
        
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
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;

            GameSettingsManager.GameplayChanged += AssignInvincibility;
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _vaultLightAction?.Disable();
            _vaultHeavyAction?.Disable();

            if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
            
            GameSettingsManager.GameplayChanged -= AssignInvincibility;
        }

        protected override void TimeUpdate()
        {
            // Insert player actions here
            OnMove();
            OnLightVault();
            OnHeavyVault();
            
            base.TimeUpdate();
            
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
            if (Input.GetKeyDown(KeyCode.Alpha6))
                Time.timeScale = 4f;
        }

        private void OnMove()
        {
            //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
            // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)
            
            // Handle movement logic here
            var direction = Mathf.RoundToInt(_moveAction.ReadValue<Vector2>().x);
            if (Mathf.Approximately(direction, 0)) return; // TODO: Test if this works on controller

            MoveDirection = direction;
            if (!isMoving) MoveToSpaceFromDirection(Mathf.RoundToInt(MoveDirection));
            if (!isVaulting) return;
            print("Yeah");
            WillMove = true;
        }

        /// <summary>
        /// The Vault Player Input Action Function
        /// </summary>
        private void OnLightVault()
        {
            if (_vaultLightAction.WasPressedThisFrame())
            {
                WillVault = true;
                PerformVault(false);
            }
            VaultPostProcess();
        }

        private void OnHeavyVault()
        {
            if (_vaultHeavyAction.WasPressedThisFrame())
            {
                WillVault = true;
                PerformVault(true);
            }
            VaultPostProcess();
        }


        // Bonus process stuff after a heavy or light vault
        private void VaultPostProcess()
        {
            if (!isVaulting) return;
            //TODO: Trigger Jump Upon Landing Logic Here
            
            // Trigger Jump if Vault Button is held
            if (_vaultHeavyAction.WasPerformedThisFrame() 
                || 
                _vaultLightAction.WasPerformedThisFrame()) WillJump = true;
            if (WillJump) TriggerJump();
        }
        #endregion

        protected override void OnVaulted()
        {
            base.OnVaulted();
            if (isVaultingHeavily) TriggerHitStop(.1f);
        }
        
        protected override void OnJumped()
        {
            base.OnJumped();
            if (isVaultingHeavily) TriggerHitStop(.1f);
        }

        protected override void OnLanded()
        {
            base.OnLanded();
            if (isVaultingHeavily) TriggerHitStop(.1f);
        }

        #region Gemstone Events

        private void GemstoneCollected(int amount)
        {
            // TODO: Gemstone Collected, Trigger some sort of effect
        }

        #endregion
        public override void OnDied()
        {
            if (GameSettingsManager.DoPlayerInvincibility) return;
            base.OnDied();
            Debug.Log("PLAYER DIED");

        } 

        public override void OnHealthRestored()
        {
            base.OnHealthRestored();
            Debug.Log("Player Health Restored");
        }

        public override void OnTookDamage()
        {
            if (GameSettingsManager.DoPlayerInvincibility) return;
            base.OnTookDamage();
            Debug.Log("Player Took Damage");
        }

        private void AssignInvincibility()
        {
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;
        }
    }
    
}

