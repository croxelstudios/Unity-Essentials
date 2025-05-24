using UnityEngine;
using Sirenix.OdinInspector;
using static SpeedBehaviour;

public class RotationToTargetEvent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Wether this code should apply rotation to the 'origin' or it should just send the rotation events elsewhere")]
    bool rotateTransform = false;

    //TO DO: Should maybe get this into a global structure like I did with speedbehaviour
    [Header("Target")]
    #region Target
    [SerializeField]
    [HideLabel]
    [InlineProperty]
    OriginTarget originTarget = new OriginTarget("Player");
    public Transform target { get { return originTarget.target; } set { originTarget.SetTarget(value); } }
    public Transform origin { get { return originTarget.origin; } set { originTarget.SetOrigin(value); } }
    [SerializeField]
    [Tooltip("Wether or not the resulting action should be projected onto a 2D plane")]
    bool projectOnPlane = false;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    [Tooltip("Wether or not the projection plane should be calculated in origin's local space")]
    bool projectionLocal = false;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    [Tooltip("Projection plane normal")]
    Vector3 planeNormal = Vector3.back;
    #endregion

    [Header("Speed behaviour")]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    SpeedBehaviour speedBehaviour = new SpeedBehaviour(SpeedMode.Linear);

    [SerializeField]
    bool local = false;
    [SerializeField]
    bool sendWhenZeroToo = false;
    [SerializeField]
    TargetMode targetMode = TargetMode.ToExactPoint;
    [SerializeField]
    [Tooltip("When is this code executed")]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [SerializeField]
    [ShowIf("@targetMode == TargetMode.StopAtMargin")]
    [Tooltip("Minimum distance to the target before the resulting rotation becomes the identity.")]
    float margin = 1f;
    [SerializeField]
    [Tooltip("If set to 'Positive' or 'Negative' it will force the rotation to be in a specific direction even if it's not the fastest")]
    RotationMode rotationMode = RotationMode.Shortest;
    [SerializeField]
    bool sendFrameMovement = false;

    [SerializeField]
    [Tooltip("Resulting rotation euler angles in degrees per second")]
    DXRotationEvent rotation = null;
    [SerializeField]
    [Tooltip("Resulting rotation amount")]
    DXFloatEvent angleSpeedPercent = null;
    [SerializeField]
    [FoldoutGroup("START and STOP rotation")]
    [Tooltip("Resulting rotation was zero and is not zero now")]
    DXEvent startedRotating = null;
    [SerializeField]
    [FoldoutGroup("START and STOP rotation")]
    [Tooltip("Resulting rotation was not zero and is zero now")]
    DXEvent stoppedRotating = null;

    enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    float prevSpd;
    Quaternion speed;
    Quaternion prevRot;
    Quaternion accelHalf;

    public void UpdatePrevPos()
    {
        //WARNING: Non dynamic. Must be updated when "local" is changed.
        prevRot = origin.Rotation(local);
    }

    void Reset()
    {
        originTarget.SetOrigin(transform);
    }

    void Awake()
    {
        speed = Quaternion.identity;
        UpdatePrevPos();
    }

    void OnEnable()
    {
        speed = Quaternion.identity;
        prevSpd = 0f;
        if (timeMode.OnEnable()) CheckEvents(timeMode.DeltaTime());
    }

    /// <summary>
    /// Calculates vectors and checks if the events should be sent.
    /// </summary>
    /// <param name="deltaTime">Last frame deltaTime</param>
    public void CheckEvents(float deltaTime)
    {
        if (originTarget.IsNotNull() && (deltaTime > 0f))
            Rotate(deltaTime);
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
    /// Calculates vectors and checks if the events should be sent.
    /// Then, if "accountForCurrentSpeed" is true,
    /// it calculates current speed based on previous transform position to account for external forces.
    /// </summary>
    void OnUpdate()
    {
        float deltaTime = timeMode.DeltaTime();
        CheckEvents(deltaTime);
        if (speedBehaviour.AffectedByCurrentSpeed() && (deltaTime != 0f))
        {
            speed = accelHalf *
                (origin.Rotation(local) * Quaternion.Inverse(prevRot)).Scale(1f / deltaTime);
        }
        accelHalf = Quaternion.identity;

        UpdatePrevPos();
    }


    void Rotate(float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        RotationPath rotPath = GetRotation();

        Quaternion spd = Quaternion.identity;
        if (ShouldIRotate(rotPath))
            switch (speedBehaviour.speedMode)
            {
                case SpeedMode.Accelerated:
                    //if (doAccelerate)
                    //{
                    //    //Angular speed is calculated on OnUpdate() if "accountForCurrentSpeed" is checked, and is unaltered otherwise.
                    //    //Rotation is more accurate if you apply half the acceleration before the rotation
                    //    //and the other half afterwards, so the variable we get here is "rotAccelHalf".
                    //    rotAccelHalf = spd.SetRotationAmount(angularAcceleration * deltaTime * 0.5f);
                    //}
                    //else
                    //{
                    //    //Reduce speed by the frictionBias factor applied to acceleration, until it is zero.
                    //    //Friction depends of acceleration, which is not exactly physically accurate but it works better designwise.
                    //    float accelMag = (angularAcceleration + angularFrictionBias) * deltaTime;
                    //    //This bit of code makes friction work by the two halfs technique described above too
                    //    //while also preventing speed from becoming negative.
                    //    float spdAbsAng = rotSpeed.Angle();
                    //    if (accelMag > spdAbsAng)
                    //    {
                    //        float rest = accelMag - spdAbsAng;
                    //        float accelMagHalf = accelMag * 0.5f;
                    //        if (rest > accelMagHalf)
                    //        {
                    //            rotSpeed = Quaternion.identity;
                    //            accelMag = 0f;
                    //        }
                    //        else
                    //        {
                    //            rotSpeed = Quaternion.Inverse(rotSpeed.SetRotationAmount(rest)) * rotSpeed;
                    //            accelMag = rotSpeed.Angle() * 0.5f;
                    //        }
                    //    }
                    //    rotAccelHalf = Quaternion.Inverse(rotSpeed.SetRotationAmount(accelMag));
                    //    //
                    //}

                    ////Get new movement amount after applying half the acceleration
                    //spd = rotAccelHalf * rotSpeed;
                    //degreesPerSecondSpeed = spd.Angle();

                    ////Limit speed by maximum speed
                    //if (degreesPerSecondSpeed > unsignedMaxRotSpd)
                    //    degreesPerSecondSpeed = unsignedMaxRotSpd;

                    ////Add the other acceleration half to the global speed for use in next frame
                    //rotSpeed = rotAccelHalf * spd.SetRotationAmount(degreesPerSecondSpeed);

                    spd = RotateAccelerated(rotPath, deltaTime);
                    break;

                case SpeedMode.SmoothDamp:
                    //spd = oRot.SmoothDamp(tRot,
                    //    ref rotSpeed, speedBehaviour.smoothTime, unsignedMaxRotSpd, deltaTime, true).Subtract(oRot);
                    //degreesPerSecondSpeed = spd.Angle() * inverseDeltaTime;
                    spd = RotateSmoothDamp(rotPath, deltaTime);
                    break;

                case SpeedMode.LerpSmooth:
                    //spd = oRot.SLerpSmooth(tRot, speedBehaviour.decay, deltaTime).Subtract(oRot);
                    //degreesPerSecondSpeed = spd.Angle() * inverseDeltaTime;
                    //if (degreesPerSecondSpeed > unsignedMaxRotSpd)
                    //    degreesPerSecondSpeed = unsignedMaxRotSpd;
                    spd = RotateLerpSmooth(rotPath, deltaTime);
                    break;

                case SpeedMode.Teleport:
                    spd = RotateTeleport(rotPath, deltaTime);
                    break;

                default:
                    spd = RotateLinear(rotPath, deltaTime);
                    break;
            }

        ExecuteRotation(spd, deltaTime);
    }

    RotationPath GetRotation()
    {
        Quaternion oRot = origin.Rotation(local);
        Quaternion tRot = target.rotation;
        if (local && (origin.parent != null))
            tRot = Quaternion.Inverse(origin.parent.rotation) * tRot;
        if (speedBehaviour.MoveAway())
            tRot = Quaternion.Inverse(tRot);

        RotationPath rotPath = new RotationPath(oRot, tRot);

        if (projectOnPlane)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            rotPath.ProjectOnPlane(localPlaneNormal);
        }

        return rotPath;
    }

    bool ShouldIRotate(RotationPath rotation)
    {
        float distanceToTarget = rotation.magnitude;
        if ((distanceToTarget < margin) && (targetMode == TargetMode.StopAtMargin))
            distanceToTarget = 0f;
        return distanceToTarget > 0f;
    }

    Quaternion RotateAccelerated(RotationPath rotPath, float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        float tangle;
        Vector3 taxis;

        Quaternion accelHalf0 = Quaternion.identity;
        if (speedBehaviour.doAccelerate)
        {
            float accel = speedBehaviour.acceleration * deltaTime * 0.5f;
            //if (keepInStraightPath)
            //{
            //TO DO: This is quite complicated because I need to somehow figure out the magnitude
            //of the speed projected in the previous path to avoid the object slowing down on corners.
            //I also need to account for externel forces, which shouldn't deviate the object but should
            //accelerete it or decelerate it by the projected variation vector.
            //}
            //else
            {
                accelHalf0 = rotPath.Direction(accel);
                accelHalf = accelHalf0;
            }
        }
        else
        {
            //if (keepInStraightPath)
            {
                //TO DO: This is quite complicated because I need to somehow figure out the magnitude
                //of the speed projected in the previous path to avoid the object slowing down on corners.
                //I also need to account for externel forces, which shouldn't deviate the object but should
                //accelerete it or decelerate it by the projected variation vector.
            }
            //else
            {
                //Calculate both halfs. They might be different.
                float accelMag = speedBehaviour.friction * deltaTime;
                float accelMagHalf1 = accelMag * 0.5f;
                float accelMagHalf2 = accelMagHalf1;

                speed.ToAngleAxis(out tangle, out taxis);

                if (accelMag > tangle)
                {
                    float rest = tangle - accelMagHalf1;
                    if (rest < 0f)
                    {
                        accelMagHalf1 = tangle;
                        accelMagHalf2 = 0f;
                    }
                    else accelMagHalf2 = rest;
                }

                accelHalf0 = Quaternion.AngleAxis(-accelMagHalf1, taxis);
                accelHalf = Quaternion.AngleAxis(-accelMagHalf2, taxis);
            }
        }

        speed = speed.Add(accelHalf0);
        speed.ToAngleAxis(out tangle, out taxis);
        Quaternion spd = Quaternion.AngleAxis(tangle * deltaTime, taxis);

        //Limit speed by maximum speed
        spd.ToAngleAxis(out tangle, out taxis);
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (tangle > maxDTSpeed)
            spd = Quaternion.AngleAxis(maxDTSpeed, taxis);

        //Add the other acceleration half to the global speed for use in next frame
        speed = speed.Add(accelHalf);

        return spd;
    }

    Quaternion RotateSmoothDamp(RotationPath rotPath, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return rotPath.SmoothDamp(ref speed, speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime).Subtract(rotPath.origin);
    }

    Quaternion RotateLerpSmooth(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        return rotPath.Displacement(Mathf.Min(0f.LerpSmooth(rotPath.magnitude, speedBehaviour.decay, deltaTime), maxDTSpeed));
    }

    Quaternion RotateTeleport(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed;
        float rotateAmount = rotPath.magnitude;
        if (targetMode == TargetMode.StopAtMargin)
            rotateAmount -= margin;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    Quaternion RotateLinear(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        float rotateAmount = rotPath.magnitude;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    void ExecuteRotation(Quaternion angleAxisPerThisFrame, float deltaTime)
    {
        angleAxisPerThisFrame.ToAngleAxis(out float angle, out Vector3 axis);

        float degreesPerSecondSpeed = angle * InverseDeltaTime(deltaTime);

        if (prevSpd <= 0f)
        {
            if (degreesPerSecondSpeed > 0f) startedRotating?.Invoke();
        }
        else if (degreesPerSecondSpeed <= 0f) stoppedRotating?.Invoke();
        prevSpd = degreesPerSecondSpeed;

        if ((degreesPerSecondSpeed > 0f) || sendWhenZeroToo)
        {
            //Calculate and send percent of angular speed from zero to max speed
            //for things like animation syncing
            float percent = (speedBehaviour.speedMode == SpeedMode.Teleport) ?
                angle / speedBehaviour.unsignedMaxSpeed : degreesPerSecondSpeed / speedBehaviour.unsignedMaxSpeed;
            angleSpeedPercent?.Invoke(percent);

            //Calculate and send euler angles with direction and amount of rotation
            Quaternion result = Quaternion.AngleAxis(degreesPerSecondSpeed, axis);
            rotation?.Invoke(sendFrameMovement ? angleAxisPerThisFrame : result);
            if (local && (origin.parent != null)) result = origin.parent.rotation * result;
            if (rotateTransform)
                origin.Rotate(angleAxisPerThisFrame.eulerAngles, local ? Space.Self : Space.World);
        }
    }

    float InverseDeltaTime(float deltaTime)
    {
        return (deltaTime != 0f) ? 1f / deltaTime : Mathf.Infinity;
    }

    /// <summary>
    /// Instantly moves the origin transform to the target position
    /// </summary>
    public void Teleport()
    {
        Quaternion dif = target.rotation.Subtract(origin.rotation);
        origin.Rotate(dif.eulerAngles, Space.World);
        speed = Quaternion.identity;
    }
}
