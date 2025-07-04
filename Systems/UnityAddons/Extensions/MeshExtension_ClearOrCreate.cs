using UnityEngine;

public static class MeshExtension_ClearOrCreate
{
    public static Mesh ClearOrCreate(this Mesh mesh)
    {
        if (mesh != null) mesh.Clear();
        else mesh = new Mesh();
        return mesh;
    }
}
