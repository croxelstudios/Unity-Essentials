using Mono.CSharp;
using UnityEngine;

public static class MeshExtension_DrawTriangle
{
    public static void DrawAllTriangles(this Mesh mesh, float offset = 0f, bool drawVertexOrder = false)
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
            mesh.DrawTriangle(i, offset, drawVertexOrder);
    }

    public static void DrawTriangle(this Mesh mesh, int i, float offset = 0f, bool drawVertexOrder = false)
    {
        float perc = i / (float)mesh.triangles.Length;

        Vector3 v1 = mesh.vertices[mesh.triangles[i]];
        Vector3 v2 = mesh.vertices[mesh.triangles[i + 1]];
        Vector3 v3 = mesh.vertices[mesh.triangles[i + 2]];
        Vector3 centroid = (v1 + v2 + v3) / 3f;
        v1 += ((centroid - v1).normalized * 0.01f) + Vector3.right * offset;
        v2 += ((centroid - v2).normalized * 0.01f) + Vector3.right * offset;
        v3 += ((centroid - v3).normalized * 0.01f) + Vector3.right * offset;
        Color col = Color.HSVToRGB(perc * 0.8f, 1f, 1f);
        Debug.DrawLine(v1, v2, col);
        Debug.DrawLine(v2, v3, col);
        Debug.DrawLine(v3, v1, col);

        if (drawVertexOrder)
        {
            v1.DebugDraw(col, perc * 0.1f);
            v2.DebugDraw(col, 0.125f + perc * 0.1f);
        }
    }
}
