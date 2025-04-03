using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

public class VectorToTargetEvent : MonoBehaviour
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

    [Header("Speed")]
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
    TargetMode targetMode = TargetMode.MoveToExactPoint;
    [SerializeField]
    [HideIf("@speedMode == SpeedMode.Teleport")]
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
    [Tooltip("NavMesh area to use")]
    int navMeshAreaMask = NavMesh.AllAreas; //TO DO: actually render popup like with layers and tags.
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
    DXVectorEvent rotation = null; //TO DO: DXRotation event that can send raw angle, axis, quaternion, or euler.
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
    enum TargetMode { MoveToExactPoint, NeverStop, StopAtMargin }

    float unsignedMaxSpd;
    int rotSign;
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
        if (useTagForOrigin) origin = TagSearch(targetTag);
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
        target = TagSearch(targetTag);
    }

    Transform TagSearch(string tag)
    {
        Transform target = null;
        if (tag != "")
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);
            if (objs.Length > 0)
            {
                if (objs[0] != gameObject) target = objs[0].transform;
                else if (objs.Length > 1) target = objs[1].transform;

                for (int i = 0; i < objs.Length; i++)
                    if ((target != objs[i]) && target.IsChildOf(objs[i].transform))
                        target = objs[i].transform;
            }
        }
        return target;
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

        Path path = GetPath();
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

    Path GetPath()
    {
        Vector3 oPos = origin.Position(local);
        Vector3 tPos = target.position;
        if (local && (origin.parent != null))
            tPos = origin.parent.InverseTransformPoint(tPos);
        if (moveAway)
            tPos = oPos - (tPos - oPos);

        Path path = new Path(oPos, tPos, useNavMesh, navMeshAreaMask);

        if (projection)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            path.ProjectOnPlane(localPlaneNormal);
        }

        return path;
    }

    bool ShouldIMove(Path path)
    {
        float distanceToTarget = path.magnitude;
        if ((distanceToTarget < margin) && (targetMode == TargetMode.StopAtMargin))
            distanceToTarget = 0f;
        return distanceToTarget > 0f;
    }

    Vector3 MoveAccelerated(Path path, float deltaTime)
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

        //Add the other acceleration half to the global speed for use in next frame
        speed += accelerationHalf;

        return spd;
    }

    Vector3 MoveSmoothDamp(Path path, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return path.SmoothDamp(ref speed, smoothTime, maxSpeed, deltaTime) - path.origin;
    }

    Vector3 MoveLerpSmooth(Path path, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxSpd * deltaTime;
        return path.Displacement(Mathf.Min(0f.LerpSmooth(path.magnitude, decay, deltaTime), maxDTSpeed));
    }

    Vector3 MoveTeleport(Path path, float deltaTime)
    {
        float maxDTSpeed = unsignedMaxSpd;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    Vector3 MoveLinear(Path path, float deltaTime)
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
            vector?.Invoke(result);
            if (local && (origin.parent != null)) result = origin.parent.TransformVector(result);
            if (moveTransform) //TO DO: Implement this bool for each feature
                origin.Translate(speedPerThisFrame, Space.World);
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

        RotationPath rotPath = new RotationPath(oRot, tRot);

        if (projection)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            rotPath.ProjectOnPlane(localPlaneNormal); //WARNING: Not implemented.
        }

        return rotPath;
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
        float angle;
        Vector3 axis;
        angleAxisPerThisFrame.ToAngleAxis(out angle, out axis);

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
                origin.Rotate(result.Scale(deltaTime).eulerAngles, Space.World);
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

    //TO DO: Move both paths structures to a static class
    struct RotationPath
    {
        public Quaternion origin;
        public Quaternion target;
        public Quaternion dif;
        public Quaternion[] path;
        public RotationMode mode;
        public float magnitude;
        public float difAngle;
        public Vector3 difAxis;

        public RotationPath(Quaternion origin, Quaternion target, RotationMode mode = RotationMode.Shortest)
        {
            this.origin = origin;
            this.target = target;
            this.mode = mode;
            dif = target.Subtract(origin, mode);
            dif.ToAngleAxis(out difAngle, out difAxis);
            path = new Quaternion[] { origin, target };
            magnitude = dif.Angle();
        }

        public RotationPath(Quaternion[] rotations, RotationMode mode = RotationMode.Shortest)
        {
            origin = rotations[0];
            target = rotations[rotations.Length - 1];
            this.mode = mode;
            path = rotations;
            dif = target.Subtract(origin, mode);
            dif.ToAngleAxis(out difAngle, out difAxis);
            magnitude = 0f;
            CalculateMagnitude();
        }

        //TO DO
        public void ProjectOnPlane(Vector3 normal)
        {
        }

        public void CalculateMagnitude()
        {
            if (IsComplexPath())
            {
                Quaternion previousCorner = path[0];
                float lengthSoFar = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    Quaternion currentCorner = path[i];
                    lengthSoFar += currentCorner.AngleDistance(previousCorner);
                    previousCorner = currentCorner;
                    i++;
                }
                magnitude = lengthSoFar;
            }
            else magnitude = dif.Angle();
        }

        public Quaternion RotationAlong(float distance)
        {
            if (IsComplexPath())
            {
                float tangle;
                Vector3 taxis;

                Quaternion previousCorner = path[0];
                float wholeDist = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    Quaternion currentCorner = path[i];

                    currentCorner.Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

                    if ((wholeDist + tangle) > distance)
                        return previousCorner.Add(Quaternion.AngleAxis(distance - wholeDist, taxis));
                    wholeDist += tangle;
                    previousCorner = currentCorner;
                    i++;
                }

                path[path.Length - 2].Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

                return previousCorner.Add(Quaternion.AngleAxis(distance - wholeDist, taxis));
            }
            else return origin.Add(Quaternion.AngleAxis(distance, difAxis));
        }

        public Quaternion DisplacementAlong(float distance)
        {
            Quaternion worldPos = RotationAlong(distance);
            return worldPos.Subtract(origin);
        }

        public Quaternion Direction(float multiplier = 1f)
        {
            if (IsComplexPath())
                return Quaternion.AngleAxis(multiplier, path[1].Subtract(path[0]).Axis());
            else return Quaternion.AngleAxis(multiplier, difAxis);
        }

        public Quaternion SmoothDamp(ref Quaternion currentVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
        {
            //Limit smoothTime to avoid division by 0f;
            smoothTime = Mathf.Max(0.0001F, smoothTime);

            //Calculate omega and exponent
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
            Quaternion target = origin.Add(Displacement(change, alongPath));
            RotationPath subPath = alongPath ? new RotationPath(target, origin, mode)/*SubPath(change, 0, true)*/ :
                new RotationPath(target, origin, mode);

            float tangle;
            Vector3 taxis;
            currentVelocity.ToAngleAxis(out tangle, out taxis);

            Quaternion temp = Quaternion.AngleAxis(tangle * deltaTime * exp, taxis).Add(
                subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp, alongPath));

            Quaternion output = target.Add(temp);

            //Avoid overshoot
            float disp;
            ClosestInPath(output, out disp);
            if (disp > magnitude) output = this.target;

            output.Subtract(origin).ToAngleAxis(out tangle, out taxis);
            currentVelocity = Quaternion.AngleAxis(tangle / deltaTime, taxis);

            return output;
        }

        public Quaternion Displacement(float distance, bool alongPath = true)
        {
            return alongPath ? DisplacementAlong(distance) : Direction(distance);
        }

        public Quaternion Displacement(float distance, float offset, bool alongPath = true)
        {
            return SubPath(magnitude - offset, offset).Displacement(distance, alongPath);
        }

        //TO DO
        public Quaternion ClosestInPath(Quaternion point)
        {
            float d;
            return ClosestInPath(point, out d);
        }

        //TO DO
        public Quaternion ClosestInPath(Quaternion point, out float disp)
        {
            disp = 0f;
            return origin;
        }

        RotationPath SubPath(float distance, float offset, bool invert = false)
        {
            if (IsComplexPath())
            {
                float tangle;
                Vector3 taxis;

                List<Quaternion> corners = new List<Quaternion>();

                Quaternion previousCorner = path[0];
                Quaternion currentCorner = previousCorner;
                float wholeDist = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    currentCorner = path[i];
                    float dist = currentCorner.AngleDistance(previousCorner);
                    wholeDist += dist;
                    if (wholeDist > offset)
                    {
                        wholeDist = wholeDist - offset;
                        break;
                    }
                    previousCorner = currentCorner;
                    i++;
                }

                previousCorner.Subtract(currentCorner).ToAngleAxis(out tangle, out taxis);

                corners.Add(currentCorner.Add(Quaternion.AngleAxis(wholeDist, taxis)));

                if (wholeDist > distance)
                    corners.Add(previousCorner.Add(Quaternion.AngleAxis(wholeDist + distance, taxis)));
                else
                {
                    corners.Add(currentCorner);
                    distance -= wholeDist;
                    previousCorner = currentCorner;
                    wholeDist = 0f;
                    i++;
                    while (i < path.Length)
                    {
                        currentCorner = path[i];
                        float dist = currentCorner.AngleDistance(previousCorner);
                        wholeDist += dist;
                        if (wholeDist > distance)
                        {
                            wholeDist = wholeDist - offset;
                            break;
                        }
                        corners.Add(currentCorner);
                        previousCorner = currentCorner;
                        i++;
                    }

                    currentCorner.Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

                    corners.Add(previousCorner.Add(Quaternion.AngleAxis(wholeDist, taxis)));
                }

                if (invert)
                {
                    List<Quaternion> inverted = new List<Quaternion>();
                    for (int j = corners.Count - 1; j >= 0; j--)
                        inverted.Add(corners[j]);
                    corners.Clear();
                    corners = inverted;
                }

                return new RotationPath(corners.ToArray(), mode);
            }
            else
            {
                return invert ?
                    new RotationPath(origin.Add(Quaternion.AngleAxis(offset + distance, difAxis)),
                        origin.Add(Quaternion.AngleAxis(offset, difAxis)), mode) :
                    new RotationPath(origin.Add(Quaternion.AngleAxis(offset, difAxis)),
                        origin.Add(Quaternion.AngleAxis(offset + distance, difAxis)), mode);
            }
        }

        bool IsComplexPath()
        {
            return (path != null) && (path.Length > 2);
        }
    }

    struct Path
    {
        public Vector3 origin;
        public Vector3 target;
        public Vector3 dif;
        public Vector3[] path;
        public float magnitude;

        public Path(Vector3 origin, Vector3 target, bool useNavMesh = false, int navMeshAreaMask = -1)
        {
            this.origin = origin;
            this.target = target;
            dif = target - origin;
            path = new Vector3[] { origin, target };
            magnitude = 0f;

            if (useNavMesh)
            {
                NavMeshPath nav = new NavMeshPath();
                NavMesh.CalculatePath(origin, target, navMeshAreaMask, nav);
                path = nav.corners;
            }

            CalculateMagnitude();
        }

        public Path(Vector3[] points)
        {
            origin = points[0];
            target = points[points.Length - 1];
            path = points;
            dif = target - origin;
            magnitude = 0f;
            CalculateMagnitude();
        }

        public void ProjectOnPlane(Vector3 normal)
        {
            origin = Vector3.ProjectOnPlane(origin, normal);
            target = Vector3.ProjectOnPlane(target, normal);
            dif = Vector3.ProjectOnPlane(dif, normal);
            if (IsComplexPath())
                for (int i = 0; i < path.Length; i++)
                    path[i] = Vector3.ProjectOnPlane(path[i], normal);
        }

        public void CalculateMagnitude()
        {
            if (IsComplexPath())
            {
                Vector3 previousCorner = path[0];
                float lengthSoFar = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    Vector3 currentCorner = path[i];
                    lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                    previousCorner = currentCorner;
                    i++;
                }
                magnitude = lengthSoFar;
            }
            else magnitude = dif.magnitude;
        }

        public Vector3 PositionAlong(float distance)
        {
            if (IsComplexPath())
            {
                Vector3 previousCorner = path[0];
                float wholeDist = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    Vector3 currentCorner = path[i];
                    float dist = Vector3.Distance(previousCorner, currentCorner);
                    if ((wholeDist + dist) > distance)
                        return previousCorner + ((currentCorner - previousCorner).normalized * (distance - wholeDist));
                    wholeDist += dist;
                    previousCorner = currentCorner;
                    i++;
                }
                return previousCorner + (previousCorner - path[path.Length - 2]).normalized * (distance - wholeDist);
            }
            else return origin + (dif.normalized * distance);
        }

        public Vector3 DisplacementAlong(float distance)
        {
            Vector3 worldPos = PositionAlong(distance);
            return worldPos - origin;
        }

        public Vector3 Direction(float multiplier = 1f)
        {
            if (IsComplexPath())
                return (path[1] - path[0]).normalized * multiplier;
            else return dif.normalized * multiplier;
        }

        public Vector3 SmoothDamp(ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
        {
            //Limit smoothTime to avoid division by 0f;
            smoothTime = Mathf.Max(0.0001F, smoothTime);

            //Calculate omega and exponent
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
            Vector3 target = origin + Displacement(change, alongPath);
            //TO DO: This doesn't seem to work because it overshoots the path and so the current method calculates a different one?
            Path subPath = alongPath ? new Path(target, origin, true) /*SubPath(change, 0, true)*/ : new Path(target, origin, false);

            Vector3 temp = (currentVelocity * deltaTime * exp) +
                subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp, alongPath);

            Vector3 output = target + temp;

            //Avoid overshoot
            float disp;
            ClosestInPath(output, out disp);
            if (disp > magnitude) output = this.target;

            currentVelocity = (output - origin) / deltaTime;

            return output;
        }

        public Vector3 Displacement(float distance, bool alongPath = true)
        {
            return alongPath ? DisplacementAlong(distance) : Direction(distance);
        }

        public Vector3 Displacement(float distance, float offset, bool alongPath = true)
        {
            return SubPath(magnitude - offset, offset).Displacement(distance, alongPath);
        }

        public Vector3 ClosestInPath(Vector3 point)
        {
            float d;
            return ClosestInPath(point, out d);
        }

        public Vector3 ClosestInPath(Vector3 point, out float disp)
        {
            if (IsComplexPath())
            {
                float dist = Mathf.Infinity;
                float dispAdd = 0f;
                int n = 0;
                Vector3 clos = path[0];
                for (int i = 0; i < path.Length - 1; i++)
                {
                    float dispAdd_c;
                    Vector3 c = ClosestPointOnSegment(point, path[i], path[i + 1], out dispAdd_c);
                    float d = Vector3.Distance(point, c);
                    if (d < dist)
                    {
                        dispAdd = dispAdd_c;
                        dist = d;
                        clos = c;
                        n = i;
                    }
                }

                disp = dispAdd * Vector3.Distance(path[n], path[n + 1]);
                for (int i = 0; i < n; i++)
                    disp += Vector3.Distance(path[i], path[i + 1]);

                return clos;
            }
            else return ClosestPointOnSegment(point, origin, target, out disp);
        }

        Vector3 ClosestPointOnLine(Vector3 p, Vector3 l0, Vector3 l1, out float dist)
        {
            Vector3 normal = (l1 - l0).normalized;

            dist = Vector3.Dot(p - l0, normal) / Vector3.Dot(normal, normal);
            return l0 + (normal * dist);
        }

        Vector3 ClosestPointOnSegment(Vector3 p, Vector3 l0, Vector3 l1, out float d)
        {
            Vector3 dir = l1 - l0;
            Vector3 clos = ClosestPointOnLine(p, l0, l1, out d);
            if (d < 0f)
            {
                d = 0f;
                clos = l0;
            }
            else if ((d * d) > dir.sqrMagnitude)
            {
                d = 1f;
                clos = l1;
            }
            return clos;
        }

        Path SubPath(float distance, float offset, bool invert = false)
        {
            if (IsComplexPath())
            {
                List<Vector3> corners = new List<Vector3>();

                Vector3 previousCorner = path[0];
                Vector3 currentCorner = previousCorner;
                float wholeDist = 0f;
                int i = 1;
                while (i < path.Length)
                {
                    currentCorner = path[i];
                    float dist = Vector3.Distance(previousCorner, currentCorner);
                    wholeDist += dist;
                    if (wholeDist > offset)
                    {
                        wholeDist = wholeDist - offset;
                        break;
                    }
                    previousCorner = currentCorner;
                    i++;
                }
                corners.Add(currentCorner + ((previousCorner - currentCorner).normalized * wholeDist));

                if (wholeDist > distance)
                    corners.Add(previousCorner + ((currentCorner - previousCorner).normalized * (wholeDist + distance)));
                else
                {
                    corners.Add(currentCorner);
                    distance -= wholeDist;
                    previousCorner = currentCorner;
                    wholeDist = 0f;
                    i++;
                    while (i < path.Length)
                    {
                        currentCorner = path[i];
                        float dist = Vector3.Distance(previousCorner, currentCorner);
                        wholeDist += dist;
                        if (wholeDist > distance)
                        {
                            wholeDist = wholeDist - offset;
                            break;
                        }
                        corners.Add(currentCorner);
                        previousCorner = currentCorner;
                        i++;
                    }
                    corners.Add(previousCorner + ((currentCorner - previousCorner).normalized * wholeDist));
                }

                if (invert)
                {
                    List<Vector3> inverted = new List<Vector3>();
                    for (int j = corners.Count - 1; j >= 0; j--)
                        inverted.Add(corners[j]);
                    corners.Clear();
                    corners = inverted;
                }

                return new Path(corners.ToArray());
            }
            else
            {
                return invert ?
                    new Path(origin + (dif.normalized * (offset + distance)), origin + (dif.normalized * offset)) :
                    new Path(origin + (dif.normalized * offset), origin + (dif.normalized * (offset + distance)));
            }
        }

        public void Draw()
        {
            for (int i = 0; i < path.Length - 1; i++)
                Debug.DrawLine(path[i], path[i + 1], Color.orange);
        }

        bool IsComplexPath()
        {
            return (path != null) && (path.Length > 2);
        }
    }
}
