using System.Collections.Generic;
using UnityEngine;

public static class MeshExtension_GenerateRandomPointsOnSurface
{
    #region Main function variants
    public static Vector3[] GenerateRandomPointsOnSurface<T>(this MeshFilter filter,
        float density, float randomVariation, out T normals, out int[] resultTris,
        out Vector2[] mappings) where T : IList<Vector3>
    {
        return filter.sharedMesh.GenerateRandomPointsOnSurface(filter.transform.lossyScale,
            density, randomVariation, out normals, out resultTris, out mappings);
    }

    public static Vector3[] GenerateRandomPointsOnSurface<T>(this MeshFilter filter,
        int amount, float randomVariation, out T normals, out int[] resultTris,
        out Vector2[] mappings) where T : IList<Vector3>
    {
        return filter.sharedMesh.GenerateRandomPointsOnSurface(filter.transform.lossyScale,
            amount, randomVariation, out normals, out resultTris, out mappings);
    }

    public static Vector3[] GenerateRandomPointsOnSurface<T>(this Mesh mesh, Vector3 scale,
        float density, float randomVariation, out T normals, out int[] resultTris,
        out Vector2[] mappings) where T : IList<Vector3>
    {
        float totalArea = mesh.CalculateArea(scale, out float[] areas);

        int amount = Mathf.RoundToInt(totalArea * density);
        float distrib = totalArea / amount;

        Vector3[] result = new Vector3[amount];
        Vector3[] meshNormals = mesh.normals;
        Vector3[] norm = new Vector3[amount];
        mappings = new Vector2[amount];
        resultTris = new int[amount];

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
            int triid = j * 3;
            Vector3Int vIds = new Vector3Int(
                mesh.triangles[triid],
                mesh.triangles[triid + 1],
                mesh.triangles[triid + 2]);
            Triangle tri = new Triangle(
                mesh.vertices[vIds.x],
                mesh.vertices[vIds.y],
                mesh.vertices[vIds.z]);

            result[i] = tri.MapVectorToLocation(mapping);

            norm[i] = ProcMesh.TrianglePointNormal(result[i], tri.p0, tri.p1, tri.p0,
                meshNormals[vIds.x], meshNormals[vIds.y], meshNormals[vIds.z]);

            resultTris[i] = triid;
            mappings[i] = mapping;
        }

        normals = (T)(object)norm;

        return result;
    }

    public static Vector3[] GenerateRandomPointsOnSurface<T>(this Mesh mesh, Vector3 scale,
        int amount, float randomVariation, out T normals, out int[] resultTris,
        out Vector2[] mappings) where T : IList<Vector3>
    {
        float totalArea = mesh.CalculateArea(scale, out float[] areas);

        float distrib = totalArea / amount;

        Vector3[] result = new Vector3[amount];
        Vector3[] meshNormals = mesh.normals;
        Vector3[] norm = new Vector3[amount];
        mappings = new Vector2[amount];
        resultTris = new int[amount];

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
            int triid = j * 3;
            Vector3Int vIds = new Vector3Int(
                mesh.triangles[triid],
                mesh.triangles[triid + 1],
                mesh.triangles[triid + 2]);
            Triangle tri = new Triangle(
                mesh.vertices[vIds.x],
                mesh.vertices[vIds.y],
                mesh.vertices[vIds.z]);

            result[i] = tri.MapVectorToLocation(mapping);

            norm[i] = ProcMesh.TrianglePointNormal(result[i], tri.p0, tri.p1, tri.p0,
                meshNormals[vIds.x], meshNormals[vIds.y], meshNormals[vIds.z]);

            resultTris[i] = triid;
            mappings[i] = mapping;
        }

        normals = (T)(object)norm;

        return result;
    }
    #endregion

    #region Mesh triangle helpers
    public static Vector3[] MapVectorsToTrianglePoints(this Mesh mesh,
        ref Vector3[] normals, int[] tris, Vector2[] mappings)
    {
        Vector3[] points = new Vector3[tris.Length];
        for (int i = 0; i < points.Length; i++)
            points[i] = mesh.MapVectorToTrianglePoint(
                out normals[i], tris[i], mappings[i]);

        return points;
    }

    public static Vector3 MapVectorToTrianglePoint(this Mesh mesh, 
        out Vector3 normal, int triid, Vector2 mapping)
    {
        Vector3Int vIds = new Vector3Int(
            mesh.triangles[triid],
            mesh.triangles[triid + 1],
            mesh.triangles[triid + 2]);
        Triangle tri = new Triangle(
            mesh.vertices[vIds.x],
            mesh.vertices[vIds.y],
            mesh.vertices[vIds.z]);

        Vector3 point = tri.MapVectorToLocation(mapping);

        normal = ProcMesh.TrianglePointNormal(point,
            tri.p0, tri.p1, tri.p2, mesh.normals[vIds.x],
            mesh.normals[vIds.y], mesh.normals[vIds.z]);

        return point;
    }

    public static Vector3 MapVectorToTrianglePoint(this Mesh mesh, int tri, Vector2 mapping)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        return Triangle.MapVectorToLocation(vertices[triangles[tri]],
            vertices[triangles[tri + 1]], vertices[triangles[tri + 2]], mapping);
    }

    public static Vector3 TrianglePointNormal(this Mesh mesh, Vector3 point, int tri)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;

        return ProcMesh.TrianglePointNormal(point,
            vertices[triangles[tri]], vertices[triangles[tri + 1]],
            vertices[triangles[tri + 2]], meshNormals[triangles[tri]],
            meshNormals[triangles[tri + 1]], meshNormals[triangles[tri + 2]]);
    }
    #endregion

    #region Calculate Area functions
    public static float CalculateArea(this MeshFilter filter)
    {
        return filter.sharedMesh.CalculateArea(filter.transform.lossyScale);
    }

    public static float CalculateArea(this MeshFilter filter, out float[] triangleAreas)
    {
        return filter.sharedMesh.CalculateArea(filter.transform.lossyScale, out triangleAreas);
    }

    public static float CalculateArea(this Mesh mesh)
    {
        return mesh.CalculateArea(Vector3.one);
    }

    public static float CalculateArea(this Mesh mesh, out float[] triangleAreas)
    {
        return mesh.CalculateArea(Vector3.one, out triangleAreas);
    }

    public static float CalculateArea(this Mesh mesh, Vector3 scale)
    {
        float[] triangleAreas = new float[0];
        return mesh.CalculateArea(scale, out triangleAreas);
    }

    public static float CalculateArea(this Mesh mesh, Vector3 scale, out float[] triangleAreas)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        triangleAreas = new float[triangles.Length / 3];
        float totalArea = 0f;
        for (int i = 0; i < triangleAreas.Length; i++)
        {
            int tri = i * 3;
            float area =
                Triangle.CalculateArea(
                    Vector3.Scale(vertices[triangles[tri]], scale),
                    Vector3.Scale(vertices[triangles[tri + 1]], scale),
                    Vector3.Scale(vertices[triangles[tri + 2]], scale));
            triangleAreas[i] = area;
            totalArea += area;
        }

        return totalArea;
    }
    #endregion
}

public struct Triangle
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;

    public Triangle(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        p0 = point0;
        p1 = point1;
        p2 = point2;
    }

    public float CalculateArea()
    {
        return CalculateArea(p0, p1, p2);
    }

    public Vector3 RandomLocation()
    {
        return RandomLocation(p0, p1, p2);
    }

    public Vector3 MapVectorToLocation(Vector2 mapping)
    {
        return MapVectorToLocation(p0, p1, p2, mapping);
    }

    public static float CalculateArea(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
    }

    public static Vector3 RandomLocation(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return MapVectorToLocation(p0, p1, p2, new Vector2(Random.value, Random.value));
    }

    public static Vector3 MapVectorToLocation(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 mapping)
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
