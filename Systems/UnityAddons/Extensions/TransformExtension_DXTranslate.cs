using UnityEngine;

public static class TransformExtension_DXTranslate
{
    public static void DXTranslate(this Transform transform, Vector3 value, Space space = Space.Self)
    {
        if (space == Space.World)
            transform.position += value;
        else transform.localPosition += value;
    }
}
