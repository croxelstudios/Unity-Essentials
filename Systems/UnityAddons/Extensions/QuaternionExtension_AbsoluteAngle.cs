using UnityEngine;

public static class QuaternionExtension_AbsoluteAngle
{
    /// <summary>
    /// Returns the angle of a quaternion relative to identity.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static float AbsoluteAngle(this Quaternion quat)
    {
        return Quaternion.Angle(Quaternion.identity, quat);
    }
}
