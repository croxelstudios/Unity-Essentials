using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public static class IcoSphere
{
    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    // return index of point in the middle of p1 and p2
    private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret))
        {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3
        (
            (point1.x + point2.x) / 2f,
            (point1.y + point2.y) / 2f,
            (point1.z + point2.z) / 2f
        );

        // add vertex makes sure point is on unit sphere
        int i = vertices.Count;
        vertices.Add(middle.normalized * radius);

        // store it, return index
        cache.Add(key, i);

        return i;
    }

    public static Mesh CreateIcoSphere(int qualityLevel = 3, bool generateSeam = false)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertList = new List<Vector3>();
        Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

        float radius = 0.5f;

        // create 12 vertices of a icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        vertList.Add(new Vector3(-1f, t, 0f).normalized * radius);
        vertList.Add(new Vector3(1f, t, 0f).normalized * radius);
        vertList.Add(new Vector3(-1f, -t, 0f).normalized * radius);
        vertList.Add(new Vector3(1f, -t, 0f).normalized * radius);

        vertList.Add(new Vector3(0f, -1f, t).normalized * radius);
        vertList.Add(new Vector3(0f, 1f, t).normalized * radius);
        vertList.Add(new Vector3(0f, -1f, -t).normalized * radius);
        vertList.Add(new Vector3(0f, 1f, -t).normalized * radius);

        vertList.Add(new Vector3(t, 0f, -1f).normalized * radius);
        vertList.Add(new Vector3(t, 0f, 1f).normalized * radius);
        vertList.Add(new Vector3(-t, 0f, -1f).normalized * radius);
        vertList.Add(new Vector3(-t, 0f, 1f).normalized * radius);


        // create 20 triangles of the icosahedron
        List<TriangleIndices> faces = new List<TriangleIndices>();

        // 5 faces around point 0
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));


        // refine triangles
        for (int i = 0; i < qualityLevel; i++)
        {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (var tri in faces)
            {
                // replace triangle by 4 triangles
                int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }
            faces = faces2;
        }

        mesh.vertices = vertList.ToArray();

        List<int> triList = new List<int>();
        for (int i = 0; i < faces.Count; i++)
        {
            triList.Add(faces[i].v1);
            triList.Add(faces[i].v2);
            triList.Add(faces[i].v3);
        }
        mesh.triangles = triList.ToArray();

        if (generateSeam) //TO DO: Unfinished!!!
        {
            Vector2Int minMax;
            List<int> seam = GenerateSeam(mesh, out minMax);

            Vector2[] uvs = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(
                    Mathf.InverseLerp(-180f, 180f, Vector2.SignedAngle(Vector2.up, new Vector2(mesh.vertices[i].x, -mesh.vertices[i].z))),
                    Mathf.InverseLerp(-radius, radius, mesh.vertices[i].y)
                    );

                if (i == minMax.x)
                {
                    uvs[i].x = 0.5f;
                    uvs[i].y = 0f;
                }
                else if (i == minMax.y)
                {
                    uvs[i].x = 0.5f;
                    uvs[i].y = 1f;
                }
                else if(seam.Contains(i))
                {
                    if (seam.IndexOf(i).IsEven() ^ qualityLevel.IsEven())
                        uvs[i].x += 1f;
                }
            }
            mesh.uv = uvs;
        }
        else
        {
            Vector2[] uvs = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(
                    Mathf.InverseLerp(-0f, 180f, Mathf.Abs(Vector2.SignedAngle(Vector2.up, new Vector2(mesh.vertices[i].x, mesh.vertices[i].z)))),
                    Mathf.InverseLerp(-radius, radius, mesh.vertices[i].y)
                    );
            }
            mesh.uv = uvs;
        }

        Vector3[] normales = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < normales.Length; i++)
            normales[i] = mesh.vertices[i].normalized;
        mesh.normals = normales;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.Optimize();

        return mesh;
    }

    static List<int> GenerateSeam(Mesh mesh, out Vector2Int minMax)
    {
        int max = 0;
        int min = 0;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (mesh.vertices[i].y > mesh.vertices[max].y)
                max = i;
            else if (mesh.vertices[i].y < mesh.vertices[min].y)
                min = i;
        }

        float h0 = Vector3.Distance(mesh.vertices[mesh.triangles[0]] - mesh.vertices[mesh.triangles[2]],
            Vector3.Project(mesh.vertices[mesh.triangles[0]] - mesh.vertices[mesh.triangles[2]],
            mesh.vertices[mesh.triangles[1]] - mesh.vertices[mesh.triangles[2]]));
        float h1 = Vector3.Distance(mesh.vertices[mesh.triangles[1]] - mesh.vertices[mesh.triangles[0]],
            Vector3.Project(mesh.vertices[mesh.triangles[1]] - mesh.vertices[mesh.triangles[0]],
            mesh.vertices[mesh.triangles[2]] - mesh.vertices[mesh.triangles[0]]));
        float h2 = Vector3.Distance(mesh.vertices[mesh.triangles[2]] - mesh.vertices[mesh.triangles[1]],
            Vector3.Project(mesh.vertices[mesh.triangles[2]] - mesh.vertices[mesh.triangles[1]],
            mesh.vertices[mesh.triangles[0]] - mesh.vertices[mesh.triangles[1]]));

        float triHeight = Mathf.Max(h0, h1, h2);

        Edge[] edges = Edge.GetAllEdges(mesh);
        int current = max;
        Vector3 prevDir = Vector3.forward;
        int count = 0;
        List<Edge> seam = new List<Edge>();
        while ((current != min) && (count < mesh.vertexCount))
        {
            float mindis = Mathf.Infinity;
            int chosen = 0;
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].Contains(current) && !seam.Contains(edges[i]))
                {
                    Vector3 dir = edges[i].Direction(current, mesh);
                    float angle = Vector3.Angle(dir, prevDir);
                    if (angle < mindis)
                    {
                        mindis = angle;
                        chosen = i;
                    }
                }
            }
            seam.Add(edges[chosen]);
            prevDir = edges[chosen].Direction(current, mesh);
            current = edges[chosen].Other(current);
            count++;
        }

        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(mesh.vertices);
        int[] triangles = mesh.triangles;

        for (int i = 0; i < seam.Count; i++)
            seam[i].Draw(mesh, Color.red, 5f);

        current = max;
        //Vector2Int[] dupes = new Vector2Int[seam.Count - 1];
        List<int> seamv = new List<int>();
        List<int> excludTris = new List<int>();
        for (int i = 0; i < seam.Count; i++)
        {
            Edge edge = seam[i];
            int next = edge.Other(current);

            excludTris.Add(edge.triangle0);
            excludTris.Add(edge.triangle1);

            int ti0 = GetOppositeIndexInTriangle(edge, edge.triangle0, mesh);
            int ti1 = GetOppositeIndexInTriangle(edge, edge.triangle1, mesh);
            float angle0 = Vector3.SignedAngle(mesh.vertices[mesh.triangles[ti0]], Vector3.forward, Vector3.up);
            float angle1 = Vector3.SignedAngle(mesh.vertices[mesh.triangles[ti1]], Vector3.forward, Vector3.up);

            int tri;
            if (angle0 < angle1) tri = edge.triangle0;
            else tri = edge.triangle1;

            if (i > 0)
            {
                int ticurr = GetIndexInTriangle(current, tri, mesh);
                triangles[ticurr] = vertices.Count - 1;
            }

            if (i < (seam.Count - 1))
            {
                int tinext = GetIndexInTriangle(next, tri, mesh);
                triangles[tinext] = vertices.Count;
                seamv.Add(next);
                seamv.Add(vertices.Count);
                //dupes[i] = new Vector2Int(next, vertices.Count);
                vertices.Add(mesh.vertices[next]);
            }

            current = next;
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (!excludTris.Contains(i))
                for (int j = 0; j < 3; j++)
                {
                    int curr = i + j;
                    int next = i + ((j + 1) % 3);
                    int tricurr = triangles[curr];
                    if (seamv.Contains(tricurr))
                    {
                        Vector3 vert = mesh.vertices[triangles[next]];
                        float angle = Vector3.SignedAngle(vert, Vector3.forward, Vector3.up);
                        if (angle < 0f) triangles[curr] = seamv[seamv.IndexOf(tricurr) + 1];
                    }
                }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;

        minMax = new Vector2Int(min, max);
        return seamv;
    }

    static int GetIndexInTriangle(int vertex, int triangle, Mesh mesh)
    {
        if (mesh.triangles[triangle] == vertex)
            return triangle;
        else if (mesh.triangles[triangle + 1] == vertex)
            return triangle + 1;
        else if (mesh.triangles[triangle + 2] == vertex)
            return triangle + 2;
        else return -1;
    }

    static int GetOppositeIndexInTriangle(Edge edge, int triangle, Mesh mesh)
    {
        if (!edge.Contains(mesh.triangles[triangle]))
            return triangle;
        else if (!edge.Contains(mesh.triangles[triangle + 1]))
            return triangle + 1;
        else if (!edge.Contains(mesh.triangles[triangle + 2]))
            return triangle + 2;
        else return -1;
    }

    struct Edge : IEquatable<Edge>
    {
        public int vertex0;
        public int vertex1;
        public int triangle0;
        public int triangle1;

        public Edge(int vertex0, int vertex1, int triangle0, int triangle1)
        {
            this.vertex0 = vertex0;
            this.vertex1 = vertex1;
            this.triangle0 = triangle0;
            this.triangle1 = triangle1;
        }

        public Edge(int vertex0, int vertex1, Mesh mesh)
        {
            this.vertex0 = vertex0;
            this.vertex1 = vertex1;

            triangle0 = -1;
            triangle1 = -1;

            CalculateTriangles(mesh);
        }

        public Edge(int vertex0, int vertex1)
        {
            this.vertex0 = vertex0;
            this.vertex1 = vertex1;

            triangle0 = -1;
            triangle1 = -1;
        }

        public void CalculateTriangles(Mesh mesh)
        {
            triangle0 = -1;
            triangle1 = -1;

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                if (((mesh.triangles[i] == vertex0) ||
                    (mesh.triangles[i + 1] == vertex0) ||
                    (mesh.triangles[i + 2] == vertex0)) &&
                    ((mesh.triangles[i] == vertex1) ||
                    (mesh.triangles[i + 1] == vertex1) ||
                    (mesh.triangles[i + 2] == vertex1)))
                {
                    if (triangle0 >= 0) triangle1 = i;
                    else triangle0 = i;
                }
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is CustomTagItem)) return false;
            return Equals((CustomTagItem)other);
        }

        public bool Equals(Edge other)
        {
            return (((vertex0 == other.vertex0) && (vertex1 == other.vertex1)) ||
                ((vertex0 == other.vertex1) && (vertex1 == other.vertex0)));
        }

        public override int GetHashCode()
        {
            return (vertex0 + vertex1).GetHashCode() * 31 + (triangle0 + triangle1).GetHashCode();
        }

        public static bool operator ==(Edge o1, Edge o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(Edge o1, Edge o2)
        {
            return !o1.Equals(o2);
        }

        public static Edge[] GetAllEdges(Mesh mesh)
        {
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int ind = i + j;
                    int next = i + ((j + 1) % 3);

                    Edge edge = new Edge(mesh.triangles[ind], mesh.triangles[next]);
                    edge.triangle0 = i;

                    if (edges.Contains(edge))
                    {
                        int ix = edges.IndexOf(edge);
                        edge = new Edge(edges[ix].vertex0, edges[ix].vertex1, edges[ix].triangle0, edge.triangle0);
                        edges.Remove(edge);
                        edges.Add(edge);
                    }
                    else edges.Add(edge);
                }
            }
            return edges.ToArray();
        }

        public bool Contains(int vertex)
        {
            return (vertex == vertex0) || (vertex == vertex1);
        }

        public Vector3 Direction(int vertex, Mesh mesh)
        {
            if (vertex >= mesh.vertexCount)
                return Vector3.zero;

            if (vertex == vertex0)
                return mesh.vertices[vertex1] - mesh.vertices[vertex0];
            else if (vertex == vertex1)
                    return mesh.vertices[vertex0] - mesh.vertices[vertex1];
            else return ((mesh.vertices[vertex0] + mesh.vertices[vertex1]) * 0.5f) - mesh.vertices[vertex];
        }

        public int Other(int vertex)
        {
            if (vertex == vertex0) return vertex1;
            else if (vertex == vertex1) return vertex0;
            else return -1;
        }

        public void Draw(Mesh mesh, Color color, float duration)
        {
            Debug.DrawLine(mesh.vertices[vertex0], mesh.vertices[vertex1], color, duration);
        }
    }
}
