using System;
using UnityEngine;

[Serializable]
public struct AxisBooleans
{
    public bool x;
    public bool y;
    public bool z;

    public AxisBooleans(bool x, bool y, bool z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x ? 1f : 0f, y ? 1f : 0f, z ? 1f : 0f);
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x ? 1 : 0, y ? 1 : 0, z ? 1 : 0);
    }
}
