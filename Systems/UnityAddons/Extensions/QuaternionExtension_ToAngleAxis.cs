using UnityEngine;

public static class QuaternionExtension_ToAngleAxis
{
    public static void ToAngleAxis(this Quaternion quat, RotationMode mode,
        out float angle, out Vector3 axis)
    {
        quat.ToAngleAxis(out angle, out axis);
        switch (mode)
        {
            case RotationMode.Positive:
                if (!IsPositive(axis))
                    ReverseAngleAxis(ref angle, ref axis);
                break;
            case RotationMode.Negative:
                if (IsPositive(axis))
                    ReverseAngleAxis(ref angle, ref axis);
                break;
            case RotationMode.Longest:
                if (angle < 180f)
                    ReverseAngleAxis(ref angle, ref axis);
                break;
            default:
                if (angle > 180f)
                    ReverseAngleAxis(ref angle, ref axis);
                break;
        }
    }

    static bool IsPositive(Vector3 vector)
    {
        float sign = Mathf.Sign(vector.x) * Mathf.Sign(vector.y) * Mathf.Sign(vector.z);
        return sign >= 0;
    }

    static void ReverseAngleAxis(ref float angle, ref Vector3 axis)
    {
        angle = 360f - angle;
        axis = -axis;
    }

    /// <summary>
    /// Returns the angle of a quaternion rotation.
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

    /// <summary>
    /// Returns the angle of a quaternion rotation.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static float Angle(this Quaternion quat, RotationMode mode)
    {
        quat.ToAngleAxis(mode, out float angle, out Vector3 axis);
        return angle;
    }

    /// <summary>
    /// Returns the axis of a quaternion rotation.
    /// </summary>
    /// <param name="quat">Original quaternion</param>
    /// <returns>Angle in degrees</returns>
    public static Vector3 Axis(this Quaternion quat, RotationMode mode)
    {
        quat.ToAngleAxis(mode, out float angle, out Vector3 axis);
        return axis;
    }

    public static float Angle(this Quaternion quat, RotationMode mode, Quaternion other)
    {
        Quaternion dif = quat.Subtract(other);
        return Mathf.Abs(dif.Angle(mode));
    }
}
