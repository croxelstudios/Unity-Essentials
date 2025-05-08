using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class VectorToTargetEvent : MonoBehaviour, INavMeshAgentTypeContainer
{
    [SerializeField]
    [Tooltip("Wether this code should apply movement to the 'origin' or it should just send the movement events elsewhere")]
    bool moveTransform = false;

    [Header("Target")]
    #region Target
    [SerializeField]
    [TagSelector]
    [Tooltip("Tag used to find the transform in case it is not specified")]
    string targetTag = "Player";
    [SerializeField]
    [Tooltip("Target transform")]
    Transform target = null;
    public Transform Target { get { return target; } set { target = value; } }
    [SerializeField]
    [Tooltip("False: The target is found through the tag. True: The 'origin' is found through the tag")]
    bool useTagForOrigin = false;
    [SerializeField]
    [HideIf("useTagForOrigin")]
    [Tooltip("The transform that will move or the transform that the movement is calculated from. By default, this object's transform.")]
    Transform origin = null;
    public Transform Origin { get { return origin; } set { origin = value; } }
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
    #region Speed
    [SerializeField]
    SpeedMode speedMode = SpeedMode.Linear;
    [ShowIf("@speedMode == SpeedMode.SmoothDamp")]
    [SerializeField]
    float smoothTime = 0.1f;
    [ShowIf("@speedMode == SpeedMode.LerpSmooth")]
    [SerializeField]
    float decay = 5f;
    [ShowIf("@speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp")]
    [SerializeField]
    bool accountForCurrentSpeed = true;
    [SerializeField]
    bool local = false;
    [SerializeField]
    bool sendWhenZeroToo = false;
    [SerializeField]
    TargetMode targetMode = TargetMode.ToExactPoint;
    [SerializeField]
    [Tooltip("When is this code executed")]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    #endregion

    [Space]
    [SerializeField]
    [LabelText("Move")]
    [Tooltip("Activates or deactivates the movement to target functionality")]
    bool move = true;
    #region Move
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Use NavMesh system to calculate the direction")]
    bool useNavMesh = false;
    [SerializeField]
    [ShowIf("@move && useNavMesh")]
    [Tooltip("NavMesh agent type to consider")]
    [NavMeshAgentTypeSelector]
    int navMeshAgentType = 0;
    [SerializeField]
    [ShowIf("@move && useNavMesh")]
    [Tooltip("NavMesh area to use")]
    NavMeshAreas navMeshAreaMask = NavMeshAreas.Walkable;
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Move away from target instead of to target")]
    bool moveAway = false; //TO DO: Make it global. What is the rotation equivalent?
    //[SerializeField]
    //[Tooltip("If this is set to true this code will keep the object in a straight path to the target instead of overshooting")]
    //[ShowIf("@useNavMesh && (speedBehaviour.speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp)")]
    //bool keepInStraightPath = false;
    [SerializeField]
    [ShowIf("move")]
    [OnValueChanged("UpdateSpeedData")]
    [Tooltip("Maximum movement speed in degrees per second")]
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
    bool sendFrameMovement = false;
    #region Events
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Resulting movement vector in units per second")]
    DXVectorEvent vector = null;
    [SerializeField]
    [ShowIf("move")]
    [Tooltip("Resulting speed percentage between zero and max speed")]
    DXFloatEvent magnitudePercent = null;
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
    #endregion
    #endregion

    [Space]
    [SerializeField]
    [Tooltip("Activates or deactivates the rotation to target functionality." +
        "This doesn't LOOK AT the target, but copies the target's rotation")]
    bool rotate = false;
    #region Rotate
    [SerializeField]
    [ShowIf("rotate")]
    [OnValueChanged("UpdateRotSpeedData")]
    [Tooltip("Maximum rotation speed in degrees per second")]
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
    RotationMode rotationMode = RotationMode.Shortest;
    #region Events
    [SerializeField]
    [ShowIf("rotate")]
    [Tooltip("Resulting rotation euler angles in degrees per second")]
    DXVectorEvent rotation = null;
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
    #endregion
    #endregion

    enum SpeedMode { Linear, Accelerated, SmoothDamp, LerpSmooth, Teleport }
    enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    float unsignedMaxSpd;
    Vector3 speed;
    Vector3 prevPos;
    Vector3 prevTarg;
    float prevSpd;
    float prevRotSpd;

    float unsignedMaxRotSpd;
    Quaternion rotSpeed;
    Quaternion prevRot;

    Vector3 accelerationHalf;
    Quaternion rotAccelHalf;

    //Agent type
    public void OverrideNavMeshAgentType(int navMeshAgentType, out int prevAgentType)
    {
        prevAgentType = this.navMeshAgentType;
        this.navMeshAgentType = navMeshAgentType;
    }
    //

    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
        UpdateSpeedData();
    }

    void UpdateSpeedData()
    {
        unsignedMaxSpd = Mathf.Abs(maxSpeed);
    }

    void UpdateRotSpeedData()
    {
        unsignedMaxRotSpd = Mathf.Abs(maxRotationSpeed);
    }

    public void UpdatePrevPos()
    {
        //WARNING: Non dynamic. Must be updated when "local" is changed.
        prevPos = origin.Position(local);
        prevRot = origin.Rotation(local);
    }

    void OnValidate()
    {
        if (origin == null) origin = transform;
    }

    void Awake()
    {
        speed = Vector3.zero;
        rotSpeed = Quaternion.identity;
        if (useTagForOrigin) origin = FindWithTag.TrCheckEmpty(targetTag);
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
        target = FindWithTag.TrCheckEmpty(targetTag);
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

        if ((speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp)
            && (deltaTime != 0f) && accountForCurrentSpeed)
        {
            if (Vector3.Distance(prevTarg, origin.Position(local)) > Mathf.Epsilon)
                speed = ((origin.Position(local) - prevPos) / deltaTime) + accelerationHalf;
            rotSpeed = rotAccelHalf *
                (origin.Rotation(local) * Quaternion.Inverse(prevRot)).Scale(1f / deltaTime);
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
        if (target == null) ResetTarget();
        if ((target != null) && (deltaTime > 0f))
        {
            if (move) Move(deltaTime);

            if (rotate) Rotate(deltaTime);
        }
    }

    void Move(float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        MovementPath path = GetPath();
        //path.Draw();

        Vector3 spd = Vector3.zero;
        if (ShouldIMove(path))
            switch (speedMode)
            {
                case SpeedMode.Accelerated:
                    spd = MoveAccelerated(path, deltaTime);
                    break;
                case SpeedMode.SmoothDamp:
                    spd = MoveSmoothDamp(path, deltaTime);
                    break;
                case SpeedMode.LerpSmooth:
                    spd = MoveLerpSmooth(path, deltaTime);
                    break;
                case SpeedMode.Teleport:
                    spd = MoveTeleport(path, deltaTime);
                    break;
                default: //Linear
                    spd = MoveLinear(path, deltaTime);
                    break;
            }

        ExecuteMovement(spd, deltaTime);
    }

    MovementPath GetPath()
    {
        Vector3 oPos = origin.Position(local);
        Vector3 tPos = target.position;
        if (local && (origin.parent != null))
            tPos = origin.parent.InverseTransformPoint(tPos);
        if (MoveAway())
            tPos = oPos - (tPos - oPos);

        MovementPath path = new MovementPath(oPos, tPos, useNavMesh, (int)navMeshAreaMask, navMeshAgentType);

        if (projection)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            path.ProjectOnPlane(localPlaneNormal);
        }

        return path;
    }

    bool MoveAway()
    {
        return moveAway ^ (Mathf.Sign(maxSpeed) < 0f);
    }

    bool ShouldIMove(MovementPath path)
    {
        float distanceToTarget = path.magnitude;
        if ((distanceToTarget < margin) && (targetMode == TargetMode.StopAtMargin))
            distanceToTarget = 0f;
        return distanceToTarget > 0f;
    }

    Vector3 MoveAccelerated(MovementPath path, float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);
        Vector3 accelerationHalf0 = Vector3.zero;
        if (doAccelerate)
        {
            float accel = acceleration * deltaTime * 0.5f;
            //if (keepInStraightPath)
            {
                //TO DO: This is quite complicated because I need to somehow figure out the magnitude
                //of the speed projected in the previous path to avoid the object slowing down on corners.
                //I also need to account for externel forces, which shouldn't deviate the object but should
                //accelerete it or decelerate it by the projected variation vector.
            }
            //else
            {
                accelerationHalf0 = path.Direction(accel);
                accelerationHalf = accelerationHalf0;
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
                float accelMag = (acceleration + frictionBias) * deltaTime;
                float accelMagHalf1 = accelMag * 0.5f;
                float accelMagHalf2 = accelMagHalf1;
                if (accelMag > speed.magnitude)
                {
                    float rest = speed.magnitude - accelMagHalf1;
                    if (rest < 0f)
                    {
                        accelMagHalf1 = speed.magnitude;
                        accelMagHalf2 = 0f;
                    }
                    else accelMagHalf2 = rest;
                }

                accelerationHalf0 = -speed.normalized * accelMagHalf1;
                accelerationHalf = -speed.normalized * accelMagHalf2;
            }
        }

        speed += accelerationHalf0;
        Vector3 spd = speed * deltaTime;

        //Limit speed by maximum speed
        float maxDTSpeed = unsignedMaxSpd * deltaTime;
        if (spd.magnitude > maxDTSpeed)
            spd = spd.normalized * maxDTSpeed;

        //Limit speed by target point
        float moveAmount = path.magnitude;
        if ((spd.magnitude > moveAmount) || (targetMode == TargetMode.NeverStop))
            spd = spd.normalized * moveAmount;

        //Add the other acceleration half to the global speed for use in next frame
        speed += accelerationHalf;

        return spd;
    }

    Vector3 MoveSmoothDamp(MovementPath path, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return path.SmoothDamp(ref speed, smoothTime, maxSpeed, deltaTime) - path.origin;
    }

    Vector3 MoveLerpSmooth(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxSpd * deltaTime;
        return path.Displacement(Mathf.Min(0f.LerpSmooth(path.magnitude, decay, deltaTime), maxDTSpeed));
    }

    Vector3 MoveTeleport(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxSpd;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    Vector3 MoveLinear(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxSpd * deltaTime;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    void ExecuteMovement(Vector3 speedPerThisFrame, float deltaTime)
    {
        float unitsPerSecondSpeed = speedPerThisFrame.magnitude * InverseDeltaTime(deltaTime);

        if (prevSpd <= 0f)
        {
            if (unitsPerSecondSpeed > 0f) startedMoving?.Invoke();
        }
        else if (unitsPerSecondSpeed <= 0f) stoppedMoving?.Invoke();
        prevSpd = unitsPerSecondSpeed;

        if ((unitsPerSecondSpeed > 0f) || sendWhenZeroToo)
        {
            //Calculate and send percent of speed from zero to max speed for things like animation syncing
            float percent = (speedMode == SpeedMode.Teleport) ?
                speedPerThisFrame.magnitude / unsignedMaxSpd :
                unitsPerSecondSpeed / unsignedMaxSpd;
            magnitudePercent?.Invoke(percent);

            Vector3 direction = speedPerThisFrame.normalized;

            //Calculate and send vector with direction and amount of speed
            Vector3 result = direction * unitsPerSecondSpeed;
            vector?.Invoke(sendFrameMovement ? speedPerThisFrame : result);
            if (local && (origin.parent != null)) result = origin.parent.TransformVector(result);
            if (moveTransform) //TO DO: Implement this bool for each feature
                origin.Translate(speedPerThisFrame, local ? Space.Self : Space.World);
            if (reorientTransform) //TO DO: Properly implement LookAt feature
                origin.forward = result;
        }
    }

    void Rotate(float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        RotationPath rotPath = GetRotation();

        Quaternion spd = Quaternion.identity;
        if (ShouldIRotate(rotPath))
            switch (speedMode)
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
        //if (RotateAway())
            //TO DO;

        RotationPath rotPath = new RotationPath(oRot, tRot);

        if (projection)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            rotPath.ProjectOnPlane(localPlaneNormal); //WARNING: Not implemented.
        }

        return rotPath;
    }

    bool RotateAway()
    {
        return /*rotateAway ^*/ (Mathf.Sign(maxRotationSpeed) < 0f);
    }

    bool ShouldIRotate(RotationPath rotation)
    {
        float distanceToTarget = rotation.magnitude;
        if ((distanceToTarget < rotationMargin) && (targetMode == TargetMode.StopAtMargin))
            distanceToTarget = 0f;
        return distanceToTarget > 0f;
    }

    Quaternion RotateAccelerated(RotationPath rotPath, float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        float tangle;
        Vector3 taxis;

        Quaternion rotAccelHalf0 = Quaternion.identity;
        if (doAngularAccelerate)
        {
            float accel = angularAcceleration * deltaTime * 0.5f;
            //if (keepInStraightPath)
            {
                //TO DO: This is quite complicated because I need to somehow figure out the magnitude
                //of the speed projected in the previous path to avoid the object slowing down on corners.
                //I also need to account for externel forces, which shouldn't deviate the object but should
                //accelerete it or decelerate it by the projected variation vector.
            }
            //else
            {
                rotAccelHalf0 = rotPath.Direction(accel);
                rotAccelHalf = rotAccelHalf0;
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
                float accelMag = (angularAcceleration + angularFrictionBias) * deltaTime;
                float accelMagHalf1 = accelMag * 0.5f;
                float accelMagHalf2 = accelMagHalf1;

                rotSpeed.ToAngleAxis(out tangle, out taxis);

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

                rotAccelHalf0 = Quaternion.AngleAxis(-accelMagHalf1, taxis);
                rotAccelHalf = Quaternion.AngleAxis(-accelMagHalf2, taxis);
            }
        }

        rotSpeed = rotSpeed.Add(rotAccelHalf0);
        rotSpeed.ToAngleAxis(out tangle, out taxis);
        Quaternion spd = Quaternion.AngleAxis(tangle * deltaTime, taxis);

        //Limit speed by maximum speed
        spd.ToAngleAxis(out tangle, out taxis);
        float maxDTSpeed = unsignedMaxRotSpd * deltaTime;
        if (tangle > maxDTSpeed)
            spd = Quaternion.AngleAxis(maxDTSpeed, taxis);

        //Add the other acceleration half to the global speed for use in next frame
        rotSpeed = rotSpeed.Add(rotAccelHalf);

        return spd;
    }

    Quaternion RotateSmoothDamp(RotationPath rotPath, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return rotPath.SmoothDamp(ref rotSpeed, smoothTime, maxRotationSpeed, deltaTime).Subtract(rotPath.origin);
    }

    Quaternion RotateLerpSmooth(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxRotSpd * deltaTime;
        return rotPath.Displacement(Mathf.Min(0f.LerpSmooth(rotPath.magnitude, decay, deltaTime), maxDTSpeed));
    }

    Quaternion RotateTeleport(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxRotSpd;
        float rotateAmount = rotPath.magnitude;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    Quaternion RotateLinear(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxRotSpd * deltaTime;
        float rotateAmount = rotPath.magnitude;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    void ExecuteRotation(Quaternion angleAxisPerThisFrame, float deltaTime)
    {
        angleAxisPerThisFrame.ToAngleAxis(out float angle, out Vector3 axis);

        float degreesPerSecondSpeed = angle * InverseDeltaTime(deltaTime);

        if (prevRotSpd <= 0f)
        {
            if (degreesPerSecondSpeed > 0f) startedRotating?.Invoke();
        }
        else if (degreesPerSecondSpeed <= 0f) stoppedRotating?.Invoke();
        prevRotSpd = degreesPerSecondSpeed;

        if ((degreesPerSecondSpeed > 0f) || sendWhenZeroToo)
        {
            //Calculate and send percent of angular speed from zero to max speed
            //for things like animation syncing
            float percent = (speedMode == SpeedMode.Teleport) ?
                angle / unsignedMaxRotSpd : degreesPerSecondSpeed / unsignedMaxSpd;
            angleSpeedPercent?.Invoke(percent);

            //Calculate and send euler angles with direction and amount of rotation
            Quaternion result = Quaternion.AngleAxis(degreesPerSecondSpeed, axis);
            rotation?.Invoke(result.eulerAngles);
            if (local && (origin.parent != null)) result = origin.parent.rotation * result;
            if (moveTransform)
                origin.Rotate(result.Scale(deltaTime).eulerAngles, local ? Space.Self : Space.World);
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
        if (move)
        {
            Vector3 dif = target.position - origin.position;
            origin.Translate(dif, Space.World);
            speed = Vector3.zero;
        }
        if (rotate)
        {
            Quaternion dif = target.rotation.Subtract(origin.rotation);
            origin.Rotate(dif.eulerAngles, Space.World);
            rotSpeed = Quaternion.identity;
        }
    }
}
