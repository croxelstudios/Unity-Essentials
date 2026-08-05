using UnityEngine;

public static class QuaternionExtension_ProjectOnPlane
{
    public static Quaternion ProjectOnPlane(this Quaternion value, Vector3 normal)
    {
        normal.Normalize();

        Vector3 v = new Vector3(value.x, value.y, value.z);

        float angle = 2.0f * Mathf.Atan2(Vector3.Dot(normal, v), value.w);

        float degAngle = angle * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(degAngle, normal);
    }
}
