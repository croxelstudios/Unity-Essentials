using UnityEngine;
using Sirenix.OdinInspector;

public class VectorToTargetEvent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Wether this code should apply movement to the 'origin' or it should just send the movement events elsewhere")]
    bool moveTransform = false;

    [Header("Target")]
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
    [Tooltip("Wether or not the resulting movement vector should be projected onto a 2D plane")]
    bool projectVector = false;
    [SerializeField]
    [ShowIf("projectVector")]
    [Tooltip("Wether or not the projection plane should be calculated in origin's local space")]
    bool projectLocal = false;
    [SerializeField]
    [ShowIf("projectVector")]
    [Tooltip("Projection plane normal")]
    Vector3 planeNormal = Vector3.back;

    [Header("Speed")]
    [SerializeField]
    [Tooltip("Determines how the origin object should follow the target")]
    SpeedMode speedMode = SpeedMode.Linear;
    [SerializeField]
    [Tooltip("The time it takes for the origin object to reach the target's position when moved by this, roughly")]
    [ShowIf("@speedMode == SpeedMode.SmoothDamp")]
    float smoothTime = 0.1f;
    [SerializeField]
    [Tooltip("If this is set to true this code will account for external forces and generate a more" +
        "natural acceleration or smoothDamp curve when the object is exposed to them")]
    [ShowIf("@speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp")] //TO DO: How do I make that work with smoothDamp?
    bool accountForCurrentSpeed = true;
    [SerializeField]
    [Tooltip("Wether or not the resulting vector should be calculated in origin's local space")]
    bool local = false;
    [SerializeField]
    [Tooltip("Wether or not the resulting vector should be sent through the events even if it has zero magnitude")]
    bool sendWhenZeroToo = false;
    [SerializeField]
    [Tooltip("Determines the behaviour of the origin object when close to the target")]
    TargetMode targetMode = TargetMode.MoveToExactPoint;
    [SerializeField]
    [Tooltip("When is this code executed")]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;

    [Space]
    [SerializeField]
    [InspectorName("Move / Look At")]
    [Tooltip("Activates or deactivates the movement to target functionality")]
    bool move = true;
    [SerializeField]
    [ShowIf("move")]
    [OnValueChanged("UpdateSpeedData")]
    [Tooltip("Maximum movement speed")]
    float maxSpeed = 1f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && move")]
    float acceleration = 1f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && move")]
    [Tooltip("Friction multiplier relative to acceleration. Applied when doAccelerate is false. Greater = stops faster")]
    float frictionBias = 0f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && move")]
    [Tooltip("Wether the object is accelerating towards the target or slowly stopping")]
    bool _doAccelerate = true;
    public bool doAccelerate { get { return _doAccelerate; } set { _doAccelerate = value; } }
    [SerializeField]
    [Min(0.0000001f)]
    [ShowIf("@move && (targetMode == TargetMode.StopAtMargin)")]
    [Tooltip("Minimum distance to the target before the resulting vector becomes zero.")]
    float margin = 0.01f;
    [SerializeField]
    [ShowIf("@move && !rotate")]
    bool reorientTransform = false;
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Resulting movement vector in units per second")]
    DXVectorEvent vector = null;
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Resulting speed percentage between zero and max speed")]
    DXFloatEvent magnitudeLerp = null;
    [SerializeField]
    [ShowIf("move")]
    [FoldoutGroup("START and STOP movement")]
    [Tooltip("Resulting vector was zero and is not zero now")]
    DXEvent startedMoving = null;
    [SerializeField]
    [ShowIf("move")]
    [FoldoutGroup("START and STOP movement")]
    [Tooltip("Resulting vector was not zero and is zero now")]
    DXEvent stoppedMoving = null;

    [Space]
    [SerializeField]
    bool rotate = false;
    [SerializeField]
    [ShowIf("rotate")]
    [OnValueChanged("UpdateRotSpeedData")]
    [Tooltip("Activates or deactivates the rotation to target functionality." +
        "This doesn't LOOK AT the target, but copies the target's rotation")]
    float maxRotationSpeed = 1f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && rotate")]
    float angularAcceleration = 1f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && rotate")]
    [Tooltip("Friction multiplier relative to acceleration. Applied when doAccelerate is false. Greater = stops faster")]
    float angularFrictionBias = 0f;
    [SerializeField]
    [ShowIf("@speedMode == SpeedMode.Accelerated && rotate")]
    [Tooltip("Wether the object is accelerating towards the target rotation or slowly stopping")]
    bool _doAngularAccelerate = true;
    public bool doAngularAccelerate { get { return _doAngularAccelerate; } set { _doAngularAccelerate = value; } }
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Minimum distance to the target before the resulting rotation becomes the identity.")]
    float rotationMargin = 1f;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("If set to 'Positive' or 'Negative' it will force the rotation to be in a specific direction even if it's not the fastest")]
    RotationMode rotationMode = RotationMode.Nearest;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Resulting rotation euler angles in degrees per second")]
    DXVectorEvent rotation = null;
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Resulting rotation amount")]
    DXFloatEvent rotationAngle = null;
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

    enum SpeedMode { Linear, Accelerated, SmoothDamp }
    enum TargetMode { MoveToExactPoint, NeverStop, StopAtMargin }
    enum RotationMode { Nearest, Positive, Negative }

    float unsignedMaxSpd;
    int sign;
    int rotSign;
    Vector3 speed;
    Vector3 prevPos;
    float prevSpd;
    float prevRotSpd;

    float unsignedMaxRotSpd;
    Quaternion rotSpeed;
    Quaternion prevRot;

    float tempAngSpd;
    Vector3 accelerationHalf;
    Quaternion rotAccelHalf;

    void UpdateSpeedData()
    {
        unsignedMaxSpd = Mathf.Abs(maxSpeed);
        sign = (int)Mathf.Sign(maxSpeed);
    }

    void UpdateRotSpeedData()
    {
        unsignedMaxRotSpd = Mathf.Abs(maxRotationSpeed);
        rotSign = (int)Mathf.Sign(maxRotationSpeed);
    }

    public void UpdatePrevPos()
    {
        //WARNING: Non dynamic. Must be updated when "local" is changed.
        prevPos = local ? origin.localPosition : origin.position;
        prevRot = local ? origin.localRotation : origin.rotation;
    }

    void OnValidate()
    {
        if (origin == null) origin = transform;
    }

    void Awake()
    {
        speed = Vector3.zero;
        rotSpeed = Quaternion.identity;
        if (useTagForOrigin) origin = GameObject.FindGameObjectWithTag(targetTag)?.transform; //TO DO: Improve this search as with the other
        UpdatePrevPos();

        if (target == null) ResetTarget();
    }

    void OnEnable()
    {
        UpdateSpeedData();
        UpdateRotSpeedData();
        speed = Vector3.zero;
        rotSpeed = Quaternion.identity;
        prevSpd = 0f;
        prevRotSpd = 0f;
        if (timeMode.OnEnable()) CheckEvents(timeMode.DeltaTime());
    }

    void Update()
    {
        if (timeMode.IsSmooth()) OnUpdate();
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) OnUpdate();
    }

    /// <summary>
    /// Looks for tagged objects and sets the target if "targetTag" is specified
    /// </summary>
    public void ResetTarget()
    {
        if (targetTag != "")
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);
            if (objs.Length > 0)
            {
                if (objs[0] != gameObject) target = objs[0].transform;
                else if (objs.Length > 1) target = objs[1].transform;
            }
        }
    }

    /// <summary>
    /// Calculates vectors and checks if the events should be sent.
    /// Then, if "accountForCurrentSpeed" is true,
    /// it calculates current speed based on previous transform position to account for external forces.
    /// </summary>
    void OnUpdate()
    {
        float deltaTime = timeMode.DeltaTime();

        CheckEvents(deltaTime);

        if ((speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp) && (deltaTime != 0f) && accountForCurrentSpeed)
        {
            speed = (((local ? origin.localPosition : origin.position) - prevPos) / deltaTime) + accelerationHalf;
            rotSpeed = rotAccelHalf * (((local ? origin.localRotation : origin.rotation) *
                Quaternion.Inverse(prevRot)).Scale(1f / deltaTime));
        }
        accelerationHalf = Vector3.zero;
        rotAccelHalf = Quaternion.identity;
        UpdatePrevPos();
    }

    /// <summary>
    /// Calculates vectors and checks if the events should be sent.
    /// </summary>
    /// <param name="deltaTime">Last frame deltaTime</param>
    public void CheckEvents(float deltaTime)
    {
        float inverseDeltaTime = (deltaTime != 0f) ? 1f / deltaTime : Mathf.Infinity;
        if (target == null) ResetTarget();
        if ((target != null) && (deltaTime > 0f))
        {
            if (move)
            {
                //Prepare unitsPerSecondSpeed (Movement amount for this frame in units per second) variable for treatment
                //Get positions of origin and target
                Vector3 oPos = origin.position;
                Vector3 tPos = target.position;

                #region If "local": Transform positions to local space
                if (local && (origin.parent != null))
                {
                    oPos = origin.parent.InverseTransformPoint(oPos);
                    tPos = origin.parent.InverseTransformPoint(tPos);
                }
                #endregion

                //Get position difference vector
                Vector3 spd = tPos - oPos;

                #region If "projectVector": Project this vector onto a plane
                if (projectVector)
                {
                    Vector3 localPlaneNormal = projectLocal ? transform.rotation * planeNormal : planeNormal;
                    spd = Vector3.ProjectOnPlane(spd, localPlaneNormal);
                }
                #endregion

                //Get "unitsPerSecondSpeed" => Movement amount for this frame in units per second.
                float mag = spd.magnitude;
                //  InverseDeltaTime is used to convert the raw distance
                //  to the units per second speed required to reach the target next frame.
                float unitsPerSecondSpeed = mag * inverseDeltaTime;
                //Stop movement if the object should StopAtMargin and the distance is less than margin
                if (mag < margin)
                {
                    switch (targetMode)
                    {
                        case TargetMode.StopAtMargin:
                            unitsPerSecondSpeed = 0f;
                            break;
                    }
                }

                //Movement modes calculations
                if (unitsPerSecondSpeed > 0f) //Calculations only done when speed is greater than zero
                    switch (speedMode)
                    {
                        case SpeedMode.Accelerated:
                            if (doAccelerate)
                            {
                                //Speed is calculated on OnUpdate() if "accountForCurrentSpeed" is checked, and is unaltered otherwise
                                //Movement is more accurate if you apply half the acceleration before the movement
                                //and the other half afterwards, so the variable we get here is "accelerationHalf".
                                accelerationHalf = spd.normalized * acceleration * deltaTime * 0.5f;
                            }
                            else //In case the object is not accelerating but it is still preserving momentum
                            {
                                //Reduce speed by the frictionBias factor applied to acceleration, until it is zero.
                                //Friction depends of acceleration, which is not exactly physically accurate but it works better designwise.
                                float accelMag = (acceleration + frictionBias) * deltaTime;
                                //This bit of code makes friction work by the two halfs technique described above too
                                //while also preventing speed from becoming negative.
                                if (accelMag > speed.magnitude)
                                {
                                    float rest = accelMag - speed.magnitude;
                                    float accelMagHalf = accelMag * 0.5f;
                                    if (rest > accelMagHalf)
                                    {
                                        speed = Vector3.zero;
                                        accelMag = 0f;
                                    }
                                    else
                                    {
                                        speed -= speed.normalized * rest;
                                        accelMag = speed.magnitude * 0.5f;
                                    }
                                }
                                accelerationHalf = -speed.normalized * accelMag;
                                //
                            }

                            //Get new movement amount after applying half the acceleration
                            spd = speed + accelerationHalf;
                            unitsPerSecondSpeed = spd.magnitude;

                            //Limit speed by maximum speed
                            if (unitsPerSecondSpeed > unsignedMaxSpd)
                                unitsPerSecondSpeed = unsignedMaxSpd;

                            //Add the other acceleration half to the global speed for use in next frame
                            speed = (spd.normalized * unitsPerSecondSpeed) + accelerationHalf;

                            break;
                        case SpeedMode.SmoothDamp:
                            //Get SmoothDamp speed result
                            spd = Vector3.SmoothDamp(oPos, tPos,
                                ref speed, smoothTime, maxSpeed, deltaTime)
                                - oPos;
                            unitsPerSecondSpeed = spd.magnitude * inverseDeltaTime;
                            break;
                        default:
                            //Default is Linear movement. This just keeps the speed at max value until the distance
                            //is less than unitsPerFrameSpeed (Converted to unitsPerSecond because maxSpeed is in that unit).
                            if ((unitsPerSecondSpeed > unsignedMaxSpd) || (targetMode == TargetMode.NeverStop))
                                unitsPerSecondSpeed = unsignedMaxSpd;
                            break;
                    }

                //Send Events. "unitsPerSecondSpeed" is final speed in units per second
                //Send start or end movement events
                if (prevSpd <= 0f)
                {
                    if (unitsPerSecondSpeed > 0f) startedMoving?.Invoke();
                }
                else if (unitsPerSecondSpeed <= 0f) stoppedMoving?.Invoke();
                prevSpd = unitsPerSecondSpeed;

                if ((unitsPerSecondSpeed > 0f) || sendWhenZeroToo)
                {
                    //Calculate and send percent of speed from zero to max speed for things like animation syncing
                    magnitudeLerp?.Invoke(unitsPerSecondSpeed / unsignedMaxSpd);
                    unitsPerSecondSpeed *= sign;

                    spd = spd.normalized;

                    //Calculate and send vector with direction and amount of speed
                    Vector3 result = spd.normalized * unitsPerSecondSpeed;
                    vector?.Invoke(result);
                    if (moveTransform)
                        origin.Translate(result * deltaTime, local ? Space.Self : Space.World);
                    if (reorientTransform)
                        origin.forward = result;
                }
            }

            if (rotate)
            {
                //Prepare degreesPerSecondSpeed (Rotation amount for this frame in degrees per second) variable for treatment
                //Get rotations of origin and target
                Quaternion oRot = origin.rotation;
                Quaternion tRot = target.rotation;

                #region If "local": Transform rotations to local space
                if (local && (origin.parent != null))
                {
                    oRot = Quaternion.Inverse(origin.parent.rotation) * oRot;
                    tRot = Quaternion.Inverse(origin.parent.rotation) * tRot;
                }
                #endregion

                //Get rotation difference quaternion
                Quaternion spd = tRot * Quaternion.Inverse(oRot);
                //Get "degreesPerSecondSpeed" => Rotation amount in degrees for this frame in degrees per second.
                float absAng = spd.AbsoluteAngle();
                //  InverseDeltaTime is used to convert the raw angle
                //  to the degrees per second speed required to reach the target next frame.
                float degreesPerSecondSpeed = absAng * inverseDeltaTime;
                //Stop rotation if the object should StopAtMargin and the distance is less than margin
                if (degreesPerSecondSpeed < rotationMargin)
                {
                    switch (targetMode)
                    {
                        case TargetMode.StopAtMargin:
                            degreesPerSecondSpeed = 0f;
                            break;
                    }
                }

                //Rotation modes calculations
                if (degreesPerSecondSpeed > 0f) //Calculations only done when speed is greater than zero
                    switch (speedMode)
                    {
                        case SpeedMode.Accelerated:
                            if (doAccelerate)
                            {
                                //Angular speed is calculated on OnUpdate() if "accountForCurrentSpeed" is checked, and is unaltered otherwise.
                                //Rotation is more accurate if you apply half the acceleration before the rotation
                                //and the other half afterwards, so the variable we get here is "rotAccelHalf".
                                rotAccelHalf = spd.SetRotationAmount(angularAcceleration * deltaTime * 0.5f);
                            }
                            else
                            {
                                //Reduce speed by the frictionBias factor applied to acceleration, until it is zero.
                                //Friction depends of acceleration, which is not exactly physically accurate but it works better designwise.
                                float accelMag = (angularAcceleration + angularFrictionBias) * deltaTime;
                                //This bit of code makes friction work by the two halfs technique described above too
                                //while also preventing speed from becoming negative.
                                float spdAbsAng = rotSpeed.AbsoluteAngle();
                                if (accelMag > spdAbsAng)
                                {
                                    float rest = accelMag - spdAbsAng;
                                    float accelMagHalf = accelMag * 0.5f;
                                    if (rest > accelMagHalf)
                                    {
                                        rotSpeed = Quaternion.identity;
                                        accelMag = 0f;
                                    }
                                    else
                                    {
                                        rotSpeed = Quaternion.Inverse(rotSpeed.SetRotationAmount(rest)) * rotSpeed;
                                        accelMag = rotSpeed.AbsoluteAngle() * 0.5f;
                                    }
                                }
                                rotAccelHalf = Quaternion.Inverse(rotSpeed.SetRotationAmount(accelMag));
                                //
                            }

                            //Get new movement amount after applying half the acceleration
                            spd = rotAccelHalf * rotSpeed;
                            degreesPerSecondSpeed = spd.AbsoluteAngle();

                            //Limit speed by maximum speed
                            if (degreesPerSecondSpeed > unsignedMaxRotSpd)
                                degreesPerSecondSpeed = unsignedMaxRotSpd;

                            //Add the other acceleration half to the global speed for use in next frame
                            rotSpeed = rotAccelHalf * spd.SetRotationAmount(degreesPerSecondSpeed);

                            break;
                        //case SpeedMode.SmoothDamp: //TO DO: I tried doing this with different methods but haven't found the perfect one yet
                        //    float oAng = oRot.AbsoluteAngle();
                        //    float tAng = tRot.AbsoluteAngle();
                        //    frameSpeed = (Mathf.SmoothDamp(oAng, tAng, ref tempAngSpd, smoothTime, unsignedMaxRotSpd, deltaTime) - oAng) * inverseDeltaTime;

                        //spd = oRot.SmoothDamp(tRot,
                        //    ref rotSpeed, smoothTime, unsignedMaxRotSpd, deltaTime)
                        //    * Quaternion.Inverse(oRot);
                        //frameSpeed = spd.AbsoluteAngle() * inverseDeltaTime;

                        //break;
                        default:
                            //Default is Linear movement. This just keeps the angular speed at max value until the distance
                            //is less than degreesPerFrameSpeed (Converted to degreesPerSecond because maxSpeed is in that unit).
                            if ((degreesPerSecondSpeed > unsignedMaxRotSpd) || (targetMode == TargetMode.NeverStop))
                                degreesPerSecondSpeed = unsignedMaxRotSpd;
                            break;
                    }

                //Send Events. "degreesPerSecondSpeed" is final angular speed in degrees per second
                //Send start or end movement events
                if (prevRotSpd <= 0f)
                {
                    if (degreesPerSecondSpeed > 0f) startedRotating?.Invoke();
                }
                else if (degreesPerSecondSpeed <= 0f) stoppedRotating?.Invoke();
                prevRotSpd = degreesPerSecondSpeed;

                if ((degreesPerSecondSpeed > 0f) || sendWhenZeroToo)
                {
                    Vector3 euler;
                    float sign;
                    //Modify rotation direction based on positive or negative modes. Default is shortest rotation to target.
                    switch (rotationMode) // TO DO: This works strangely with SmoothDamp mode
                    {
                        case RotationMode.Positive:
                            euler = spd.eulerAngles;
                            sign = Mathf.Sign(euler.x.OffsetedRepeat(360f, -180f) + euler.y.OffsetedRepeat(360f, -180f) + euler.z.OffsetedRepeat(360f, -180f));
                            if (sign < 0f) spd = Quaternion.Inverse(spd);
                            break;
                        case RotationMode.Negative:
                            euler = spd.eulerAngles;
                            sign = Mathf.Sign(euler.x.OffsetedRepeat(360f, -180f) + euler.y.OffsetedRepeat(360f, -180f) + euler.z.OffsetedRepeat(360f, -180f));
                            if (sign > 0f) spd = Quaternion.Inverse(spd);
                            break;
                        default:
                            break;
                    }

                    //Calculate and send percent of angular speed from zero to max speed for things like animation syncing
                    rotationAngle?.Invoke(degreesPerSecondSpeed / unsignedMaxRotSpd);
                    degreesPerSecondSpeed *= rotSign;

                    spd = spd.SetRotationAmount(1f);

                    //Calculate and send euler angles with direction and amount of rotation
                    rotation?.Invoke(spd.Scale(degreesPerSecondSpeed).eulerAngles);

                    //Rotate transform if "moveTransform" is checked
                    if (moveTransform)
                        origin.Rotate(spd.Scale(degreesPerSecondSpeed * deltaTime).eulerAngles, local ? Space.Self : Space.World);
                }
            }
        }
    }

    /// <summary>
    /// Instantly moves the origin transform to the target position
    /// </summary>
    public void Teleport()
    {
        Vector3 dif = target.position - origin.position;
        origin.Translate(dif, Space.World);
        speed = Vector3.zero;
    }
}
