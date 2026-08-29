using UnityEngine;

public static class MeshExtension_Clone
{
    public static Mesh Clone(this Mesh source)
    {
        if (source == null)
            return null;

        Mesh mesh = Object.Instantiate(source);
        return mesh;
    }
}
