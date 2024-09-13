using UnityEngine;
using static UnityEngine.UI.Image;

public static class QuaternionExtension_SmoothDamp
{
	public static Quaternion SmoothDamp(this Quaternion rot, Quaternion target, ref Quaternion tmpSpeed, float smoothTime, float maxSpeed, float deltaTime, bool dontCorrectLongPaths = false)
    {
        // ChatGPT code:
        // Ensure that smoothTime is greater than a minimum value
        smoothTime = Mathf.Max(0.0001f, smoothTime);

        // Ensure the quaternions are in the same hemisphere to avoid taking the long path
        float dot = Quaternion.Dot(rot, target);
        if ((dot < 0f) && !dontCorrectLongPaths)
            target = new Quaternion(-target.x, -target.y, -target.z, -target.w);

        // Calculate the maximum angular velocity in radians per second
        float maxRadiansDelta = maxSpeed * Mathf.Deg2Rad * deltaTime;

        // Damping function
        float t = 1 - Mathf.Exp(-deltaTime / smoothTime);

        // Smoothly interpolate the rotation towards the target using Slerp with the new t
        Quaternion result = Quaternion.Slerp(rot, target, t);

        // Calculate the angular derivative (velocity)
        //TO DO: I suspect this is not correct because it isn't using the tmpSpeed variable
        Vector4 deriv4 = new Vector4(tmpSpeed.x, tmpSpeed.y, tmpSpeed.z, tmpSpeed.w);
        Vector4 current4 = new Vector4(rot.x, rot.y, rot.z, rot.w);
        Vector4 result4 = new Vector4(result.x, result.y, result.z, result.w);

        deriv4 = (result4 - current4) / deltaTime;
        tmpSpeed = new Quaternion(deriv4.x, deriv4.y, deriv4.z, deriv4.w);

        // Limit angular velocity if necessary
        if (Quaternion.Angle(rot, result) > maxRadiansDelta)
            result = Quaternion.RotateTowards(rot, result, maxRadiansDelta);

        return result;
    }
}
