using UnityEngine;
using EditorAttributes;
using UnityEngine.InputSystem;

public class Player_Driver_Controller : MonoBehaviour
{
    [Header("Steering Controls")]
    public bool canMove = true;
    [SerializeField] private Vector2 _inputDirection = new();
    [Space(10)]
    [SerializeField] private Vector3 _steerTarget = new();
    [SerializeField] private float _heightOffset = .2f;
    [Space(10)]
    [Tooltip("")] public float steerSpeed = 5;
    [Tooltip("")] public float steerAcceleration = 5;
    [Tooltip("")] public float steerDistance = 5;
    [Space(10)]
    [Tooltip("")] public float vehicleSpeed = 2;

    [Header("Components")]
    [SerializeField] Rigidbody rb;

    private Vector3 roadCenter;

    private readonly PlayerInput _input;

    // Cached action references
    private InputAction _steerAction;
    private InputAction _actionAction;
    // private InputAction attackAction;

    [Header("Debug")]
    [SerializeField] private bool _enableDebug = true;

    void Awake()
    {
        // Subscribe to the PlayerInput component's events
        PlayerInput input = GetComponent<PlayerInput>();

        // Cache actions from the provided InputActionAsset
        _steerAction = input.actions["Steer"];
        _actionAction = input.actions["Action"];

        _steerAction.performed += OnSteerPerformed;
        _steerAction.canceled += OnSteerCanceled;
        _actionAction.performed += OnAction;

        roadCenter = rb.position;
        _steerTarget = roadCenter + new Vector3(0, _heightOffset, 0);
    }

    // ==================================================
    // Input Events:

    private void OnSteerPerformed(InputAction.CallbackContext context)
    {
        _inputDirection = context.ReadValue<Vector2>();
    }

    private void OnSteerCanceled(InputAction.CallbackContext context)
    {
        _inputDirection = Vector2.zero;
    }

    void OnAction(InputAction.CallbackContext context)
    {
        return; // TODO Add action events
    }


    // ==================================================

    void Update()
    {
        MoveSteerTarget();
    }

    void FixedUpdate()
    {
        SteerVehicle();
    }

    void MoveSteerTarget()
    {
        // Calculate the desired target X position based on input
        float targetX = rb.position.x + (_inputDirection.x * steerDistance);

        // Smoothly interpolate _steerTarget.x towards targetX
        _steerTarget.x = Mathf.Lerp(_steerTarget.x, targetX, steerAcceleration * Time.deltaTime);

        // Clamp the steer target within allowed range
        _steerTarget.x = Mathf.Clamp(_steerTarget.x, roadCenter.x - steerDistance, roadCenter.x + steerDistance);

        // Maintain Y and Z
        _steerTarget.y = rb.position.y + _heightOffset;
        _steerTarget.z = rb.position.z;
    }

    void SteerVehicle()
    {
        rb.velocity = new Vector2(rb.velocity.x + (_steerTarget.x - rb.position.x) * vehicleSpeed * Time.deltaTime, rb.velocity.y);
    }

    // ==================================================
    // Debug

    private void OnDrawGizmos()
    {
        if (!_enableDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_steerTarget, 1);
    }
}
