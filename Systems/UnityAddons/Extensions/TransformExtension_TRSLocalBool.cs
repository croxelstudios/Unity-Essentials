using Mono.CSharp;
using UnityEngine;

public static class TransformExtension_TRSLocalBool
{
    public static Vector3 Position(this Transform tr, bool local)
    {
        return local ? tr.localPosition : tr.position;
    }

    public static Quaternion Rotation(this Transform tr, bool local)
    {
        return local ? tr.localRotation : tr.rotation;
    }

    public static Vector3 Scale(this Transform tr, bool local)
    {
        return local ? tr.localScale : tr.lossyScale;
    }

    public static Vector3 EulerAngles(this Transform tr, bool local)
    {
        return local ? tr.localEulerAngles : tr.eulerAngles;
    }
}
