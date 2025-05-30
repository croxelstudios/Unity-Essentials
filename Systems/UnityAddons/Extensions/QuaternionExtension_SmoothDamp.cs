using UnityEngine;

public static class QuaternionExtension_SmoothDamp
{
	public static Quaternion SmoothDamp(this Quaternion rot, Quaternion target,
        ref Quaternion tmpSpeed, float smoothTime, float maxSpeed, float deltaTime,
        bool dontCorrectLongPaths = false)
    {
        Quaternion dif = target.Subtract(rot);
        dif.ToAngleAxis(out float difAngle, out Vector3 difAxis);

        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001F, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, difAngle);
        Quaternion targ = rot.Add(Quaternion.AngleAxis(change, difAxis));
        Quaternion subRot = rot.Subtract(targ);
        subRot.ToAngleAxis(out float subRotAngle, out Vector3 subRotAxis);

        tmpSpeed.ToAngleAxis(out float tangle, out Vector3 taxis);

        Quaternion temp = Quaternion.AngleAxis(tangle * deltaTime * exp, taxis).Add(
            Quaternion.AngleAxis((subRotAngle + (omega * subRotAngle) * deltaTime) * exp, subRotAxis));

        Quaternion output = targ.Add(temp);

        //Avoid overshoot
        Quaternion outputDif = output.Subtract(rot);
        if (outputDif.Angle() > difAngle)
        {
            output = target;
            outputDif = target.Subtract(rot);
        }

        outputDif.ToAngleAxis(out tangle, out taxis);
        tmpSpeed = Quaternion.AngleAxis(tangle / deltaTime, taxis);

        return output;
    }

    public static Quaternion EulerSmoothDamp(this Quaternion rot, Quaternion target,
        ref Quaternion tmpSpeed, float smoothTime, float maxSpeed, float deltaTime)
    {
        Vector3 euler = rot.eulerAngles;
        Vector3 targetEuler = target.eulerAngles;

        euler.x = Mathf.SmoothDampAngle(euler.x, targetEuler.x, ref tmpSpeed.x,
            smoothTime, maxSpeed, deltaTime);
        euler.y = Mathf.SmoothDampAngle(euler.y, targetEuler.y, ref tmpSpeed.y,
            smoothTime, maxSpeed, deltaTime);
        euler.z = Mathf.SmoothDampAngle(euler.z, targetEuler.z, ref tmpSpeed.z,
            smoothTime, maxSpeed, deltaTime);

        return Quaternion.Euler(euler);
    }
}
