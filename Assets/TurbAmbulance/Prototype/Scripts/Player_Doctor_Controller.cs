using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerDoctorArmData
{
    public Rigidbody ArmHandle;
    public Transform ArmShoulder;

    public Vector3 ArmTarget = new();

    public bool IsGrabbing = false;
    public Collider GrabbedSurface;

    public Vector2 InputDirection = new();

    public float ArmDistance = 0.025f;
    public float ArmInterpolation = 1f;
    public float ArmSpeed = 5f;
}

public class Player_Doctor_Controller : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerDoctorArmData[] ArmData = new PlayerDoctorArmData[2];
    [SerializeField] private float _armRotationOffset = 180f;

    [Header("Input")]
    private InputAction _r_Aim;
    private InputAction _l_Aim;
    private InputAction _r_Grab;
    private InputAction _l_Grab;

    [Header("Debug")]
    [SerializeField] private bool _enableDebug = true;

    void Awake()
    {
        // Subscribe to the PlayerInput component's events
        PlayerInput input = GetComponent<PlayerInput>();

        // Save actions from the provided InputActionAsset
        _r_Aim = input.actions["Aim Right"];
        _l_Aim = input.actions["Aim Left"];

        _r_Grab = input.actions["Grab Right"];
        _l_Grab = input.actions["Grab Left"];

        // Right Inputs

        _r_Aim.performed += OnAimRightPerformed;
        _r_Aim.canceled += OnAimRightCancelled;

        _r_Grab.performed += OnGrabRightPerformed;
        _r_Grab.canceled += OnGrabRightCancelled;

        // Left Inputs

        _l_Aim.performed += OnAimLeftPerformed;
        _l_Aim.canceled += OnAimLeftCancelled;

        _l_Grab.performed += OnGrabLeftPerformed;
        _l_Grab.canceled += OnGrabLeftCancelled;
    }

    void FixedUpdate()
    {
        foreach (var item in ArmData)
        {
            // Calculate the desired target X position based on input
            Vector3 targetPos = new(item.ArmTarget.x + (item.InputDirection.x * item.ArmDistance), item.ArmTarget.y,
            item.ArmTarget.z + (item.InputDirection.y * item.ArmDistance));

            // Smoothly interpolate towards targetPos
            item.ArmTarget = Vector3.Lerp(item.ArmHandle.position, targetPos, item.ArmInterpolation * Time.deltaTime);

            item.ArmTarget = ClampCircle(item.ArmTarget, item.ArmShoulder.position, item.ArmDistance);

            item.ArmHandle.MovePosition(item.ArmTarget);

            if (item.InputDirection != Vector2.zero)
            {
                Vector3 direction = new Vector3(item.InputDirection.x, 0, item.InputDirection.y).normalized;

                // Add an offset to the rotation y axis
                Quaternion offset = Quaternion.Euler(0, _armRotationOffset, 0);
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up) * offset;

                item.ArmHandle.MoveRotation(Quaternion.Slerp(item.ArmHandle.rotation, targetRotation, item.ArmInterpolation * Time.deltaTime));
            }
        }
    }

    // ==================================================
    // Clamp the Aim Target in a circle shape

    Vector3 ClampCircle(Vector3 a, Vector3 center, float radius)
    {
        Vector3 toCircle = a - center;

        if (toCircle.magnitude > radius) return center + Vector3.ClampMagnitude(toCircle, radius);
        else return a;
    }

    // ==================================================
    // Input Events:

    // Aim RIGHT Input Events:

    private void OnAimRightPerformed(InputAction.CallbackContext context)
    {
        ArmData[0].InputDirection = context.ReadValue<Vector2>();
    }

    private void OnAimRightCancelled(InputAction.CallbackContext context)
    {
        ArmData[0].InputDirection = Vector2.zero;
    }

    // Grab RIGHT Input Events:

    private void OnGrabRightPerformed(InputAction.CallbackContext context)
    {
        ArmData[0].IsGrabbing = true;
    }

    private void OnGrabRightCancelled(InputAction.CallbackContext context)
    {
        ArmData[0].IsGrabbing = false;
    }

    // Aim LEFT Input Events:

    private void OnAimLeftPerformed(InputAction.CallbackContext context)
    {
        ArmData[1].InputDirection = context.ReadValue<Vector2>();
    }

    private void OnAimLeftCancelled(InputAction.CallbackContext context)
    {
        ArmData[1].InputDirection = Vector2.zero;
    }

    // Grab LEFT Input Events:

    private void OnGrabLeftPerformed(InputAction.CallbackContext context)
    {
        ArmData[1].IsGrabbing = true;
    }

    private void OnGrabLeftCancelled(InputAction.CallbackContext context)
    {
        ArmData[1].IsGrabbing = false;
    }
    
    // ==================================================
    // Debug

    private void OnDrawGizmos()
    {
        if (!_enableDebug) return;

        Gizmos.color = Color.red;
        foreach (var item in ArmData)
        {
            Gizmos.DrawWireSphere(item.ArmTarget, 1);
        }
    }
}
