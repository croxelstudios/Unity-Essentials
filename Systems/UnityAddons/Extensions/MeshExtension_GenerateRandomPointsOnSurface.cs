using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtension_GenerateRandomPointsOnSurface
{
    public static Vector3[] GenerateRandomPointsOnSurface<T>(this Mesh mesh,
        int amount, float randomVariation, ref T normals) where T : IList<Vector3>
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

        float distrib = totalArea / amount;
        for (int i = 0; i < amount; i++)
        {
            //Get the triangle
            float r = Mathf.Lerp(distrib * i, Random.Range(0, totalArea), randomVariation);
            float c = 0f;
            int j = 0;
            for (j = 0; j < areas.Length; j++)
            {
                c += areas[j];
                if (c > r) break;
            }

            //Get mapping for that triangle
            float cb = c - areas[j];
            r = Mathf.InverseLerp(cb, c, r);
            Vector2 mapping = r.MapToVector();

            //Set point
            int tri = j * 3;
            result[i] =
                MapVectorToTrianglePoint(vertices[triangles[tri]],
            vertices[triangles[tri + 1]], vertices[triangles[tri + 2]], mapping);
            float dist0 = Vector3.Distance(result[i], vertices[triangles[tri]]);
            float dist1 = Vector3.Distance(result[i], vertices[triangles[tri + 1]]);
            float dist2 = Vector3.Distance(result[i], vertices[triangles[tri + 2]]);
            float totDist = dist0 + dist1 + dist2;
            dist0 = 1f - (dist0 / totDist);
            dist1 = 1f - (dist1 / totDist);
            dist2 = 1f - (dist2 / totDist);
            Vector3 normal = ((meshNormals[triangles[tri]] * dist0) + (meshNormals[triangles[tri + 1]] * dist1) +
                    (meshNormals[triangles[tri + 2]] * dist2)).normalized;
            norm[i] = normal;
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
        return MapVectorToTrianglePoint(p0, p1, p2, new Vector2(Random.value, Random.value));
    }

    public static Vector3 MapVectorToTrianglePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 mapping)
    {
        Vector3 a = p1 - p0;
        Vector3 b = p2 - p0;
        float u1 = mapping.x;
        float u2 = mapping.y;
        if ((u1 + u2) > 1f)
        {
            u1 = 1 - u1;
            u2 = 1 - u2;
        }
        return p0 + ((u1 * a) + (u2 * b));
    }
}
