using UnityEngine;

public static class MeshExtension_Inflate //TO DO: Would be a lot faster if using ComputableMesh.
{
    public static void Inflate(this Mesh mesh, float amount)
    {
        if (mesh == null)
            return;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] += normals[i] * amount;
        mesh.vertices = vertices;

        mesh.RecalculateBounds();
    }
}
