using Mono.CSharp;
using UnityEngine;

public static class QuaternionExtension_AngleAndAxis
{
    /// <summary>
    /// Returns the angle of a quaternion relative to identity.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static float Angle(this Quaternion quat)
    {
        quat.ToAngleAxis(out float angle, out Vector3 axis);
        return angle;
    }

    /// <summary>
    /// Returns the axis of a quaternion rotation.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static Vector3 Axis(this Quaternion quat)
    {
        quat.ToAngleAxis(out float angle, out Vector3 axis);
        return axis;
    }
}
