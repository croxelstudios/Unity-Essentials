using UnityEngine;

public static class VectorExtension_InterpretVector2
{
    //TO DO: Theres probably a better mathematical way of doing this
    public static Vector3 InterpretVector2(this Vector2 input, Vector3 planeNormal, Vector3 planeUp)
    {
        Quaternion rotateNormal = Quaternion.FromToRotation(Vector3.forward, planeNormal);
        Quaternion rotateUpVector = Quaternion.AngleAxis(
            Vector3.SignedAngle(rotateNormal * Vector3.up, planeUp, planeNormal), planeNormal);

        return rotateUpVector * rotateNormal * input;
    }

    public static Vector3 InterpretVector2(this Vector3 input, Vector3 planeNormal, Vector3 planeUp)
    {
        return ((Vector2)input).InterpretVector2(planeNormal, planeUp);
    }
}
