using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProcMesh
{
    public static void RegisterTriangle(int v0, int v1, int v2, ref List<int> triangles)
    {
        triangles.Add(v0);
        triangles.Add(v1);
        triangles.Add(v2);
    }

    public static void RegisterQuad(int v0, int v1, int v2, int v3, ref List<int> triangles)
    {
        RegisterTriangle(v0, v1, v3, ref triangles);
        RegisterTriangle(v3, v1, v2, ref triangles);
    }

    public static void PositionQuad<T>(int br, int bl, int tr, int tl,
        Vector3 origin, Vector3 direction, Vector3 normal, Vector2 size,
        ref T vertices, ref T normals) where T : IList<Vector3>
    {
        float halfSizeX = size.x * 0.5f;
        direction = direction.normalized;
        normal = normal.normalized;
        Vector3 sideways = Vector3.Cross(direction, normal).normalized;
        normal = Vector3.Cross(sideways, direction).normalized;

        vertices[br] = origin + (sideways * halfSizeX);
        vertices[bl] = origin - (sideways * halfSizeX);
        vertices[tr] = vertices[br] + (direction * size.y);
        vertices[tl] = vertices[bl] + (direction * size.y);

        normals[br] = normal;
        normals[bl] = normal;
        normals[tr] = normal;
        normals[tl] = normal;
    }

    public static void MapQuadUVs(int br, int bl, int tr, int tl,
        ref List<Vector2> uvs, bool reverseUV = false)
    {
        uvs[br] = reverseUV ? Vector2.zero : Vector2.right;
        uvs[bl] = reverseUV ? Vector2.right : Vector2.zero;
        uvs[tr] = reverseUV ? Vector2.up : Vector2.one;
        uvs[tl] = reverseUV ? Vector2.one : Vector2.up;
    }

    public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }

    public static Vector3 CubicCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 p0 = QuadraticCurve(a, b, c, t);
        Vector3 p1 = QuadraticCurve(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }

    public static int[] AddCircleOfPoints(uint sides, Vector3 position, Vector3 direction,
        float width, ref List<Vector3> pointList, ref List<Vector3> normalsList,
        ref List<Vector2> uvList, float verticalUV)
    {
        float angle = 360f / sides;
        Vector3 normal = Vector3.Cross(direction, direction + Vector3.right).normalized;
        List<int> points = new List<int>();
        for (int i = 0; i < sides; i++)
        {
            points.Add(pointList.Count);
            pointList.Add(position + (normal * width));
            normalsList.Add(normal);
            uvList.Add(new Vector2(i / sides, verticalUV));
            normal = Quaternion.AngleAxis(angle, direction) * normal;
        }
        return points.ToArray();
    }
}
