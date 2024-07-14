using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtension_GenerateRandomPointsOnSurface
{
    public static Vector3[] GenerateRandomPointsOnSurface<T>(this Mesh mesh, int amount, ref T normals) where T : IList<Vector3>
    {
        Vector3[] result = new Vector3[amount];
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;
        Vector3[] norm = new Vector3[amount];
        float[] areas = new float[triangles.Length / 3];
        float totalArea = 0f;
        for (int i = 0; i < areas.Length; i++)
        {
            int tri = i * 3;
            float area =
                CalculateTriangleArea(vertices[triangles[tri]],
                vertices[triangles[tri + 1]], vertices[triangles[tri + 2]]);
            areas[i] = area;
            totalArea += area;
        }

        int resultCount = 0;
        for (int i = 0; i < areas.Length; i++)
        {
            int tri = i * 3;
            float areaPercent = areas[i] / totalArea;
            int n = Mathf.FloorToInt(amount * areaPercent);
            Vector3 triNormal = (meshNormals[triangles[tri]] + meshNormals[triangles[tri + 1]] +
                    meshNormals[triangles[tri + 2]]).normalized;
            for (int j = 0; j < n; j++)
            {
                result[resultCount] =
                    RandomPointOnTriangle(vertices[triangles[tri]],
                vertices[triangles[tri + 1]], vertices[triangles[tri + 2]]);
                norm[resultCount] = triNormal;

                resultCount++;
            }
        }

        normals = (T)(object)norm;

        return result;
    }

    public static float CalculateTriangleArea(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
    }

    public static Vector3 RandomPointOnTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 a = p1 - p0;
        Vector3 b = p2 - p0;
        float u1 = Random.value;
        float u2 = Random.value;
        if ((u1 + u2) > 1f)
        {
            u1 = 1 - u1;
            u2 = 1 - u2;
        }
        return p0 + ((u1 * a) + (u2 * b));
    }
}
