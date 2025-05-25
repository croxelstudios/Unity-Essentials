using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using static SpeedBehaviour;

public class VectorToTargetEvent : MonoBehaviour, INavMeshAgentTypeContainer
{
    [SerializeField]
    [Tooltip("Wether this code should apply movement to the 'origin' or it should just send the movement events elsewhere")]
    bool moveTransform = false;

    [Header("Target")]
    #region Target
    [SerializeField]
    [HideLabel]
    [InlineProperty]
    OriginTarget originTarget = new OriginTarget("Player");
    public Transform target { get { return originTarget.target; } set { originTarget.SetTarget(value); } }
    public Transform origin { get { return originTarget.origin; } set { originTarget.SetOrigin(value); } }
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
    SpeedBehaviour speedBehaviour = new SpeedBehaviour(SpeedMode.Linear);
    public bool doAccelerate
    {
        get { return speedBehaviour.doAccelerate; }
        set { speedBehaviour.doAccelerate = value; }
    }

    [SerializeField]
    [Tooltip("Use NavMesh system to calculate the direction")]
    bool useNavMesh = false;
    [SerializeField]
    [ShowIf("@useNavMesh")]
    [Tooltip("NavMesh agent type to consider")]
    [NavMeshAgentTypeSelector]
    int navMeshAgentType = 0;
    [SerializeField]
    [ShowIf("@useNavMesh")]
    [Tooltip("NavMesh area to use")]
    NavMeshAreas navMeshAreaMask = NavMeshAreas.Walkable;

    [SerializeField]
    bool local = false;
    [SerializeField]
    bool sendWhenZeroToo = false;
    [SerializeField]
    TargetMode targetMode = TargetMode.ToExactPoint;
    [SerializeField]
    [Tooltip("When is this code executed")]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    //[SerializeField]
    //[Tooltip("If this is set to true this code will keep the object in a straight path to the target instead of overshooting")]
    //[ShowIf("@useNavMesh && (speedBehaviour.speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp)")]
    //bool keepInStraightPath = false;
    [SerializeField]
    [Min(0.0000001f)]
    [ShowIf("@targetMode == TargetMode.StopAtMargin")]
    [Tooltip("Minimum distance to the target before the resulting vector becomes zero.")]
    float margin = 0.01f;
    [SerializeField]
    bool reorientTransform = false;
    [SerializeField]
    bool sendFrameMovement = false;
    #region Events
    [SerializeField]
    [Tooltip("Resulting movement vector in units per second")]
    DXVectorEvent vector = null;
    [SerializeField]
    [Tooltip("Resulting speed percentage between zero and max speed")]
    DXFloatEvent magnitudePercent = null;
    [SerializeField]
    [FoldoutGroup("START and STOP movement")]
    [Tooltip("Resulting vector was zero and is not zero now")]
    DXEvent startedMoving = null;
    [SerializeField]
    [FoldoutGroup("START and STOP movement")]
    [Tooltip("Resulting vector was not zero and is zero now")]
    DXEvent stoppedMoving = null;
    #endregion

    enum TargetMode { ToExactPoint, NeverStop, StopAtMargin }

    Vector3 speed;
    Vector3 prevPos;
    Vector3 prevTarg;
    float prevSpd;

    Vector3 accelHalf;

    //Agent type
    public void OverrideNavMeshAgentType(int navMeshAgentType, out int prevAgentType)
    {
        prevAgentType = this.navMeshAgentType;
        this.navMeshAgentType = navMeshAgentType;
    }
    //

    public void SetMaxSpeed(float speed)
    {
        speedBehaviour.SetMaxSpeed(speed);
    }

    public void UpdatePrevPos()
    {
        //WARNING: Non dynamic. Must be updated when "local" is changed.
        prevPos = origin.Position(local);
    }

    void Reset()
    {
        originTarget.SetOrigin(transform);
    }

    void Awake()
    {
        speed = Vector3.zero;
        UpdatePrevPos();
    }

    void OnEnable()
    {
        speed = Vector3.zero;
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
            Move(deltaTime);
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
            if (Vector3.Distance(prevTarg, origin.Position(local)) > Mathf.Epsilon)
                speed = ((origin.Position(local) - prevPos) / deltaTime) + accelHalf;
        }
        accelHalf = Vector3.zero;

        UpdatePrevPos();
    }

    void Move(float deltaTime)
    {
        float inverseDeltaTime = InverseDeltaTime(deltaTime);

        MovementPath path = GetPath();
        //path.Draw();

        Vector3 spd = Vector3.zero;
        if (ShouldIMove(path))
            switch (speedBehaviour.speedMode)
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
        if (speedBehaviour.MoveAway())
            tPos = oPos - (tPos - oPos);

        MovementPath path = new MovementPath(oPos, tPos, useNavMesh, (int)navMeshAreaMask, navMeshAgentType);

        if (projection)
        {
            Vector3 localPlaneNormal = projectionLocal ? transform.rotation * planeNormal : planeNormal;
            path.ProjectOnPlane(localPlaneNormal);
        }

        return path;
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
            float accel = speedBehaviour.acceleration * deltaTime * 0.5f;
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
                accelHalf = accelerationHalf0;
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
                accelHalf = -speed.normalized * accelMagHalf2;
            }
        }

        speed += accelerationHalf0;
        Vector3 spd = speed * deltaTime;

        //Limit speed by maximum speed
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (spd.magnitude > maxDTSpeed)
            spd = spd.normalized * maxDTSpeed;

        //Limit speed by target point
        float moveAmount = path.magnitude;
        if ((spd.magnitude > moveAmount) || (targetMode == TargetMode.NeverStop))
            spd = spd.normalized * moveAmount;

        //Add the other acceleration half to the global speed for use in next frame
        speed += accelHalf;

        return spd;
    }

    Vector3 MoveSmoothDamp(MovementPath path, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return path.SmoothDamp(ref speed, speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime)
            - path.origin;
    }

    Vector3 MoveLerpSmooth(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        return path.Displacement(
            Mathf.Min(0f.LerpSmooth(path.magnitude, speedBehaviour.decay, deltaTime), maxDTSpeed));
    }

    Vector3 MoveTeleport(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    Vector3 MoveLinear(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        float moveAmount = path.magnitude;
        if (targetMode == TargetMode.StopAtMargin)
            moveAmount -= margin;
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
            float percent = (speedBehaviour.speedMode == SpeedMode.Teleport) ?
                speedPerThisFrame.magnitude / speedBehaviour.unsignedMaxSpeed :
                unitsPerSecondSpeed / speedBehaviour.unsignedMaxSpeed;
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

    float InverseDeltaTime(float deltaTime)
    {
        return (deltaTime != 0f) ? 1f / deltaTime : Mathf.Infinity;
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
