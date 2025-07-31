using Sirenix.OdinInspector;
using UnityEngine;
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
    #endregion

    protected override void OnEnable()
    {
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

        RotationPath rotPath = new RotationPath(oRot, tRot, rotationMode);

        if (projectOnPlane)
        {
            Vector3 localPlaneNormal = projectLocally ? transform.rotation * planeNormal : planeNormal;
            rotPath.ProjectOnPlane(localPlaneNormal);
        }

        return rotPath;
    }

    protected override Quaternion Accelerated(RotationPath rotPath,
        ref DynamicInfo dynamicInfo, float deltaTime)
    {
        float inverseDeltaTime = deltaTime.Reciprocal();

        float tAngle;
        Vector3 tAxis;

        Vector3 accelHalf0 = Vector3.zero;
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
                accelHalf0 = rotPath.DirectionAxis() * accel;
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

                tAngle = dynamicInfo.speed.magnitude;
                tAxis = dynamicInfo.speed / tAngle;

                if (accelMag > tAngle)
                {
                    float rest = tAngle - accelMagHalf1;
                    if (rest < 0f)
                    {
                        accelMagHalf1 = tAngle;
                        accelMagHalf2 = 0f;
                    }
                    else accelMagHalf2 = rest;
                }

                accelHalf0 = -accelMagHalf1 * tAxis;
                dynamicInfo.accelHalf = -accelMagHalf2 * tAxis;
            }
        }

        dynamicInfo.speed = dynamicInfo.speed + accelHalf0;
        tAngle = dynamicInfo.speed.magnitude;
        tAxis = dynamicInfo.speed / tAngle;
        Quaternion spd = Quaternion.AngleAxis(tAngle * deltaTime, tAxis);

        //Limit speed by maximum speed
        spd.ToAngleAxis(out tAngle, out tAxis);
        float maxDTSpeed = speedBehaviour.unsignedMaxSpeed * deltaTime;
        if (tAngle > maxDTSpeed)
            spd = Quaternion.AngleAxis(maxDTSpeed, tAxis);

        //Limit speed by target point
        spd.ToAngleAxis(out tAngle, out tAxis);
        float rotAmount = rotPath.magnitude;
        if ((tAngle > rotAmount) && (targetMode != TargetMode.NeverStop))
            spd = Quaternion.AngleAxis(rotAmount, tAxis);

        //Add the other acceleration half to the global speed for use in next frame
        dynamicInfo.speed = dynamicInfo.speed + dynamicInfo.accelHalf;

        return spd;
    }

    protected override Quaternion SmoothDamp(RotationPath rotPath,
        ref DynamicInfo dynamicInfo, float deltaTime)
    {
        //TO DO: Doesn't work with keepinpath feature
        //TO DO: Only works with Shortest RotationMode
        return rotPath.SmoothDamp(ref dynamicInfo.speed,
            speedBehaviour.smoothTime, speedBehaviour.maxSpeed, deltaTime).Subtract(rotPath.origin);
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

        CheckStartStop(degreesPerSecondSpeed);

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

    public override void UpdateSpeed(ref Vector3 speed,
        Vector3 accelHalf, Quaternion prev, float deltaTime)
    {
        //WARNING: This assumes shortest rotation, but it is technically possible for the object
        //to be rotated externally by a speed greater than 180 degrees per frame,
        //and this will fail in that case. Can't really see a way to fix this.
        Current().Subtract(prev).ToAngleAxis(RotationMode.Shortest, out float angle, out Vector3 axis);
        speed = accelHalf + (axis * (angle / deltaTime));
    }
}
