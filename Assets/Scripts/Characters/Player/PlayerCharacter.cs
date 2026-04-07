using System;
using CarterGames.Assets.AudioManager;
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
        
        #region Event Listeners

        // Action Listening
        public static event Action OnPlayerMoved;
        public static event Action OnPlayerVaulted;
        public static event Action OnPlayerHeavyVaulted;
        public static event Action OnPlayerJumped;
        public static event Action OnPlayerHeavyJumped;

        // Event Listening
        public delegate void PlayerDied();
        public static PlayerDied OnPlayerDied;
        public static event Action OnPlayerKilledEnemy;

        #endregion

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

            // TODO: TEMP. Reset health whenever a new level is loaded
            GameLevelManager.OnLevelLoaded += HealthComponent.RestoreHealth;

            if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected += GemstoneCollected;
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;

            GameSettingsManager.GameplayChanged += AssignInvincibility;
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _vaultLightAction?.Disable();
            _vaultHeavyAction?.Disable();
            
            // TODO: TEMP. Reset health whenever a new level is loaded
            GameLevelManager.OnLevelLoaded -= HealthComponent.RestoreHealth;

            if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
            
            GameSettingsManager.GameplayChanged -= AssignInvincibility;
        }

        protected override void TimeUpdate()
        {
            // Insert player actions here
            MoveInput();

            if (!WillVault)
            {
                OnLightVault();
                OnHeavyVault();
            }
            else
            {
                VaultPostProcess();
            }

            
            base.TimeUpdate();
            
            //TODO: Temporary way of triggering the players bounce, for testing purposes. Remove on Build
            if (Input.GetKeyDown(KeyCode.Alpha0))
                TriggerBounce();
            
            //TODO: Temporary way of controlling timescale for playtesting purposes. Remove on Build
            if (Input.GetKeyDown(KeyCode.Alpha1))
                Time.timeScale = 0f;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                Time.timeScale = .1f;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                Time.timeScale = .2f;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                Time.timeScale = .25f;
            if (Input.GetKeyDown(KeyCode.Alpha5))
                Time.timeScale = 1f;
            if (Input.GetKeyDown(KeyCode.Alpha6))
                Time.timeScale = 2f;
            if (Input.GetKeyDown(KeyCode.Alpha7))
                Time.timeScale = 10f;
        }

        private void MoveInput()
        {
            //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
            // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)
            
            // Handle movement logic here
            var direction = Mathf.RoundToInt(_moveAction.ReadValue<Vector2>().x);
            if (Mathf.Approximately(direction, 0)) return; // TODO: Test if this works on controller

            MoveDirection = direction;
            if (!isMoving)
            {
                MoveToSpaceFromDirection(Mathf.RoundToInt(MoveDirection)); 
                // Trigger OnPlayerMoved listeners. Used for the Tutorial Section.
                if (isMoving) OnPlayerMoved?.Invoke(); // If moving check is to confirm that the player is indeed moving
            }
            if (!isVaulting) return;
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
                isVaultingHeavily = false;
                PerformVault(false);
            }
            // VaultPostProcess();
        }

        private void OnHeavyVault()
        {
            if (_vaultHeavyAction.WasPressedThisFrame())
            {
                WillVault = true;
                isVaultingHeavily = true;
                PerformVault(true);
            }
            // VaultPostProcess();
        }


        // Bonus process stuff after a heavy or light vault
        private void VaultPostProcess()
        {
            if (!isVaulting) return;
            //TODO: Trigger Jump Upon Landing Logic Here
            
            // Trigger Jump if Vault Button is held
            if (_vaultHeavyAction.WasPerformedThisFrame() || _vaultLightAction.WasPerformedThisFrame())
            {
                Debug.Log("Jump Action Performed");
                if (WillJump) return;
                WillJump = true;
            }
            
            // TriggerJump(); //TODO: Jump is still broken and won't trigger upon vaulting

        }
        #endregion

        protected override void OnVault()
        {
            base.OnVault();
            AudioManager.Play(Clip.Plyr_Vault);
            
            // Trigger OnPlayerVaulted listeners. Used for the Tutorial Section.
            if (isVaultingHeavily) OnPlayerHeavyVaulted?.Invoke();
            else OnPlayerVaulted?.Invoke();
        }

        protected override void OnVaulted()
        {
            base.OnVaulted();
            if (isVaultingHeavily)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Land_1); // TODO: needs a new sfx for heavy vaulting
            }
            else AudioManager.Play(Clip.Plyr_Land_0);
        }

        protected override void OnMove()
        {
            base.OnMove();
            AudioManager.PlayGroup(Group.Plyr_Dash);
        }

        protected override void OnMoved()
        {
            base.OnMoved();
        }

        protected override void OnJumped()
        {
            base.OnJumped();
            if (isVaultingHeavily)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Jump_1); // Heavy Jump
                OnPlayerHeavyJumped?.Invoke();
            }
            else
            {
                // Trigger OnPlayerJumped listeners. Used for the Tutorial Section.
                OnPlayerJumped?.Invoke();
                AudioManager.Play(Clip.Plyr_Jump_0); // Light Jump
            }
        }

        protected override void OnLanded()
        {
            base.OnLanded();
            if (isVaultingHeavily)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Land_1);
            }
            else AudioManager.Play(Clip.Plyr_Land_0);
        }

        protected override void OnTargetEliminated()
        {
            base.OnTargetEliminated();
            OnPlayerKilledEnemy?.Invoke(); // Enemy Killed Check for the Tutorial
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
            
            TriggerHitStop(1); // TODO: Get a timed reference or something to delay the End Game Logic and the Death SFX
            MusicManager.Instance.PauseMusic();
            OnPlayerDied?.Invoke();
            Debug.Log("PLAYER DIED");
                
            // Wait for the hitstop to end to trigger the death event
            Invoke(nameof(DeathEvent), 0.1f);
        } 
        
        private void DeathEvent()
        {
            // Explode artwork and play death sound 
            if (TryGetComponent<ArtExplode>(out var explode)) explode.ExplodeArt();
            AudioManager.Play(Clip.Plyr_Died);
            
            // End the game after dying! Wait four seconds before doing so
            // GameManager.Instance.Invoke(nameof(GameManager.GameLogic.EndGame), 4f);
            GameManager.GameLogic.EndGame();
            Debug.Log("PLAYER DIED. ENDED GAME");
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

            AudioManager.Play(Clip.Plyr_Hurt);
            // Debug.Log("Player Took Damage");
        }

        private void AssignInvincibility()
        {
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;
        }
    }
    
}

