using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class ComputableMeshExtension_Tesselate
{
    static ComputeShader _tesselateCompute;
    public static ComputeShader tesselateCompute
    {
        get
        {
            if (_tesselateCompute == null)
                _tesselateCompute = (ComputeShader)Resources.Load(tesselationComputeShaderName);
            return _tesselateCompute;
        }
    }
    const string tesselationComputeShaderName = "TesselateMeshCompute";
    const string resetDivKernel = "ResetDivisions";
    const string setDivOneKernel = "SetDivisionsToOne";
    const string copyOldVertices = "CopyOldVertices";
    const string writeNewVertices = "WriteNewVertices";
    const string subdivideTriangles = "SubdivideTriangles";
    const string getEdgeDivisions = "GetEdgeDivisions";

    public static void Tesselate(this ComputableMesh mesh, int submesh = -1)
    {
        ComputeBuffer edges = mesh.GetEdges(submesh);
        ComputeBuffer divisions = new(mesh.indexCount, sizeof(int), ComputeBufferType.Structured);
        Compute_ResetDivisions(mesh, divisions, submesh);
        Compute_SetDivisionsToOne(mesh, edges, divisions, submesh);
        ComputeBuffer divEdgesIds = DivEdgesIds(mesh.indexCount);
        TesselateInternal(mesh, edges, divisions, divEdgesIds, mesh.indexCount, submesh);
    }

    public static void Tesselate(this ComputableMesh mesh, float maxTriSize, int submesh = -1)
    {
        mesh.Tesselate(maxTriSize, Matrix4x4.identity, submesh);
    }

    public static void Tesselate(this ComputableMesh mesh, float maxTriSize, Matrix4x4 objectToWorld, int submesh = -1)
    {
        for (int i = 0; i < 100; i++)
        {
            ComputeBuffer edges = mesh.GetEdges(submesh);
            ComputeBuffer divisions = new(mesh.indexCount, sizeof(int), ComputeBufferType.Structured);
            Compute_ResetDivisions(mesh, divisions, submesh);
            Compute_GetEdgeDivisions(mesh, edges, divisions, maxTriSize, objectToWorld, submesh);
            ComputeBuffer divEdgesIds = DivEdgesIds(divisions, out int extraVertices);
            if (extraVertices > 0)
                TesselateInternal(mesh, edges, divisions, divEdgesIds, extraVertices, submesh);
            else break;
        }
    }

    static void TesselateInternal(ComputableMesh mesh, ComputeBuffer edges,
        ComputeBuffer divisions, ComputeBuffer divEdgesIds, int extraVertices, int submesh = -1)
    {
        NativeArray<ComputableMesh.VertexData> vertexData = new(mesh.vertexCount + extraVertices, Allocator.Persistent);

        NativeArray<uint>[] triangleData = new NativeArray<uint>[mesh.subMeshCount];
        for (int i = 0; i < mesh.subMeshCount; i++)
            triangleData[i] = new NativeArray<uint>((int)mesh.GetIndexCount(i), Allocator.Persistent);

        ComputableMesh newMesh = new(vertexData, triangleData, mesh.name + "_Tesselated");
        Compute_CopyOldVertices(mesh, newMesh);
        Compute_WriteNewVertices(newMesh, edges, divEdgesIds, divisions, mesh.vertexCount, extraVertices);

        ComputeBuffer triDiv = new(mesh.indexCount / 3, TriangleDivision.Size(), ComputeBufferType.Structured);

        Compute_SubdivideTriangles(mesh, edges, divisions, triDiv, submesh);

        TriangleDivision[] triDivData = new TriangleDivision[triDiv.count];
        triDiv.GetData(triDivData);

        triangleData = new NativeArray<uint>[mesh.subMeshCount];
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            if ((submesh < 0) || (submesh == i))
            {
                int start = (int)mesh.GetIndexStart(i);
                int count = (int)mesh.GetIndexCount(i);

                List<uint> ind = new();
                for (int j = 0; j < count; j++)
                {
                    TriangleDivision triDivision = triDivData[(start / 3) + (j / 3)];
                    Triangle t = triDivision.tri1;
                    if (t.v1 >= 0)
                    {
                        ind.Add(t.v1);
                        ind.Add(t.v2);
                        ind.Add(t.v3);
                    }
                    t = triDivision.tri2;
                    if (t.v1 >= 0)
                    {
                        ind.Add(t.v1);
                        ind.Add(t.v2);
                        ind.Add(t.v3);
                    }
                    t = triDivision.tri3;
                    if (t.v1 >= 0)
                    {
                        ind.Add(t.v1);
                        ind.Add(t.v2);
                        ind.Add(t.v3);
                    }
                    t = triDivision.tri4;
                    if (t.v1 >= 0)
                    {
                        ind.Add(t.v1);
                        ind.Add(t.v2);
                        ind.Add(t.v3);
                    }
                }

                triangleData[i] = new NativeArray<uint>(ind.Count, Allocator.Persistent);
                for (int j = 0; j < ind.Count; j++)
                    triangleData[i][j] = ind[j];
            }
            else
            {
                triangleData[i] = new NativeArray<uint>((int)mesh.GetIndexCount(i), Allocator.Persistent);
                int[] ind = mesh.mesh.GetIndices(i);
                for (int j = 0; j < ind.Length; j++)
                    triangleData[i][j] = (uint)ind[j];
            }
        }

        newMesh.BakeVertexDataToCPU();
        newMesh.SetTriangles(triangleData);
        mesh.GetDataFromCopy(newMesh);
    }

    static ComputeBuffer DivEdgesIds(int size)
    {
        uint[] ids = new uint[size];
        for (uint i = 0; i < ids.Length; i++)
            ids[i] = i;

        ComputeBuffer buff = new(size, sizeof(uint), ComputeBufferType.Structured);
        buff.SetData(ids);
        return buff;
    }

    static ComputeBuffer DivEdgesIds(ComputeBuffer divisionsBuff, out int size)
    {
        int[] divs = new int[divisionsBuff.count];
        divisionsBuff.GetData(divs);

        List<uint> ids = new List<uint>();
        for (uint i = 0; i < divs.Length; i++)
            if (divs[i] > 0)
                ids.Add(i);

        if (ids.Count <= 0)
        {
            size = 0;
            return null;
        }

        ComputeBuffer buff = new(ids.Count, sizeof(uint), ComputeBufferType.Structured);
        buff.SetData(ids);
        size = ids.Count;
        return buff;
    }

    static void Compute_ResetDivisions(ComputableMesh mesh, ComputeBuffer divisionsBuff, int submesh)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = tesselateCompute.FindKernel(resetDivKernel);
        tesselateCompute.SetInt("indexCount", indexCount);
        tesselateCompute.SetBuffer(ki, "divisions", divisionsBuff);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_SetDivisionsToOne(ComputableMesh mesh, ComputeBuffer edges, ComputeBuffer divisionsBuff, int submesh)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = tesselateCompute.FindKernel(setDivOneKernel);
        tesselateCompute.SetInt("indexCount", indexCount);
        tesselateCompute.SetBuffer(ki, "edges", edges);
        tesselateCompute.SetBuffer(ki, "divisions", divisionsBuff);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_GetEdgeDivisions(ComputableMesh mesh, ComputeBuffer edges, ComputeBuffer divisionsBuff,
        float maxArea, Matrix4x4 objectToWorld, int submesh)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = tesselateCompute.FindKernel(getEdgeDivisions);
        tesselateCompute.SetInt("indexStart", (int)mesh.GetIndexStart(submesh));
        tesselateCompute.SetInt("indexCount", indexCount);
        tesselateCompute.SetInt("indexStride", mesh.indexBuffer.stride);
        tesselateCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        tesselateCompute.SetFloat("areaThreshold", maxArea);
        tesselateCompute.SetMatrix("objectToWorld", objectToWorld);
        tesselateCompute.SetBuffer(ki, "edges", edges);
        tesselateCompute.SetBuffer(ki, "divisions", divisionsBuff);
        tesselateCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        tesselateCompute.SetBuffer(ki, "indices", mesh.indexBuffer);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_CopyOldVertices(ComputableMesh mesh, ComputableMesh newMesh)
    {
        int ki = tesselateCompute.FindKernel(copyOldVertices);
        tesselateCompute.SetInt("vertexCount", mesh.vertexCount);
        tesselateCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        tesselateCompute.SetBuffer(ki, "oldVertices", mesh.vertexBuffer);
        tesselateCompute.SetBuffer(ki, "vertices", newMesh.vertexBuffer);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            mesh.vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_WriteNewVertices(ComputableMesh mesh, ComputeBuffer edges,
        ComputeBuffer divEdgesIds, ComputeBuffer divisions, int oldSize, int extraVertices)
    {
        int ki = tesselateCompute.FindKernel(writeNewVertices);
        tesselateCompute.SetInt("oldSize", oldSize);
        tesselateCompute.SetInt("divEdgesSize", extraVertices);
        tesselateCompute.SetInt("vertexCount", mesh.vertexCount);
        tesselateCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        tesselateCompute.SetBuffer(ki, "edges", edges);
        tesselateCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        tesselateCompute.SetBuffer(ki, "divisions", divisions);
        tesselateCompute.SetBuffer(ki, "divisionEdgesIds", divEdgesIds);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            mesh.vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    struct Triangle
    {
        public uint v1;
        public uint v2;
        public uint v3;

        public Triangle(uint v1, uint v2, uint v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public static int Size()
        {
            return sizeof(uint) * 3;
        }
    }

    struct TriangleDivision
    {
        public Triangle tri1;
        public Triangle tri2;
        public Triangle tri3;
        public Triangle tri4;

        public TriangleDivision(Triangle tri1, Triangle tri2, Triangle tri3, Triangle tri4)
        {
            this.tri1 = tri1;
            this.tri2 = tri2;
            this.tri3 = tri3;
            this.tri4 = tri4;
        }

        public static int Size()
        {
            return Triangle.Size() * 4;
        }
    }

    static void Compute_SubdivideTriangles(ComputableMesh mesh, ComputeBuffer edges,
        ComputeBuffer divisions, ComputeBuffer triangleDivisions, int submesh = -1)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = tesselateCompute.FindKernel(subdivideTriangles);
        tesselateCompute.SetInt("indexStart", (int)mesh.GetIndexStart(submesh));
        tesselateCompute.SetInt("indexCount", indexCount);
        tesselateCompute.SetInt("indexStride", mesh.indexBuffer.stride);
        tesselateCompute.SetBuffer(ki, "indices", mesh.indexBuffer);
        tesselateCompute.SetBuffer(ki, "edges", edges);
        tesselateCompute.SetBuffer(ki, "divisions", divisions);
        tesselateCompute.SetBuffer(ki, "triangleDivisions", triangleDivisions);

        tesselateCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }
}
