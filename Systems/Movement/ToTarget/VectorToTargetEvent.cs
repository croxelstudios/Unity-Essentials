using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using static SpeedBehaviour;

public class VectorToTargetEvent : BToTarget<Vector3, MovementPath>, INavMeshAgentTypeContainer
{
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
    //[SerializeField]
    //[Tooltip("If this is set to true this code will keep the object in a straight path to the target instead of overshooting")]
    //[ShowIf("@useNavMesh && (speedBehaviour.speedMode == SpeedMode.Accelerated || speedMode == SpeedMode.SmoothDamp)")]
    //bool keepInStraightPath = false;
    [SerializeField]
    bool reorientTransform = false;
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

    float prevSpd;

    //Agent type
    public void OverrideNavMeshAgentType(int navMeshAgentType, out int prevAgentType)
    {
        prevAgentType = this.navMeshAgentType;
        this.navMeshAgentType = navMeshAgentType;
    }
    //

    protected override void OnEnable()
    {
        prevSpd = 0f;
        base.OnEnable();
    }

    protected override MovementPath GetPath()
    {
        Vector3 oPos = Current(late);
        Vector3 tPos = target.position;
        if (local && (origin.parent != null))
            tPos = origin.parent.InverseTransformPoint(tPos);
        if (speedBehaviour.MoveAway())
            tPos = oPos - (tPos - oPos);

        MovementPath path = new MovementPath(oPos, tPos, useNavMesh, (int)navMeshAreaMask, navMeshAgentType);

        if (projectOnPlane)
        {
            Vector3 localPlaneNormal = projectLocally ? transform.rotation * planeNormal : planeNormal;
            path.ProjectOnPlane(localPlaneNormal);
        }

        return path;
    }

    protected override Vector3 Accelerated(MovementPath path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        float inverseDeltaTime = deltaTime.Reciprocal();
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
                dynamicInfo.accelHalf = accelerationHalf0;
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
                if (accelMag > dynamicInfo.speed.magnitude)
                {
                    float rest = dynamicInfo.speed.magnitude - accelMagHalf1;
                    if (rest < 0f)
                    {
                        accelMagHalf1 = dynamicInfo.speed.magnitude;
                        accelMagHalf2 = 0f;
                    }
                    else accelMagHalf2 = rest;
                }

                accelerationHalf0 = -dynamicInfo.speed.normalized * accelMagHalf1;
                dynamicInfo.accelHalf = -dynamicInfo.speed.normalized * accelMagHalf2;
            }
        }

        dynamicInfo.speed += accelerationHalf0;
        Vector3 spd = dynamicInfo.speed * deltaTime;

        //Limit speed by maximum speed
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (spd.magnitude > maxDTSpeed)
            spd = spd.normalized * maxDTSpeed;

        //Limit speed by target point
        float moveAmount = path.magnitude;
        if ((spd.magnitude > moveAmount) && (targetMode != TargetMode.NeverStop))
            spd = spd.normalized * moveAmount;

        //Add the other acceleration half to the global speed for use in next frame
        dynamicInfo.speed += dynamicInfo.accelHalf;

        return spd;
    }

    protected override Vector3 SmoothDamp(MovementPath path, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return path.SmoothDamp(ref dynamicInfo.speed,
            speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime,
            useNavMesh ? MovementPath.SmoothMode.NavMesh : MovementPath.SmoothMode.AlongPath)
            - path.origin;
    }

    protected override Vector3 LerpSmooth(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        return path.Displacement(
            Mathf.Min(0f.LerpSmooth(path.magnitude, speedBehaviour.decay, deltaTime), maxDTSpeed));
    }

    protected override Vector3 Teleport(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed;
        float moveAmount = path.magnitude;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    protected override Vector3 Linear(MovementPath path, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        float moveAmount = path.magnitude;
        if (targetMode == TargetMode.StopAtMargin)
            moveAmount -= margin;
        if ((moveAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            moveAmount = maxDTSpeed;
        return path.Displacement(moveAmount);
    }

    protected override void Execute(Vector3 speedPerThisFrame, float deltaTime)
    {
        float unitsPerSecondSpeed = speedPerThisFrame.magnitude * deltaTime.Reciprocal();

        if (prevSpd <= Mathf.Epsilon)
        {
            if (unitsPerSecondSpeed > Mathf.Epsilon) startedMoving?.Invoke();
        }
        else if (unitsPerSecondSpeed <= Mathf.Epsilon) stoppedMoving?.Invoke();
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
            if (applyInTransform)
                origin.Translate(speedPerThisFrame, local ? Space.Self : Space.World);
            if (reorientTransform)
                origin.forward = result;
        }
    }

    /// <summary>
    /// Instantly applies transformations to the origin transform
    /// </summary>
    public override void Teleport()
    {
        Vector3 dif = target.position - origin.position;
        origin.Translate(dif, Space.World);
        ResetSpeed();
    }

    public override Vector3 Current()
    {
        return origin.Position(local);
    }

    protected override void SetLatePrev()
    {
        prev = transform.localPosition;
    }

    protected override Vector3 GetWorldLatePrev()
    {
        return (transform.parent == null) ? prev : transform.parent.InverseTransformPoint(prev);
    }

    public override void UpdateSpeed(ref Vector3 speed,
        Vector3 accelHalf, Vector3 prev, float deltaTime)
    {
        speed = ((Current(late) - prev) / deltaTime) + accelHalf;
    }
}
