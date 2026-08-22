using UnityEngine;

public static class VectorExtension_InterpretVector2
{
    public static Vector3 InterpretVector2(this Vector2 input, Vector3 planeNormal, Vector3 planeUp)
    {
        planeNormal.Normalize();
        Vector3 up = Vector3.ProjectOnPlane(planeUp, planeNormal).normalized;
        Vector3 right = Vector3.Cross(up, planeNormal);

        return (right * input.x) + (up * input.y);
    }

    public static Vector3 InterpretVector2(this Vector3 input, Vector3 planeNormal, Vector3 planeUp)
    {
        return ((Vector2)input).InterpretVector2(planeNormal, planeUp);
    }
}
