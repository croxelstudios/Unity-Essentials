using UnityEngine;

public static class MeshExtension_ApplyMatrix //TO DO: Would be a lot faster if using ComputableMesh.
{
    public static void ApplyMatrix(this Mesh mesh, Matrix4x4 matrix)
    {
        Vector3[] v = mesh.vertices;

        Vector3 scale = matrix.lossyScale;
        Matrix4x4 vMatrix = Matrix4x4.TRS(Vector3.zero, matrix.rotation, scale.Sign());

        for (int i = 0; i < v.Length; i++)
            v[i] = matrix.MultiplyPoint3x4(v[i]);

        if (matrix.determinant < 0f)
        {
            int[] t = mesh.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                int tmp = t[i];
                t[i] = t[i + 1];
                t[i + 1] = tmp;
            }
            mesh.triangles = t;
        }

        mesh.vertices = v;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
