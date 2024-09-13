using Mono.CSharp;
using UnityEngine;

public static class QuaternionExtension_AbsoluteAngle
{
    /// <summary>
    /// Returns the angle of a quaternion relative to identity.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static float Angle(this Quaternion quat)
    {
        Vector3 axis;
        float angle;
        quat.ToAngleAxis(out angle, out axis);
        return angle;
    }
}
