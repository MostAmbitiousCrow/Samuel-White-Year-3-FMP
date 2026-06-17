using System;
using System.Collections;
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
        private static readonly int Died = Animator.StringToHash("Died");
        private static readonly int TookDamage = Animator.StringToHash("TookDamage");

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
        [SerializeField, ReadOnly] protected bool isGrounded = true;
        public bool IsGrounded => isGrounded;
        [Space]
        [SerializeField] protected LayerMask targetableCharacterLayers;
        [SerializeField] protected LayerMask damageableCharacterLayers;

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
        [SerializeField] protected Vector3 stompSize = Vector3.one;
        [SerializeField] protected Collider characterCollider;
        public Collider CharacterCollider => characterCollider;

        #endregion
        
        #region Directions
        
        [Header("Rotation")]
        [Tooltip("Determines if this character is currently rotating towards a direction")]
        [SerializeField, ReadOnly] private bool isDirecting;
        /// <summary> Determines if this current is currently rotating towards a direction </summary>
        public bool IsDirecting => isDirecting;

        [SerializeField] private AnimationCurve rotationCurve; 

        /// <summary> Reverses the current direction of the enemy </summary>
        public void FlipDirection(bool animate = true)
        {
            switch (currentDirection)
            {
                case MoveDirection.Left: SetDirection(MoveDirection.Right, animate); break;
                case MoveDirection.Right: SetDirection(MoveDirection.Left, animate); break;
            }
        }

        /// <summary> Explicitly sets the direction of the enemy with a given parameter </summary>
        public void SetDirection(MoveDirection direction, bool animate)
        {
            // if (direction == currentDirection) return;
            if (animate) StartCoroutine(DirectionRoutine(direction));
            else
            {
                // isDirecting = true;
                // // Previous Rotation
                // var currentRotation = currentDirection switch
                // {
                //     MoveDirection.Left => 0f,
                //     MoveDirection.Right => 180f,
                //     _ => throw new ArgumentOutOfRangeException()
                // };
            
                currentDirection = direction;

                // New Direction
                var targetRotation = currentDirection switch
                {
                    MoveDirection.Left => 180f,
                    MoveDirection.Right => 0f,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                transform.localRotation = Quaternion.Euler(0f, targetRotation, 0f);
                isDirecting = false;
            }
        }

        private IEnumerator DirectionRoutine(MoveDirection direction, bool animate = true)
        {
            isDirecting =  true;
            var t = 0f;

            // Switch Expression :D
            var currentRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => 0f,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            currentDirection = direction;

            // Swapping 
            var targetRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => 0f,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            rb.freezeRotation = false;

            if (animate)
            {
                while(t < 1f)
                {
                    float y = Mathf.Lerp(currentRotation, targetRotation, rotationCurve.Evaluate(t));
                    transform.localRotation = Quaternion.Euler(0f, y, 0f);
                    t += Time.deltaTime;
                    
                    // Debug.Log($"Is Rotating. Rotation = {y}. Time = {t}");
                    
                    yield return PauseWait;
                }
            }
            transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(currentRotation, targetRotation, 1f), 0f);
            
            rb.freezeRotation = true;
            isDirecting = false;
        }
        #endregion

        #region Damage Events

        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this character takes damage
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void OnTookDamage(DamageType damageType)
        {
            animator.SetTrigger(TookDamage);
            TriggerHitStop();
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script when this character dies
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void OnDied(DamageType damageType)
        {
            characterCollider.enabled = false;
            rb.isKinematic = true;
            animator.SetTrigger(Died);
            TriggerHitStop(.1f);
            // Debug.Log($"{name} Died!. Collider is {characterCollider.enabled}");
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