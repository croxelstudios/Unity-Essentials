using UnityEngine;

public static class QuaternionExtension_ToVector
{
    public static Vector4 ToVector(this Quaternion quat)
    {
        return new Vector4(quat.x, quat.y, quat.z, quat.w);
    }
}
