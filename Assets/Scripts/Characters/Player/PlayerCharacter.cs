using System;
using System.Collections;
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
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _vaultAction;
        private InputAction _groundPoundAction;

        private float _inputInhibitation = .1f;
        
        #region Event Listeners

        // Action Listening
        public static event Action OnPlayerMoved;
        public static event Action OnPlayerVaulted;
        public static event Action OnPlayerGroundPounded;
        public static event Action OnPlayerJumped;

        // Event Listening
        public delegate void PlayerDied(DamageType damageType);
        public static PlayerDied OnPlayerDied;
        public static event Action OnPlayerKilledEnemy;
        /// <summary>
        /// Event to trigger when the player has taken damage. The parameter is the amount of health remaining.
        /// </summary>
        public static event Action<int> OnPlayerDamaged;

        #endregion

        private void Awake()
        {
            var actionMap = InputSystem.actions.actionMaps[0];
            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");

            _vaultAction = actionMap.FindAction("Vault");
            _groundPoundAction = actionMap.FindAction("GroundPound");
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
            _jumpAction?.Enable();
            _vaultAction?.Enable();
            _groundPoundAction?.Enable();

            // TODO: TEMP. Reset health whenever a new level is loaded
            GameLevelManager.OnLevelLoaded += HealthComponent.RestoreHealth;

            GameManager.GameLogic.OnGemstoneCollected += GemstoneCollected;
            GameManager.GameLogic.OnGameResume += () => _inputInhibitation = 0f;
            
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;

            GameSettingsManager.GameplayChanged += AssignInvincibility;
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _vaultAction?.Disable();
            _groundPoundAction?.Disable();
            
            // TODO: TEMP. Reset health whenever a new level is loaded
            GameLevelManager.OnLevelLoaded -= HealthComponent.RestoreHealth;

            if (GameManager.Instance) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
            
            GameSettingsManager.GameplayChanged -= AssignInvincibility;
        }

        protected override void TimeUpdate()
        {
            if (GameManager.GameLogic.IsGamePaused) return;
            
            if (Mathf.Approximately(_inputInhibitation, .1f))
            {
                // Player actions here
                MoveInput();

                VaultInput();

                GroundPoundInput();
                JumpInput();
            }
            _inputInhibitation = .1f; // TODO: Temp as to somewhat prevent the player from making an input as they press 'resume'

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
        private void VaultInput()
        {
            if (WillVault) return;
            
            switch (GameSettingsManager.UseAlternativeControlScheme)
            {
                // Alternative vault towards the vertical move direction
                case true:
                    var direction = _moveAction.ReadValue<Vector2>().y;
                    // Player is on the opposite lane of the boat
                    if (currentSpace.sideID > 0)
                    {
                        if (direction < -.6f) PerformVault();
                    }
                    else if (direction > .6f) PerformVault();
                    break;
                // Default tap to vault
                case false:
                    if (!_vaultAction.WasPerformedThisFrame()) return;
                    PerformVault();
                    break;
            }
        }
        
        /// <summary>
        /// The Ground Pound Player Input Action Function
        /// </summary>
        private void GroundPoundInput()
        {
            var action = GameSettingsManager.UseAlternativeControlScheme
                ? _groundPoundAction.WasPressedThisFrame()
                : _groundPoundAction.WasPerformedThisFrame();
            if (action && !isGroundPounding) TriggerGroundPound();
        }

        protected override void OnGroundPoundTriggered()
        {
            base.OnGroundPoundTriggered();
            
            // Play initial Ground Pound audio based on if the player is grounded or not
            var clip = isGrounded? Clip.Plyr_Pound0 : Clip.Plyr_Pound1;
            AudioManager.Play(clip);
        }

        protected override void OnGroundPound()
        {
            base.OnGroundPound();
            if (isGrounded)
            {
                OnPlayerGroundPounded?.Invoke();
                AudioManager.Play(Clip.Plyr_Pound2);
            }
            else
            {
                // Debug.Log("Player Triggered the Ground Pound in the Air");
                // AudioManager.Play(Clip.AirSlam); // TODO: Create SFX for triggering Ground Pound in the air
            }
        }
        #endregion

        protected override void OnVault()
        {
            base.OnVault();
            AudioManager.Play(Clip.Plyr_Vault);
            
            // Trigger OnPlayerVaulted listeners. Used for the Tutorial Section.
            OnPlayerVaulted?.Invoke();
        }

        protected override void OnVaulted()
        {
            base.OnVaulted();
            if (isGroundPounding)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Land_1); // TODO: needs a new sfx for ground pounding
            }
            else AudioManager.Play(Clip.Plyr_Land_0);
        }

        protected override void OnMove()
        {
            base.OnMove();
            AudioManager.Play(isGrounded ? Clip.Plyr_Dash_0 : Clip.Plyr_Dash_1);
        }

        protected override void OnMoved()
        {
            base.OnMoved();
        }

        private void JumpInput()
        {
            if (!isGrounded) return;
            switch (GameSettingsManager.UseAlternativeControlScheme)
            {
                // Alternative tap to jump
                case true:
                    if (_jumpAction.WasPressedThisFrame())
                    {
                        if (isVaulting) WillJump = true;
                        else TriggerJump();
                    }
                    break;
                // Default hold to jump
                case false:
                    if (_jumpAction.WasPerformedThisFrame())
                    {
                        if (isVaulting) WillJump = true;
                        else TriggerJump();
                    }
                    break;
            }
        }

        protected override void OnJumped()
        {
            base.OnJumped();
            /*if (isGroundPounding)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Jump_1); // Heavy Jump
            }
            else
            {
                // Trigger OnPlayerJumped listeners. Used for the Tutorial Section.
                OnPlayerJumped?.Invoke();
                AudioManager.Play(Clip.Plyr_Jump_0); // Light Jump
            }*/

            CancelGroundPound();
            
            // Trigger OnPlayerJumped listeners. Used for the Tutorial Section.
            OnPlayerJumped?.Invoke();
            AudioManager.Play(Clip.Plyr_Jump_0); // Light Jump
        }

        protected override void OnLanded()
        {
            base.OnLanded();
            if (isGroundPounding)
            {
                TriggerHitStop(.05f);
                AudioManager.Play(Clip.Plyr_Land_1);
                
            }
            else AudioManager.Play(Clip.Plyr_Land_0);
        }

        protected override void OnTargetEliminated()
        {
            base.OnTargetEliminated();
            AudioManager.Play(Clip.Stomped);
            GameManager.GameLogic.playerData.EnemiesDefeated++;
            OnPlayerKilledEnemy?.Invoke(); // Enemy Killed Check for the Tutorial
        }

        #region Gemstone Events

        private void GemstoneCollected(int amount)
        {
            // TODO: Gemstone Collected, Trigger some sort of effect
        }

        #endregion
        public override void OnDied(DamageType damageType)
        {
            // if (GameSettingsManager.DoPlayerInvincibility) return;
            base.OnDied(damageType);
            
            TriggerHitStop(1); // TODO: Get a timed reference or something to delay the End Game Logic and the Death SFX
            MusicManager.Instance.PauseMusic();
            OnPlayerDied?.Invoke(damageType);
            Debug.Log($"PLAYER DIED! Type = {damageType}");

            var gameOverType = damageType switch
            {
                DamageType.Stomp or DamageType.Standard => GameManager.MainGameLogic.GameOverType.Default,
                DamageType.Tsunami => GameManager.MainGameLogic.GameOverType.Tsunami,
                _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
            };

            // Wait for the hitstop to end to trigger the death event
            StartCoroutine(DeathEvent(gameOverType));
        } 
        
        private IEnumerator DeathEvent(GameManager.MainGameLogic.GameOverType gameOverType)
        {
            yield return new WaitForSeconds(.1f);
            
            // Explode artwork and play death sound 
            if (TryGetComponent<ArtExplode>(out var explode)) explode.ExplodeArt();
            AudioManager.Play(Clip.Plyr_Died);
            
            // End the game after dying! Wait four seconds before doing so
            // GameManager.Instance.Invoke(nameof(GameManager.GameLogic.EndGame), 4f);
            GameManager.GameLogic.EndGame(gameOverType);
            Debug.Log("PLAYER DIED. ENDED GAME");
        }

        public override void OnHealthRestored()
        {
            base.OnHealthRestored();
            // Debug.Log("Player Health Restored");
        }

        public override void OnTookDamage(DamageType damageType)
        {
            if (GameSettingsManager.DoPlayerInvincibility && damageType != DamageType.Tsunami) return;
            base.OnTookDamage(damageType);

            AudioManager.Play(Clip.Plyr_Hurt);
            OnPlayerDamaged?.Invoke(HealthComponent.CurrentHealth);
            // Debug.Log("Player Took Damage");
        }

        private void AssignInvincibility()
        {
            HealthComponent.IsInvincible = GameSettingsManager.DoPlayerInvincibility;
        }
    }
    
}

