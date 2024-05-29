using UnityEngine;

public static class QuaternionExtension_Scale
{
    public static Quaternion Scale(this Quaternion quat, float scale)
    {
        float angle = Quaternion.Angle(Quaternion.identity, quat);
        if (angle <= Mathf.Epsilon) return Quaternion.identity;
        return Quaternion.SlerpUnclamped(Quaternion.identity, quat, scale);
    }

    public static Quaternion SetRotationAmount(this Quaternion quat, float scale)
    {
        float angle = Quaternion.Angle(Quaternion.identity, quat);
        if (angle <= Mathf.Epsilon) return Quaternion.identity;
        return Quaternion.SlerpUnclamped(Quaternion.identity, quat, scale / quat.AbsoluteAngle());
    }
}
