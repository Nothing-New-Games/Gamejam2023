using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CameraTargetTracking : MonoBehaviour
{
    [InfoBox("Target is not assigned and is considered a crucial component! Is this intentional?", InfoMessageType = InfoMessageType.Warning, VisibleIf = "@target == null")]
    public Transform target;

    private bool _cameraStarted = false;

    #region Customization Tab
    [TabGroup("Customization"), Tooltip("Determines the positioning offset of the camera's resting pos based on the target's current pose.")]
    public Vector3 offset;
    [TabGroup("Customization"), MinValue(0), Tooltip("What is the max distance from the target the camera can be before it starts to move?")]
    public float maxDistanceToTarget = 5f;
    [TabGroup("Customization"), MinValue(0), MaxValue(100), Tooltip("What is the rate of movement of the camera?")]
    public float movementRate = 1f;
    [TabGroup("Customization"), MinValue(0), MaxValue(10000), Tooltip("How fast can the camera rotate to face the target?")]
    public float rotationSpeed = 100f;
    [TabGroup("Customization"), Range(0.001f, 10)]
    public float minStrength = 0.1f;
    #endregion

    #region Position Lock Tab
    [TabGroup("Position Locks")]
    public bool lockXPos = false;
    [TabGroup("Position Locks")]
    public bool lockZPos = false;
    [TabGroup("Position Locks")]
    public bool lockYPos = false;

    [TabGroup("Position Locks"), ReadOnly]
    public float XPosLock;
    [TabGroup("Position Locks"), ReadOnly]
    public float YPosLock;
    [TabGroup("Position Locks"), ReadOnly]
    public float ZPosLock;
    #endregion

    #region Rotation Lock Tab
    [TabGroup("Rotation Locks")]
    public bool lockRotorXPos = false;
    [TabGroup("Rotation Locks")]
    public bool lockRotorZPos = false;
    [TabGroup("Rotation Locks")]
    public bool lockRotorYPos = false;

    [TabGroup("Rotation Locks"), ReadOnly]
    public float XRotorPosLock;
    [TabGroup("Rotation Locks"), ReadOnly]
    public float YRotorPosLock;
    [TabGroup("Rotation Locks"), ReadOnly]
    public float ZRotorPosLock;
    #endregion

    #region Debug Tab
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private float _movementStrength = 0f;
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private float _currentDist;
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private float _rotationStrength = 0f;

    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private Vector3 _destination;
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private Vector3 _targetDirection;
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private Quaternion _desiredRotation;
    [ShowInInspector, TabGroup("Debug"), ReadOnly]
    private Quaternion _currentRotation;
    [ShowInInspector, TabGroup("Debug")]
    private bool _isDebuggingEnabled = false;
    #endregion

    void Start()
    {
        if (target == null)
            Debug.LogWarning($"{name} has no target! Is this intentional?");

        _cameraStarted = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        CalculateStrength();

        _targetDirection = target.position - transform.position;
        _desiredRotation = Quaternion.LookRotation(_targetDirection);
        _currentRotation = transform.rotation;
        transform.rotation =
            Quaternion.RotateTowards
            (
                transform.rotation,
                _desiredRotation,
                _rotationStrength * rotationSpeed * Time.fixedDeltaTime
            );
        //transform.rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, Time.fixedDeltaTime * rotationSpeed);

        _currentDist = Vector3.Distance(gameObject.transform.position, target.position + offset);
        if (_movementStrength > minStrength)
        {
            CalculateDest();

            transform.position =
                Vector3.Slerp
                (
                    transform.position,
                    _destination,
                    _movementStrength * movementRate * Time.fixedDeltaTime
                );
        }

        HandlePosLocks();
    }

    /// <summary>
    /// Sets locks for the camera on specified planes when they are activated.
    /// </summary>
    private void HandlePosLocks()
    {
        #region Capturing Axis Coordinates
        //If a coordinate for the axis being locked was not captured, capture one.
        if (lockXPos && XPosLock == 0)
            XPosLock = transform.position.x;
        if (lockYPos && YPosLock == 0)
            YPosLock = transform.position.y;
        if (lockZPos && ZPosLock == 0)
            ZPosLock = transform.position.z;
        //Do the same for Rotation.
        if (lockRotorXPos && XRotorPosLock == 0)
            XRotorPosLock = transform.rotation.eulerAngles.x;
        if (lockRotorYPos && YRotorPosLock == 0)
            YRotorPosLock = transform.rotation.eulerAngles.y;
        if (lockRotorZPos && ZRotorPosLock == 0)
            ZRotorPosLock = transform.rotation.eulerAngles.z;
        #endregion

        #region Locking the Axis
        //Use the captured coordinate to lock the corresponding axis.
        if (lockXPos)
            transform.position = new Vector3(XPosLock, transform.position.y, transform.position.z);
        else XPosLock = 0;
        if (lockYPos)
            transform.position = new Vector3(transform.position.x, YPosLock, transform.position.z);
        else YPosLock = 0;
        if (lockZPos)
            transform.position = new Vector3(transform.position.x, transform.position.y, ZPosLock);
        else ZPosLock = 0;
        //Do the same for the Rotation.
        if (lockRotorXPos)
            transform.rotation = Quaternion.Euler(XRotorPosLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        else XRotorPosLock = 0;
        if (lockRotorYPos)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, YRotorPosLock, transform.rotation.eulerAngles.z);
        else YRotorPosLock = 0;
        if (lockRotorZPos)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, ZRotorPosLock);
        else ZRotorPosLock = 0;
        #endregion
    }

    /// <summary>
    /// Calculates the strength of the camera movement.
    /// </summary>
    private void CalculateStrength()
    {
        //Sets the strength for the camera movement speed based on the distance to it's target.
        _movementStrength = (float)Math.Round(Mathf.Clamp(
                        _currentDist / maxDistanceToTarget,
                        0, 1), 1);
        //Sets the strength for the speed the camera rotates based on the distance it has to rotate to face it's target.
        _rotationStrength = (float)Math.Round(Mathf.Clamp(
                        Quaternion.Angle(_currentRotation, _desiredRotation),
                        0, 1), 1);

        #region Debug Calculations
        //Color codes the Gizmo to represent the values the camera is using to determine how it moves.
        if (_movementStrength < minStrength)
            StrengthIndicator = Color.green;
        else if (_movementStrength < minStrength + 0.25f)
            StrengthIndicator = Color.yellow;
        else if (_movementStrength <= 1f)
            StrengthIndicator = Color.red;
        #endregion
    }

    /// <summary>
    /// Calculates the desired destination of the camera.
    /// </summary>
    private void CalculateDest() =>
        _destination = target.position + offset;

    //[SerializeField]
    private float GizmoSize = 10f;
    private Color StrengthIndicator = Color.black;
    private void OnDrawGizmos()
    {
        if (_isDebuggingEnabled)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        if (!_cameraStarted) return;
        if (!_isDebuggingEnabled) return;
        if (target == null) return;

        Gizmos.color = StrengthIndicator;
        Gizmos.DrawWireSphere(target.transform.position, _movementStrength * GizmoSize);
        Gizmos.DrawCube(transform.forward, new Vector3(0.1f, 0.1f, 0.1f));
    }
}
