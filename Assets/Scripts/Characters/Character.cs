using System.Collections;
using CameraShake;
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
        public void SetDirection(MoveDirection direction, bool animate = true, float duration = 2f)
        {
            // Debug.Log($"{gameObject.name} Rotation Was Set. Animating = {animate}");
            
            if (animate && !isDirecting) StartCoroutine(DirectionRoutine(direction));
            else
            {
                currentDirection = direction;

                // New Direction
                var targetRotation = currentDirection switch
                {
                    MoveDirection.Left => 180f,
                    MoveDirection.Right => 0f,
                };
                
                rb.MoveRotation(Quaternion.Euler(0f, targetRotation, 0f));
            }
        }

        private IEnumerator DirectionRoutine(MoveDirection direction, float duration = 2f)
        {
            isDirecting =  true;
            var t = 0f;

            // Switch Expression :D
            var currentRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => -0f,
            };
            
            currentDirection = direction;

            // Swapping 
            var targetRotation = currentDirection switch
            {
                MoveDirection.Left => 180f,
                MoveDirection.Right => 0f,
            };
            
            rb.freezeRotation = false;
            
            while (t < duration)
            {
                float lerpT = rotationCurve.Evaluate(t / duration);

                rb.MoveRotation(Quaternion.Euler(0f, Mathf.Lerp(currentRotation, targetRotation, lerpT), 0f));

                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            rb.MoveRotation(Quaternion.Euler(0f, targetRotation, 0f));
            
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
            CameraShaker.Presets.Explosion3D(4f);
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script when this character dies
        /// </summary>
        public virtual void OnDied()
        {
            characterCollider.enabled = false;
            animator.SetTrigger("Died");
            
            // Do Effects
            TriggerHitStop(.5f);
            CameraShaker.Presets.Explosion3D(12f);
        }

        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this characters health is restored
        /// </summary>
        public virtual void OnHealthRestored()
        {
            characterCollider.enabled = true;
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