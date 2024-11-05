using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public static class ProcMesh
{
    public static void RegisterTriangle(uint v0, uint v1, uint v2, int index, ref NativeArray<uint> triangles)
    {
        triangles[index] = v0;
        triangles[index + 1] = v1;
        triangles[index + 2] = v2;
    }

    public static void RegisterTriangle(int v0, int v1, int v2, ref List<int> triangles)
    {
        triangles.Add(v0);
        triangles.Add(v1);
        triangles.Add(v2);
    }

    public static void RegisterQuad(uint v0, uint v1, uint v2, uint v3, int index, ref NativeArray<uint> triangles)
    {
        RegisterTriangle(v0, v1, v3, index, ref triangles);
        RegisterTriangle(v3, v1, v2, index + 3, ref triangles);
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
        Vector3 vbr = Vector3.zero;
        Vector3 vbl = Vector3.zero;
        Vector3 vtr = Vector3.zero;
        Vector3 vtl = Vector3.zero;
        Vector3 nbr = Vector3.up;
        Vector3 nbl = Vector3.up;
        Vector3 ntr = Vector3.up;
        Vector3 ntl = Vector3.up;
        PositionQuad(ref vbr, ref vbl, ref vtr, ref vtl, ref nbr, ref nbl, ref ntr, ref ntl,
            origin, direction, normal, size);

        vertices[br] = vbr;
        vertices[bl] = vbl;
        vertices[tr] = vtr;
        vertices[tl] = vtl;

        normals[br] = nbr;
        normals[bl] = nbl;
        normals[tr] = ntr;
        normals[tl] = ntl;
    }

    public static void PositionQuad<T>(int br, int bl, int tr, int tl, Matrix4x4 transform,
        ref T vertices, ref T normals) where T : IList<Vector3>
    {
        Vector3 vbr = Vector3.zero;
        Vector3 vbl = Vector3.zero;
        Vector3 vtr = Vector3.zero;
        Vector3 vtl = Vector3.zero;
        Vector3 nbr = Vector3.up;
        Vector3 nbl = Vector3.up;
        Vector3 ntr = Vector3.up;
        Vector3 ntl = Vector3.up;
        PositionQuad(ref vbr, ref vbl, ref vtr, ref vtl, ref nbr, ref nbl, ref ntr, ref ntl, transform);

        vertices[br] = vbr;
        vertices[bl] = vbl;
        vertices[tr] = vtr;
        vertices[tl] = vtl;

        normals[br] = nbr;
        normals[bl] = nbl;
        normals[tr] = ntr;
        normals[tl] = ntl;
    }

    public static void PositionQuad(ref Vector3 br, ref Vector3 bl, ref Vector3 tr, ref Vector3 tl,
        ref Vector3 nbr, ref Vector3 nbl, ref Vector3 ntr, ref Vector3 ntl,
        Vector3 origin, Vector3 direction, Vector3 normal, Vector2 size)
    {
        float halfSizeX = size.x * 0.5f;
        direction = direction.normalized;
        normal = normal.normalized;
        Vector3 sideways = Vector3.Cross(direction, normal).normalized;
        normal = Vector3.Cross(sideways, direction).normalized;

        br = origin + (sideways * halfSizeX);
        bl = origin - (sideways * halfSizeX);
        tr = br + (direction * size.y);
        tl = bl + (direction * size.y);

        nbr = normal;
        nbl = normal;
        ntr = normal;
        ntl = normal;
    }

    public static void PositionQuad(ref Vector3 br, ref Vector3 bl, ref Vector3 tr, ref Vector3 tl,
        ref Vector3 nbr, ref Vector3 nbl, ref Vector3 ntr, ref Vector3 ntl,
        Matrix4x4 transform)
    {
        Vector3 vbr = new Vector3(0.5f, -0.5f, 0f);
        Vector3 vbl = new Vector3(-0.5f, -0.5f, 0f);
        Vector3 vtr = new Vector3(0.5f, 0.5f, 0f);
        Vector3 vtl = new Vector3(-0.5f, 0.5f, 0f);

        br = transform.MultiplyPoint(vbr);
        bl = transform.MultiplyPoint(vbl);
        tr = transform.MultiplyPoint(vtr);
        tl = transform.MultiplyPoint(vtl);

        Vector3 normal = Vector3.Cross(br - bl, tl - bl);
        nbr = normal;
        nbl = normal;
        ntr = normal;
        ntl = normal;
    }

    public static void MapQuadUVs(int br, int bl, int tr, int tl,
        ref List<Vector2> uvs, bool reverseUV = false)
    {
        Vector2 vbr = Vector2.zero;
        Vector2 vbl = Vector2.zero;
        Vector2 vtr = Vector2.zero;
        Vector2 vtl = Vector2.zero;

        MapQuadUVs(ref vbr, ref vbl, ref vtr, ref vtl, reverseUV);

        uvs[br] = vbr;
        uvs[bl] = vbl;
        uvs[tr] = vtr;
        uvs[tl] = vtl;
    }

    public static void MapQuadUVs(int br, int bl, int tr, int tl,
        ref Vector2[] uvs, bool reverseUV = false)
    {
        MapQuadUVs(ref uvs[br], ref uvs[bl], ref uvs[tr], ref uvs[tl], reverseUV);
    }

    public static void MapQuadUVs(ref Vector2 br, ref Vector2 bl, ref Vector2 tr, ref Vector2 tl,
        bool reverseUV = false)
    {
        br = reverseUV ? Vector2.zero : Vector2.right;
        bl = reverseUV ? Vector2.right : Vector2.zero;
        tr = reverseUV ? Vector2.up : Vector2.one;
        tl = reverseUV ? Vector2.one : Vector2.up;
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

    public static Vector3 TrianglePointNormal(
        Vector3 point, Vector3 tp0, Vector3 tp1, Vector3 tp2, Vector3 tn0, Vector3 tn1, Vector3 tn2)
    {
        float dist0 = Vector3.Distance(point, tp0);
        float dist1 = Vector3.Distance(point, tp1);
        float dist2 = Vector3.Distance(point, tp2);
        float totDist = dist0 + dist1 + dist2;
        dist0 = 1f - (dist0 / totDist);
        dist1 = 1f - (dist1 / totDist);
        dist2 = 1f - (dist2 / totDist);
        return ((tn0 * dist0) + (tn1 * dist1) + (tn2 * dist2)).normalized;
    }

    public static void PositionQuadTriangle<T>(int bl, int tl, int br,
        Vector3 origin, Vector3 direction, Vector3 normal, Vector2 size,
        ref T vertices, ref T normals) where T : IList<Vector3>
    {
        Vector3 vbl = Vector3.zero;
        Vector3 vtl = Vector3.zero;
        Vector3 vbr = Vector3.zero;
        Vector3 nbl = Vector3.up;
        Vector3 ntl = Vector3.up;
        Vector3 nbr = Vector3.up;
        PositionQuadTriangle(ref vbl, ref vtl, ref vbr, ref nbl, ref ntl, ref nbr,
            origin, direction, normal, size);

        vertices[bl] = vbl;
        vertices[tl] = vtl;
        vertices[br] = vbr;

        normals[bl] = nbl;
        normals[tl] = ntl;
        normals[br] = nbr;
    }

    public static void PositionQuadTriangle<T>(int bl, int tl, int br, Matrix4x4 transform,
        ref T vertices, ref T normals) where T : IList<Vector3>
    {
        Vector3 vbl = Vector3.zero;
        Vector3 vtl = Vector3.zero;
        Vector3 vbr = Vector3.zero;
        Vector3 nbl = Vector3.up;
        Vector3 ntl = Vector3.up;
        Vector3 nbr = Vector3.up;
        PositionQuadTriangle(ref vbl, ref vtl, ref vbr, ref nbl, ref ntl, ref nbr, transform);

        vertices[bl] = vbl;
        vertices[tl] = vtl;
        vertices[br] = vbr;

        normals[bl] = nbl;
        normals[tl] = ntl;
        normals[br] = nbr;
    }

    public static void PositionQuadTriangle(ref Vector3 bl, ref Vector3 tl, ref Vector3 br,
        ref Vector3 nbl, ref Vector3 ntl, ref Vector3 nbr,
        Vector3 origin, Vector3 direction, Vector3 normal, Vector2 size)
    {
        float halfSizeX = size.x * 0.5f;
        direction = direction.normalized;
        normal = normal.normalized;
        Vector3 sideways = Vector3.Cross(direction, normal).normalized;
        normal = Vector3.Cross(sideways, direction).normalized;

        bl = origin - (sideways * halfSizeX);
        tl = bl + (direction * size.y * 2f);
        br = origin + (sideways * (halfSizeX + size.x));

        nbl = normal;
        ntl = normal;
        nbr = normal;
    }

    public static void PositionQuadTriangle(ref Vector3 bl, ref Vector3 tl, ref Vector3 br,
        ref Vector3 nbl, ref Vector3 ntl, ref Vector3 nbr, Matrix4x4 transform)
    {
        Vector3 vbl = new Vector3(-0.5f, -0.5f, 0f);
        Vector3 vtl = new Vector3(-0.5f, 1.5f, 0f);
        Vector3 vbr = new Vector3(1.5f, -0.5f, 0f);

        bl = transform.MultiplyPoint(vbl);
        tl = transform.MultiplyPoint(vtl);
        br = transform.MultiplyPoint(vbr);

        Vector3 normal = Vector3.Cross(br - bl, tl - bl);
        nbl = normal;
        ntl = normal;
        nbr = normal;
    }

    public static void MapQuadTriangleUVs(int bl, int tl, int br,
        ref List<Vector2> uvs, bool reverseUV = false)
    {
        Vector2 vbl = Vector2.zero;
        Vector2 vtl = Vector2.zero;
        Vector2 vbr = Vector2.zero;

        MapQuadTriangleUVs(ref vbl, ref vtl, ref vbr, reverseUV);

        uvs[bl] = vbl;
        uvs[tl] = vtl;
        uvs[br] = vbr;
    }

    public static void MapQuadTriangleUVs(int bl, int tl, int br,
        ref Vector2[] uvs, bool reverseUV = false)
    {
        MapQuadTriangleUVs(ref uvs[bl], ref uvs[tl], ref uvs[br], reverseUV);
    }

    public static void MapQuadTriangleUVs(ref Vector2 bl, ref Vector2 tl, ref Vector2 br,
        bool reverseUV = false)
    {
        bl = reverseUV ? Vector2.right : Vector2.zero;
        tl = (Vector2.up * 2f) + (reverseUV ? Vector2.right : Vector2.zero);
        br = reverseUV ? -Vector2.right : (Vector2.right * 2f);
    }

    public static void RegisterArbitraryMesh(Mesh mesh, int vOffset, int index, ref NativeArray<uint> triangles, int submesh = 0)
    {
        int[] tris = mesh.GetTriangles(submesh);
        for (int i = 0; i < tris.Length; i++)
            triangles[index + i] = (uint)(tris[i] + vOffset);
    }

    public static void RegisterArbitraryMesh(Mesh mesh, int vOffset, ref List<int> triangles, int submesh = 0)
    {
        int[] tris = mesh.GetTriangles(submesh);
        for (int i = 0; i < tris.Length; i++)
            triangles.Add(tris[i] + vOffset);
    }

    public static void PositionArbitraryMesh<T>(Mesh mesh, ref T vertices, Matrix4x4 transform)
        where T : IList<Vector3>
    {
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = transform.MultiplyPoint(mesh.vertices[i]);
    }

    public static void PositionArbitraryMesh<T>(Mesh mesh, ref T vertices, ref T normals, Matrix4x4 transform)
        where T : IList<Vector3>
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = transform.MultiplyPoint(mesh.vertices[i]);
            normals[i] = transform.rotation * mesh.normals[i];
        }
    }

    public static void MapUVsArbitraryMesh<T>(Mesh mesh, ref T uvs, bool reverseUV = false, int channel = 0)
        where T : IList<Vector2>
    {
        List<Vector2> uv = new List<Vector2>();
        mesh.GetUVs(channel, uv);
        for (int i = 0; i < uvs.Count; i++)
            uvs[i] = reverseUV ? new Vector2(1f - uv[i].x, uv[i].y) : uv[i];
    }

    public static Mesh CreateQuad()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        PositionQuad(0, 1, 2, 3, Matrix4x4.identity, ref vertices, ref normals);
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        List<int> triangles = new List<int>();
        RegisterQuad(0, 1, 2, 3, ref triangles);
        mesh.SetTriangles(triangles, 0);
        Vector2[] uvs = new Vector2[3];
        MapQuadUVs(0, 1, 2, 3, ref uvs);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.name = "proc_Quad";
        return mesh;
    }

    public static Mesh CreateQuadTriangle()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        Vector3[] normals = new Vector3[3];
        PositionQuadTriangle(0, 1, 2, Matrix4x4.identity, ref vertices, ref normals);
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        List<int> triangles = new List<int>();
        RegisterTriangle(0, 1, 2, ref triangles);
        mesh.SetTriangles(triangles, 0);
        Vector2[] uvs = new Vector2[3];
        MapQuadTriangleUVs(0, 1, 2, ref uvs);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.name = "proc_QuadTriangle";
        return mesh;
    }
}
