using UnityEngine;

public static class VectorExtension_Sign
{
    public static Vector2 Sign(this Vector2 vector)
    {
        return new Vector2(Mathf.Sign(vector.x), Mathf.Sign(vector.y));
    }

    public static Vector3 Sign(this Vector3 vector)
    {
        return new Vector3(Mathf.Sign(vector.x), Mathf.Sign(vector.y), Mathf.Sign(vector.z));
    }

    public static Vector4 Sign(this Vector4 vector)
    {
        return new Vector4(Mathf.Sign(vector.x), Mathf.Sign(vector.y), Mathf.Sign(vector.z), Mathf.Sign(vector.w));
    }
}
