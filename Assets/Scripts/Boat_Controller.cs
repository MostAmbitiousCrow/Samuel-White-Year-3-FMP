using UnityEngine;

public class Boat_Controller : River_Object, IDamageable
{
    [Header("Boat Settings")]
    public float steerSpeed = 1;
    public AnimationCurve SteerInterpolationCurve;
    [Tooltip("Determines if the boat is currently moving")]
    public bool IsMoving { get; private set; }
    [Space(10)]
    [Tooltip("The duration of the stun after hitting an obstacle")]
    [SerializeField] float stunDuration = 1.5f;
    [Tooltip("How much of the current boats speed is decreased when an obstacle is hit")]
    [SerializeField] float stunSlowMultiplier = .5f;

    /// <summary>
    /// Steers the players boat in a given direction and animates the force
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    public void SteerBoat(int direction, float force)
    {
        print($"Steered Board in the {direction} direction");
        MoveTowardsLane(direction);
    }

    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException(); //TODO: Implement Stun Effect and Animations
    }
}
