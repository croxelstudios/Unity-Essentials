using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension_TransformUnscaledPoint
{
    public static Vector3 TransformUnscaledPoint(this Transform tr, Vector3 inPosition)
    {
        return GetLocalToWorldMatrix(tr).MultiplyPoint(inPosition);
    }

    public static Vector3 InverseTransformUnscaledPoint(this Transform tr, Vector3 inPosition)
    {
        Vector3 newPosition, localPosition;
        if (tr.parent)
            localPosition = tr.parent.InverseTransformUnscaledPoint(inPosition);
        else
            localPosition = inPosition;

        localPosition -= tr.localPosition;
        newPosition = Quaternion.Inverse(tr.localRotation) * localPosition;

        return newPosition;
    }

    static Matrix4x4 GetLocalToWorldMatrix(Transform tr)
    {
        Matrix4x4 t = new Matrix4x4();
        t.SetTRS(tr.localPosition, tr.localRotation, Vector3.one);

        if (tr.parent != null)
            t = GetLocalToWorldMatrix(tr.parent) * t;

        return t;
    }
}
