using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

public class RotationToTargetEvent_UNFINISHED : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Wether this code should apply rotation to the 'origin' or it should just send the rotation events elsewhere")]
    bool rotateTransform = false;

    //TO DO: Should maybe get this into a global structure like I did with speedbehaviour
    [Header("Target")]
    #region Target
    [SerializeField]
    [TagSelector]
    [Tooltip("Tag used to find the transform in case it is not specified")]
    string targetTag = "Player";
    [SerializeField]
    [Tooltip("True: The target is found through the tag. False: The 'origin' is found through the tag")]
    bool useTagForOrigin = false;
    [SerializeField]
    [Tooltip("Positional and rotational target transform")]
    Transform target = null;
    public Transform Target { get { return target; } set { target = value; } }
    [SerializeField]
    [Tooltip("The transform that will move or the transform that the movement is calculated from. By default, this object's transform.")]
    Transform origin = null;
    [Space]
    [SerializeField]
    [Tooltip("Wether or not the resulting action should be projected onto a 2D plane")]
    bool projection = false;
    [SerializeField]
    [ShowIf("projection")]
    [Tooltip("Wether or not the projection plane should be calculated in origin's local space")]
    bool projectionLocal = false;
    [SerializeField]
    [ShowIf("projection")]
    [Tooltip("Projection plane normal")]
    Vector3 planeNormal = Vector3.back;
    #endregion

    [Header("Speed behaviour")]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    SpeedBehaviour speedBehaviour = new SpeedBehaviour(SpeedBehaviour.SpeedMode.Linear);

    [SerializeField]
    bool local = false;
    [SerializeField]
    bool sendWhenZeroToo = false;
    [SerializeField]
    TargetMode targetMode = TargetMode.ToExactPoint;
    [SerializeField]
    [HideIf("@speedMode == SpeedMode.Teleport")]
    [Tooltip("When is this code executed")]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Minimum distance to the target before the resulting rotation becomes the identity.")]
    float margin = 1f;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("If set to 'Positive' or 'Negative' it will force the rotation to be in a specific direction even if it's not the fastest")]
    RotationMode rotationMode = RotationMode.Shortest;

    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Resulting rotation euler angles in degrees per second")]
    DXRotationEvent rotation = null;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Resulting rotation amount")]
    DXFloatEvent angleSpeedPercent = null;
    [SerializeField]
    [ShowIf("rotate")]
    [FoldoutGroup("START and STOP rotation")]
    [Tooltip("Resulting rotation was zero and is not zero now")]
    DXEvent startedRotating = null;
    [SerializeField]
    [ShowIf("rotate")]
    [FoldoutGroup("START and STOP rotation")]
    [Tooltip("Resulting rotation was not zero and is zero now")]
    DXEvent stoppedRotating = null;

    enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    float unsignedSpd;
    float unsignedMaxSpd;
    float prevSpd;
    Quaternion speed;
    Quaternion prevRot;
    Quaternion accelHalf;

    void UpdateSpeedData()
    {
        unsignedMaxSpd = Mathf.Abs(speedBehaviour.GetSpeed());
    }

    public void UpdatePrevPos()
    {
        //WARNING: Non dynamic. Must be updated when "local" is changed.
        prevRot = origin.Rotation(local);
    }

    void OnValidate()
    {
        if (origin == null) origin = transform;
    }

    void Awake()
    {
        speed = Quaternion.identity;
        if (useTagForOrigin) origin = FindWithTag.TrCheckEmpty(targetTag);
        UpdatePrevPos();

        if (target == null) ResetTarget();
    }

    void OnEnable()
    {
        UpdateSpeedData();
        speed = Quaternion.identity;
        prevSpd = 0f;
        if (timeMode.OnEnable()) CheckEvents(timeMode.DeltaTime());
    }

    /// <summary>
    /// Looks for tagged objects and sets the target if "targetTag" is specified
    /// </summary>
    public void ResetTarget()
    {
        target = FindWithTag.TrCheckEmpty(targetTag);
    }

    /// <summary>
    /// Calculates vectors and checks if the events should be sent.
    /// </summary>
    /// <param name="deltaTime">Last frame deltaTime</param>
    public void CheckEvents(float deltaTime)
    {
        if (target == null) ResetTarget();
        //if ((target != null) && (deltaTime > 0f))
        //    Rotate(deltaTime);
    }

    void Update()
    {
        //if (timeMode.IsSmooth()) OnUpdate();
    }

    void FixedUpdate()
    {
        //if (timeMode.IsFixed()) OnUpdate();
    }
}
