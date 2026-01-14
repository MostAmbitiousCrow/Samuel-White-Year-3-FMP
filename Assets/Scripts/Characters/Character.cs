using System.Collections;
using System.Collections.Generic;
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

        [Header("Movement")]
        [SerializeField] protected bool canMove;
        [Tooltip("The time it takes for the character to move to their next targetted space whilst grounded")]
        [SerializeField] protected float groundedMovementTime = .2f;
        [Tooltip("The curve controlling the ground movement animation of the character")]
        [SerializeField] protected AnimationCurve groundedMovementCurve;

        [Tooltip("The time remaining of the characters movement they're able to immediately move again")]
        [SerializeField] protected float coyoteTime = .15f;
        [SerializeField, ReadOnly] protected bool coyoteTriggered;

        protected float _movementTimeElapsed = 0f;

        [Space]

        [Tooltip("The time t takes for the character to move to their next targetted space whilst in the air")]
        [SerializeField] protected float airMovementTime = .25f;
        [Tooltip("The curve controlling the air movement animation of the character")]
        [SerializeField] protected AnimationCurve airMovementCurve;

        [Space]

        [Header("World")]
        [Tooltip("The gravity of the character")]
        [SerializeField] protected float gravity = 1f;
        [Tooltip("The fall speed of the character")]
        [SerializeField] protected float fallSpeed = 6f;

        [Space]

        [Tooltip("The vertical distance from the characters current space or lane")]
        // Must be set specifically by either Boat or RiverLane Characters!
        [SerializeField, ReadOnly] protected float verticalDistance;

        [Header("Checks")]
        [SerializeField, ReadOnly] protected bool isMoving;
        [SerializeField, ReadOnly] protected bool isGrounded;
        [Space]
        [SerializeField] protected LayerMask targetableCharacterLayers;

        [Header("Components")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected Animator anim;
        [SerializeField] protected CharacterHealth healthComponent;

        #endregion

        #region Damage Events
        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this character takes damage
        /// </summary>
        public virtual void RecieveTookDamage()
        {

        }

        /// <summary>
        /// Event Called by the CharacterHealth Script when this character dies
        /// </summary>
        public virtual void RecieveDied()
        {

        }

        /// <summary>
        /// Event Called by the CharacterHealth Script whenever this characters health is restored
        /// </summary>
        public virtual void RecieveHealthRestored()
        {

        }
        #endregion
    }

}