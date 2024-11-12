using UnityEngine;

public static class VectorExtension_Abs
{
    public static Vector3 Abs(this Vector3 input)
    {
        return new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
    }
}
