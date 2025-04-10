using UnityEngine;
using UnityEngine.UIElements;

public static class QuaternionExtension_Scale
{
    public static Quaternion Scale(this Quaternion quat, float scale)
    {
        quat.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle <= Mathf.Epsilon) return Quaternion.identity;
        return Quaternion.AngleAxis(angle * scale, axis);
    }

    public static Quaternion SetRotationAmount(this Quaternion quat, float amount)
    {
        Vector3 axis;
        float angle;
        quat.ToAngleAxis(out angle, out axis);
        if (angle <= Mathf.Epsilon) return Quaternion.identity;
        return Quaternion.AngleAxis(amount, axis);
    }
}
