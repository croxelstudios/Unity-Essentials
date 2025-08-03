using Sirenix.OdinInspector;
using System;
using UnityEngine;

[InlineProperty]
[Serializable]
public struct AxisBooleans
{
    [HorizontalGroup(LabelWidth = 10f, Width = 30f)]
    public bool x;
    [HorizontalGroup(LabelWidth = 10f, Width = 30f)]
    public bool y;
    [HorizontalGroup(LabelWidth = 10f, Width = 30f)]
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

[InlineProperty]
[Serializable]
public struct AxisBooleans2D
{
    [HorizontalGroup(LabelWidth = 10f, Width = 30f)]
    public bool x;
    [HorizontalGroup(LabelWidth = 10f, Width = 30f)]
    public bool y;

    public AxisBooleans2D(bool x, bool y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x ? 1f : 0f, y ? 1f : 0f);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x ? 1 : 0, y ? 1 : 0);
    }

    public static implicit operator AxisBooleans(AxisBooleans2D b) => new AxisBooleans(b.x, b.y, false);
    public static implicit operator AxisBooleans2D(AxisBooleans b) => new AxisBooleans2D(b.x, b.y);
}
