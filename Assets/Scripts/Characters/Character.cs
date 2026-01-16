using UnityEngine;
using EditorAttributes;

namespace GameCharacters
{
    /// <summary>
    /// The root class of all characters
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CharacterHealth))]
    public abstract class Character : MonoTimeBehaviour
    {
        #region Variables
        [Title("Character")]
        [Line(GUIColor.White)]
        
        [Header("Movement")]
        [SerializeField] protected bool canMove;
        public bool CanMove => canMove;
        [Tooltip("The time it takes for the character to move to their next targeted space whilst grounded")]
        [SerializeField] protected float groundedMovementTime = .2f;
        [Tooltip("The curve controlling the ground movement animation of the character")]
        [SerializeField] protected AnimationCurve groundedMovementCurve;

        [Tooltip("The time remaining of the characters movement they're able to immediately move again")]
        [SerializeField] protected float coyoteTime = .15f;
        [SerializeField, ReadOnly] protected bool coyoteTriggered;

        protected float MovementTimeElapsed = 0f;

        [Space]

        [Tooltip("The time it takes for the character to move to their next targeted space whilst in the air")]
        [SerializeField] protected float airMovementTime = .25f;
        [Tooltip("The curve controlling the air movement animation of the character")]
        [SerializeField] protected AnimationCurve airMovementCurve;

        [Space]

        [Tooltip("The vertical distance from the characters current space or lane")]
        // Must be set specifically by either Boat or RiverLane Characters!
        [SerializeField, ReadOnly] protected float verticalDistance;
        [Tooltip("The vertical offset of the character from their current space or lane. Modify this for flying enemies.")]
        [SerializeField] protected float movementVerticalOffset = 0f;
        
        [SerializeField, ReadOnly] protected MoveDirection currentDirection = MoveDirection.Left;
        public  MoveDirection CurrentDirection => currentDirection;
        /// <summary> Left = 1 | Right = -1 </summary>
        public enum MoveDirection { Right = -1, Left = 1 }

        [Header("Checks")]
        [SerializeField, ReadOnly] protected bool isMoving;
        public bool IsMoving => isMoving;
        [SerializeField, ReadOnly] protected bool isGrounded;
        public bool IsGrounded => isGrounded;
        [Space]
        [SerializeField] private LayerMask targetableCharacterLayers;
        public LayerMask TargetableCharacterLayers => targetableCharacterLayers;

        [Header("Components")]
        [SerializeField] protected Transform artRoot;
        public Transform ArtRoot => artRoot;
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;
        [SerializeField] private CharacterHealth healthComponent;
        public CharacterHealth HealthComponent => healthComponent;

        #endregion
        
        #region Directions
        /// <summary> Reverses the current direction of the enemy </summary>
        public void FlipDirection()
        {
            // TODO: Flip the enemy character art to face current direction (see SetDirection())
            switch (currentDirection)
            {
                case MoveDirection.Left: SetDirection(MoveDirection.Right); break;
                case MoveDirection.Right: SetDirection(MoveDirection.Left); break;
                default: break;
            }
        }

        /// <summary> Explicitly sets the direction of the enemy with a given parameter </summary>
        public void SetDirection(MoveDirection direction)
        {
            currentDirection = direction;
        
            //TODO: Set the direction of the character here!
        }
        #endregion

        #region Damage Events
        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this character takes damage
        /// </summary>
        public virtual void OnTookDamage()
        {
            animator.SetTrigger("TookDamage");
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script when this character dies
        /// </summary>
        public virtual void OnDied()
        {
            animator.SetTrigger("Died");
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this characters health is restored
        /// </summary>
        public virtual void OnHealthRestored()
        {

        }
        #endregion

        protected override void OnHitStop()
        {
            base.OnHitStop();
            animator.speed = 0f;
            
            // TODO: Add SFX + VFX
        }

        protected override void OnHitStopEnded()
        {
            animator.speed = 1f;
        }
    }

}