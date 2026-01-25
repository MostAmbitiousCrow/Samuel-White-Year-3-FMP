using System.Collections;
using UnityEngine;
using EditorAttributes;
using UnityEngine.Serialization;

namespace GameCharacters
{
    /// <summary>
    /// The root class of all characters
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CharacterHealth))]
    public abstract class Character : MonoTimeBehaviour
    {
        #region Variables
        //TODO: Convert majority movement variables to a scriptable object
        [Title("Character")]
        [Line(GUIColor.White)]
        
        [Header("Movement")]
        public bool canMove;
        [Tooltip("The time it takes for the character to move to their next targeted space whilst grounded")]
        [SerializeField, ShowField(nameof(canMove))] protected float groundedMovementTime = .2f;
        [Tooltip("The curve controlling the ground movement animation of the character")]
        [SerializeField, ShowField(nameof(canMove))] protected AnimationCurve groundedMovementCurve;

        [Tooltip("The time remaining of the characters movement they're able to immediately move again")]
        [SerializeField] protected float coyoteTime = .15f;
        [SerializeField, ReadOnly] protected bool coyoteTriggered;

        protected float MovementTimeElapsed = 0f;

        [Space]

        [Tooltip("The time it takes for the character to move to their next targeted space whilst in the air")]
        [SerializeField, ShowField(nameof(canMove))] protected float airMovementTime = .25f;
        [Tooltip("The curve controlling the air movement animation of the character")]
        [SerializeField, ShowField(nameof(canMove))] protected AnimationCurve airMovementCurve;

        [Space]

        [Tooltip("The vertical distance from the characters current space or lane")]
        // Must be set specifically by either Boat or RiverLane Characters!
        [SerializeField, ReadOnly] protected float verticalDistance;
        [Tooltip("The vertical offset of the character from their current space or lane. Modify this for flying enemies.")]
        [SerializeField] protected float movementVerticalOffset = 0f;
        
        [SerializeField, ReadOnly] protected MoveDirection currentDirection = MoveDirection.Left;
        public  MoveDirection CurrentDirection => currentDirection;
        /// <summary> Left = 1 | Right = -1 </summary>
        public enum MoveDirection { Right = 1, Left = -1 }

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
        [SerializeField] private Transform stompPosition;
        public Transform StompPosition => stompPosition;
        [SerializeField] protected Collider characterCollider;
        public Collider CharacterCollider => characterCollider;

        #endregion
        
        #region Directions
        
        [Header("Rotation")]
        [Tooltip("Determines if this character is currently rotating towards a direction")]
        [SerializeField, ReadOnly] private bool isDirecting;
        /// <summary> Determines if this current is currently rotating towards a direction </summary>
        public bool IsDirecting { get { return isDirecting; } }

        [SerializeField] private AnimationCurve rotationCurve; 

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
        public void SetDirection(MoveDirection direction, bool animate = true)
        {
            // currentDirection = direction;
            
            StartCoroutine(DirectionRoutine(direction, animate));
        }

        private IEnumerator DirectionRoutine(MoveDirection direction, bool animate)
        {
            isDirecting =  true;
            var t = 0f;

            float currentRotation, targetRotation;

            // Switch Expression :D
            currentRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => 0f,
            };
            
            currentDirection = direction;
            
            // Swapping 
            targetRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => 0f,
            };
            
            rb.freezeRotation = false;

            if (animate)
            {
                while(t < 1f)
                {
                    rb.MoveRotation(Quaternion.Euler
                    (0f, 
                        Mathf.Lerp(currentRotation, targetRotation, rotationCurve.Evaluate(t)),
                    0f));
                    t += Time.deltaTime;
                    yield return PauseWait;
                }
            }
            
            rb.MoveRotation(Quaternion.Euler
            (0f, Mathf.Lerp(currentRotation, targetRotation, 1f), 1f));
            
            rb.freezeRotation = true;
            
            isDirecting = false;
        }
        #endregion

        #region Damage Events
        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this character takes damage
        /// </summary>
        public virtual void OnTookDamage()
        {
            animator.SetTrigger("TookDamage");
            TriggerHitStop(.1f);
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script when this character dies
        /// </summary>
        public virtual void OnDied()
        {
            animator.SetTrigger("Died");
            TriggerHitStop(.5f);
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
            // TODO: Add SFX + VFX
        }

        protected override void OnHitStopEnded()
        {
            base.OnHitStopEnded();
        }
    }

}