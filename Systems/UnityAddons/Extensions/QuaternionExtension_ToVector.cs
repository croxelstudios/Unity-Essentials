using UnityEngine;

public static class QuaternionExtension_ToVector
{
    public static Vector4 ToVector(this Quaternion quat)
    {
        return new Vector4(quat.x, quat.y, quat.z, quat.w);
    }

    public static Quaternion ToQuaternion(this Vector4 vector)
    {
        return new Quaternion(vector.x, vector.y, vector.z, vector.w);
    }
}
