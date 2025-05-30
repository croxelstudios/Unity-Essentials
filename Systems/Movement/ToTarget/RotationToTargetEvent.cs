using UnityEngine;
using Sirenix.OdinInspector;
using static SpeedBehaviour;

public class RotationToTargetEvent : BToTarget<Quaternion, RotationPath>
{
    [SerializeField]
    [Tooltip("If set to 'Positive' or 'Negative' it will force the rotation to be in a specific direction even if it's not the fastest")]
    RotationMode rotationMode = RotationMode.Shortest;
    #region Events
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
    #endregion

    float prevSpd;

    protected override void OnEnable()
    {
        prevSpd = 0f;
        base.OnEnable();
    }

    protected override RotationPath GetPath()
    {
        Quaternion oRot = Current();
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

    protected override Quaternion Accelerated(RotationPath rotPath, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        float inverseDeltaTime = deltaTime.Reciprocal();

        float tangle;
        Vector3 taxis;

        Quaternion accelHalf0 = Quaternion.identity;
        if (doAccelerate)
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
                dynamicInfo.accelHalf = accelHalf0;
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

                dynamicInfo.speed.ToAngleAxis(out tangle, out taxis);

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
                dynamicInfo.accelHalf = Quaternion.AngleAxis(-accelMagHalf2, taxis);
            }
        }

        dynamicInfo.speed = dynamicInfo.speed.Add(accelHalf0);
        dynamicInfo.speed.ToAngleAxis(out tangle, out taxis);
        Quaternion spd = Quaternion.AngleAxis(tangle * deltaTime, taxis);

        //Limit speed by maximum speed
        spd.ToAngleAxis(out tangle, out taxis);
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (tangle > maxDTSpeed)
            spd = Quaternion.AngleAxis(maxDTSpeed, taxis);

        //Add the other acceleration half to the global speed for use in next frame
        dynamicInfo.speed = dynamicInfo.speed.Add(dynamicInfo.accelHalf);

        return spd;
    }

    protected override Quaternion SmoothDamp(RotationPath rotPath, ref DynamicInfo dynamicInfo, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        return rotPath.SmoothDamp(ref dynamicInfo.speed, speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime)
            .Subtract(rotPath.origin);
    }

    protected override Quaternion LerpSmooth(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        return rotPath.Displacement(
            Mathf.Min(0f.LerpSmooth(rotPath.magnitude, speedBehaviour.decay, deltaTime), maxDTSpeed));
    }

    protected override Quaternion Teleport(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed;
        float rotateAmount = rotPath.magnitude;
        if (targetMode == TargetMode.StopAtMargin)
            rotateAmount -= margin;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    protected override Quaternion Linear(RotationPath rotPath, float deltaTime)
    {
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        float rotateAmount = rotPath.magnitude;
        if ((rotateAmount > maxDTSpeed) || (targetMode == TargetMode.NeverStop))
            rotateAmount = maxDTSpeed;
        return rotPath.Displacement(rotateAmount);
    }

    protected override void Execute(Quaternion angleAxisPerThisFrame, float deltaTime)
    {
        angleAxisPerThisFrame.ToAngleAxis(out float angle, out Vector3 axis);

        float degreesPerSecondSpeed = angle * deltaTime.Reciprocal();

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
            if (applyInTransform)
                origin.Rotate(angleAxisPerThisFrame.eulerAngles, local ? Space.Self : Space.World);
        }
    }

    /// <summary>
    /// Instantly applies transformations to the origin transform
    /// </summary>
    public override void Teleport()
    {
        Quaternion dif = target.rotation.Subtract(origin.rotation);
        origin.Rotate(dif.eulerAngles, Space.World);
        ResetSpeed();
    }

    public override Quaternion Current()
    {
        return origin.Rotation(local);
    }

    public override void UpdateSpeed(ref Quaternion speed, Quaternion prev, Quaternion accelHalf, float deltaTime)
    {
        speed = accelHalf *
            (Current() * Quaternion.Inverse(prev)).Scale(1f / deltaTime);
    }
}
