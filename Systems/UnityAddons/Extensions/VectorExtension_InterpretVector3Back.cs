using UnityEngine;

public static class VectorExtension_InterpretVector3Back
{
    public static Vector2 InterpretVector3Back(this Vector3 input, Vector3 planeNormal, Vector3 planeUp)
    {
        Quaternion rotateNormal = Quaternion.FromToRotation(planeNormal, Vector3.forward);
        Quaternion rotateUpVector = Quaternion.FromToRotation(rotateNormal * planeUp, Vector3.up);

        return rotateNormal * rotateUpVector * input;
    }

    public static Vector2 InterpretVector3Back(this Vector2 input, Vector3 planeNormal, Vector3 planeUp)
    {
        return ((Vector3)input).InterpretVector3Back(planeNormal, planeUp);
    }
}
