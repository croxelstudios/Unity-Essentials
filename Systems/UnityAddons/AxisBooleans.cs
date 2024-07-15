using System;

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
}
