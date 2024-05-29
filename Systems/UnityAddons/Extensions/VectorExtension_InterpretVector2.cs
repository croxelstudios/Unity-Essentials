using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension_InterpretVector2
{
    //TO DO: Theres probably a better mathematical way of doing this
    public static Vector3 InterpretVector2(this Vector2 input, Vector3 planeNormal, Vector3 planeUp)
    {
        Quaternion rotateNormal = Quaternion.FromToRotation(Vector3.forward, planeNormal);
        Quaternion rotateUpVector = Quaternion.FromToRotation(rotateNormal * Vector3.up, planeUp);

        return rotateNormal * rotateUpVector * input;
    }

    public static Vector3 InterpretVector2(this Vector3 input, Vector3 planeNormal, Vector3 planeUp)
    {
        return ((Vector2)input).InterpretVector2(planeNormal, planeUp);
    }
}
