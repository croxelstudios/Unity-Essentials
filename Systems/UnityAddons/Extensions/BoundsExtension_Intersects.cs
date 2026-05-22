using UnityEngine;

public static class BoundsExtension_Intersects
{
    public static bool Intersects(this Bounds a, Bounds b, float tolerance)
    {
        a.Expand(tolerance * 2f);
        b.Expand(tolerance * 2f);
        return a.Intersects(b);
    }
}
