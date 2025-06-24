using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System;
using UnityEngine.Events;

public class ComputableMesh
{
    public Mesh original { get; private set; }
    public Mesh mesh { get; private set; }
    public GraphicsBuffer vertexBuffer
    {
        get
        {
            vertexBuf ??= mesh.GetVertexBuffer(0);
            return vertexBuf;
        }
    }
    public GraphicsBuffer indexBuffer
    {
        get
        {
            indexBuf ??= mesh.GetIndexBuffer();
            return indexBuf;
        }
    }

    NativeArray<VertexData> vertexData;
    NativeArray<uint>[] triangleData;
    GraphicsBuffer vertexBuf;
    GraphicsBuffer indexBuf;

    Vector3[] tmpParticleVert;
    Vector3[] tmpParticleNorm;

    struct VertexData //TO DO: Posibility of using less data? ->
                      //Would require different versions of compute shaders
                      //or implementing custom stride reading by passing a
                      //parameter to the compute shader that holds info on the data structure
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector4 tangent;
        public Color color;
        public Vector2 uv;

        public VertexData(Vector3 position, Vector3 normal,
            Vector4 tangent, Color color, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.tangent = tangent;
            this.color = color;
            this.uv = uv;
        }

        public static int Size()
        {
            return
                sizeof(float) * 3 + // Position;
                sizeof(float) * 3 + // Normal;
                sizeof(float) * 4 + // Tangent;
                sizeof(float) * 4 + // Color;
                sizeof(float) * 2; // UV;
        }
    }

    static ComputeShader _genericCompute;
    public static ComputeShader genericCompute
    {
        get
        {
            if (_genericCompute == null)
                _genericCompute = (ComputeShader)Resources.Load(genericComputeShaderName);
            return _genericCompute;
        }
    }
    const string computeShader_ApplyDisplacementKernel = "ApplyDisplacement";
    const string computeShader_ResetDisplacementKernel = "ResetDisplacement";
    const string computeShader_ResetColorsKernel = "ResetColors";
    const string computeShader_ResetVerticesKernel = "ResetVertices";
    const string computeShader_ResetVerticesAndColorKernel = "ResetVerticesAndColor";
    const string computeShader_RecordVerticesKernel = "RecordVertices";
    const string genericComputeShaderName = "ComputableMeshGenericCompute";
    const string cleanNullAreaTrianglesKernel = "CleanNullAreaTriangles";
    const string clearMaskKernel = "ClearMask";
    const string fillMaskKernel = "FillMask";
    const string getSubmeshMaskKernel = "GetSubmeshMask";

    #region Initialize
    public ComputableMesh(string name, int vCount, int tCount)
    {
        Initialize(name, vCount, tCount);
    }

    public ComputableMesh(Mesh meshToCopy, string name)
    {
        Initialize(meshToCopy, name);
    }

    public void Initialize(string name, int vCount, params int[] tCount)
    {
        if (mesh != null)
            mesh.Clear();
        else mesh = new Mesh();

        mesh.name = name;
        mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

        //Vertex data setup
        vertexData = new NativeArray<VertexData>(vCount, Allocator.Persistent);
        SetMeshData(vertexData);

        //Triangles
        triangleData = new NativeArray<uint>[tCount.Length];
        for (int i = 0; i < tCount.Length; i++)
            triangleData[i] = new NativeArray<uint>(tCount[i], Allocator.Persistent);
        SetTriangles(triangleData);
    }

    public void Initialize(Mesh meshToCopy, string name)
    {
        int[] tCount = new int[meshToCopy.subMeshCount];
        for (int i = 0; i < tCount.Length; i++)
            tCount[i] = (int)meshToCopy.GetIndexCount(i);
        Initialize(name, meshToCopy.vertexCount, tCount);
        for (int i = 0; i < meshToCopy.subMeshCount; i++)
            CopyMesh(meshToCopy, 0, 0, i, i);
        original = meshToCopy;
    }
    #endregion

    #region Automatic methods
    public void CopyMesh(Mesh meshToCopy, int vOffset, int indexOffset,
        int originSubmesh = 0, int targetSubmesh = 0)
    {
        Prepare_CopyMesh(meshToCopy, vOffset, indexOffset, originSubmesh, targetSubmesh);

        UpdateMeshData();
    }

    public void CopyTriangles(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        Prepare_CopyTriangles(meshToCopy, vOffset, indexOffset);

        UpdateTrianglesData();
    }

    public void SetIndex(int id, uint newTri, int submesh = 0)
    {
        Prepare_SetIndex(id, newTri, submesh);
        UpdateTrianglesData();
    }

    public void CopyAllVertexData(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        Prepare_CopyAllVertexData(meshToCopy, vOffset, indexOffset);

        UpdateVertexData();
    }

    public void SetVertexColor(int index, Color color)
    {
        Prepare_SetVertexColor(index, color);

        UpdateVertexData();
    }

    public void SetVertexColors(int[] index, Color color)
    {
        Prepare_SetVertexColors(index, color);

        UpdateVertexData();
    }

    public void SetVertexColors(int start, int end, Color color)
    {
        Prepare_SetVertexColors(start, end, color);

        UpdateVertexData();
    }

    public void CopyUVs(Mesh meshToCopy, int vOffset, bool reverse)
    {
        Prepare_CopyUVs(meshToCopy, vOffset, reverse);

        UpdateVertexData();
    }

    public void CopyPositionNormal(
        Mesh meshToCopy, int vOffset, Vector3 offset, Matrix4x4 mat)
    {
        Prepare_CopyPositionNormal(meshToCopy, vOffset, offset, mat);

        UpdateVertexData();
    }
    #endregion

    #region Preparation methods
    public void Prepare_CopyMesh(Mesh meshToCopy, int vOffset, int indexOffset,
        int originSubmesh = 0, int targetSubmesh = 0)
    {
        Prepare_CopyTriangles(meshToCopy, vOffset, indexOffset, originSubmesh, targetSubmesh);

        Prepare_CopyAllVertexData(meshToCopy, vOffset, indexOffset);
    }

    public void Prepare_CopyTriangles(Mesh meshToCopy, int vOffset, int indexOffset,
        int originSubmesh = 0, int targetSubmesh = 0)
    {
        int tCount = meshToCopy.triangles.Length;
        ProcMesh.RegisterArbitraryMesh(meshToCopy, vOffset, indexOffset,
            ref triangleData[targetSubmesh], originSubmesh);
    }

    public void Prepare_SetIndex(int id, uint newTri, int submesh = 0)
    {
        triangleData[submesh][id] = newTri;
    }

    public void Prepare_CopyAllVertexData(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        uint vCount = (uint)meshToCopy.vertexCount;
        for (uint i = 0; i < vCount; i++)
            VData_SetData((int)(vOffset + i), meshToCopy.vertices[i],
                meshToCopy.normals.Length > i ? meshToCopy.normals[i] : Vector3.zero,
                meshToCopy.tangents.Length > i ? meshToCopy.tangents[i] : Vector3.zero,
                meshToCopy.colors.Length > i ? meshToCopy.colors[i] : Color.white,
                meshToCopy.uv.Length > i ? meshToCopy.uv[i] : Vector2.zero);
    }

    public void Prepare_SetVertexColor(int index, Color color)
    {
        VData_SetColor(index, color);
    }

    public void Prepare_SetVertexColors(int[] index, Color color)
    {
        for (int i = 0; i < index.Length; i++)
            Prepare_SetVertexColor(index[i], color);
    }

    public void Prepare_SetVertexColors(int start, int end, Color color)
    {
        for (int i = start; i < end; i++)
            Prepare_SetVertexColor(i, color);
    }

    public void Prepare_CopyUVs(Mesh meshToCopy, int vOffset, bool reverse)
    {
        Vector2[] uvs = new Vector2[meshToCopy.vertexCount];

        ProcMesh.MapUVsArbitraryMesh(meshToCopy, ref uvs, reverse);

        for (int i = 0; i < uvs.Length; i++)
            VData_SetUV(vOffset + i, uvs[i]);
    }

    public void Prepare_CopyPositionNormal(
        Mesh meshToCopy, int vOffset, Vector3 offset, Matrix4x4 mat, bool clearTempArrays = true)
    {
        int vCount = meshToCopy.vertexCount;

        if ((tmpParticleVert == null) || (tmpParticleVert.Length < vCount))
        {
            tmpParticleVert = new Vector3[vCount];
            tmpParticleNorm = new Vector3[vCount];
        }

        ProcMesh.PositionArbitraryMesh(meshToCopy,
                ref tmpParticleVert, ref tmpParticleNorm, offset, mat);

        for (int i = 0; i < vCount; i++)
            VData_SetPositionNormal(vOffset + i, tmpParticleVert[i], tmpParticleNorm[i]);

        if (clearTempArrays)
        {
            tmpParticleVert = null;
            tmpParticleNorm = null;
        }
    }

    public void UpdateTrianglesData()
    {
        SetTriangles(triangleData);
    }

    public void UpdateVertexData()
    {
        SetMeshData(vertexData);
    }

    public void UpdateMeshData()
    {
        UpdateTrianglesData();
        UpdateVertexData();
    }
    #endregion

    #region Helper methods
    void SetMeshData(NativeArray<VertexData> vertexData)
    {
        mesh.SetVertexBufferParams(vertexData.Length,
            new VertexAttributeDescriptor(
                VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(
                VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(
                VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(
                VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
        mesh.SetVertexBufferData(
            vertexData, 0, 0, vertexData.Length, 0, MeshUpdateFlags.DontRecalculateBounds);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        vertexBuf = null;
    }

    void SetTriangles(NativeArray<uint>[] triangles)
    {
        int subMeshCount = triangles.Length;

        int count = 0;
        for (int i = 0; i < subMeshCount; i++)
            count += triangles[i].Length;

        bool changeSize = count != totalIndexCount;
        if (changeSize)
            mesh.SetIndexBufferParams(count, IndexFormat.UInt32);

        mesh.subMeshCount = subMeshCount;

        int offset = 0;
        for (int i = 0; i < subMeshCount; i++)
        {
            count = triangles[i].Length;

            MeshUpdateFlags updateFlags = MeshUpdateFlags.DontRecalculateBounds;
            if (count < mesh.GetIndexCount(i))
                updateFlags |= MeshUpdateFlags.DontValidateIndices;

            mesh.SetIndexBufferData(triangles[i], 0, offset, count, updateFlags);

            offset += count;
        }

        offset = 0;
        for (int i = 0; i < subMeshCount; i++)
        {
            count = triangles[i].Length;
            if (count != mesh.GetIndexCount(i))
            {
                SubMeshDescriptor submesh = new SubMeshDescriptor(offset, count);
                mesh.SetSubMesh(i, submesh);
            }
            offset += count;
        }

        indexBuf = null;
    }

    void VData_SetData(int index, Vector3 position, Vector3 normal,
        Vector4 tangent, Color color, Vector2 uv)
    {
        VertexData data = vertexData[index];
        data.position = position;
        data.normal = normal;
        data.tangent = tangent;
        data.color = color;
        data.uv = uv;
        vertexData[index] = data;
    }

    void VData_SetPositionNormal(int index, Vector3 position, Vector3 normal)
    {
        VertexData data = vertexData[index];
        data.position = position;
        data.normal = normal;
        vertexData[index] = data;
    }

    void VData_SetColor(int index, Color color)
    {
        VertexData data = vertexData[index];
        data.color = color;
        vertexData[index] = data;
    }

    void VData_SetUV(int index, Vector2 uv)
    {
        VertexData data = vertexData[index];
        data.uv = uv;
        vertexData[index] = data;
    }
    #endregion

    #region Methods from Mesh
    public void Clear()
    {
        mesh.Clear();
    }

    public Bounds bounds { get { return mesh.bounds; } }

    public int indexCount { get { return (int)mesh.GetIndexCount(0); } }

    public int totalIndexCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
                count += (int)mesh.GetIndexCount(i);
            return count;
        }
    }

    public int vertexCount { get { return mesh.vertexCount; } }

    public int subMeshCount { get { return mesh.subMeshCount; } }

    public static implicit operator Mesh(ComputableMesh m) => m.mesh;

    public Vector3[] vertices { get { return mesh.vertices; } }
    #endregion

    public void ResetVertexBuffer()
    {
        vertexBuf?.Dispose();
        vertexBuf = null;
    }

    public void ResetVertexColors()
    {
        SetVertexColors(0, vertexCount, Color.white);
    }

    #region Cut mesh
    static ComputeShader cuttingCompute;
    const string cutMeshComputeShaderName = "CutMeshCompute";
    const string getEdgesKernel = "GetEdges";
    const string proccessEdgesKernel = "ProccessDuplicateEdges";
    const string getIntersectionsKernel = "GetIntersections";
    const string getTriangleCutDataKernel = "GetTriangleCutData";
    const string getTriangleCutDataSquareKernel = "GetTriangleCutData_SquareCut";
    const string cleanNullAreaTrianglesInIntersectionKernel = "CleanNullAreaTrianglesInIntersection";
    const float Numthreads_Large = 512;
    const float Numthreads_Small = 128;
    const float Numthreads_2D = 16;

    struct TriangleCutProperties
    {
        public int triCount;
        public int cutPoint1;
        public int cutPoint2;
        public Vector3Int newTri1;
        public Vector3Int newTri2;
        public Vector3Int newTri3;
        public Vector3Int newTri4;

        public static int Size()
        {
            return
                sizeof(int) +
                sizeof(int) + // cutPoint1;
                sizeof(int) + // cutPoint2;
                sizeof(int) * 3 + // Triangle1;
                sizeof(int) * 3 + // Triangle2;
                sizeof(int) * 3 + // Triangle3;
                sizeof(int) * 3; // Triangle4;
        }

        public Vector3Int[] InterpretIndexes(Intersection[] intersections)
        {
            Vector3Int[] result;
            switch (triCount)
            {
                case 3:
                    result = new Vector3Int[3];
                    result[0].x = InterpretTriValue(newTri1.x, intersections);
                    result[0].y = InterpretTriValue(newTri1.y, intersections);
                    result[0].z = InterpretTriValue(newTri1.z, intersections);

                    result[1].x = InterpretTriValue(newTri2.x, intersections);
                    result[1].y = InterpretTriValue(newTri2.y, intersections);
                    result[1].z = InterpretTriValue(newTri2.z, intersections);

                    result[2].x = InterpretTriValue(newTri3.x, intersections);
                    result[2].y = InterpretTriValue(newTri3.y, intersections);
                    result[2].z = InterpretTriValue(newTri3.z, intersections);
                    break;
                case 4:
                    result = new Vector3Int[4];
                    result[0].x = InterpretTriValue(newTri1.x, intersections);
                    result[0].y = InterpretTriValue(newTri1.y, intersections);
                    result[0].z = InterpretTriValue(newTri1.z, intersections);

                    result[1].x = InterpretTriValue(newTri2.x, intersections);
                    result[1].y = InterpretTriValue(newTri2.y, intersections);
                    result[1].z = InterpretTriValue(newTri2.z, intersections);

                    result[2].x = InterpretTriValue(newTri3.x, intersections);
                    result[2].y = InterpretTriValue(newTri3.y, intersections);
                    result[2].z = InterpretTriValue(newTri3.z, intersections);

                    result[3].x = InterpretTriValue(newTri4.x, intersections);
                    result[3].y = InterpretTriValue(newTri4.y, intersections);
                    result[3].z = InterpretTriValue(newTri4.z, intersections);
                    break;
                default:
                    result = new Vector3Int[4];
                    result[0] = newTri1;
                    result[1] = newTri2;
                    result[2] = newTri3;
                    result[3] = newTri4;
                    break;
            }
            return result;
        }

        int InterpretTriValue(int value, Intersection[] intersections)
        {
            switch (value)
            {
                case -1:
                    return intersections[cutPoint1].info;
                case -2:
                    return intersections[cutPoint1].info + 1;
                case -3:
                    return intersections[cutPoint2].info;
                case -4:
                    return intersections[cutPoint2].info + 1;
                default:
                    return value;
            }
        }
    }

    struct Edge
    {
        public int v1;
        public int v2;
        public int index1;
        public int index2;

        public Vector3Int triangle1
        {
            get { return new Vector3Int(index1, index1 + 1, index1 + 2); }
        }

        public Vector3Int triangle2
        {
            get { return new Vector3Int(index2, index2 + 1, index2 + 2); }
        }

        public static int Size()
        {
            return
                sizeof(int) + // v1
                sizeof(int) + // v2;
                sizeof(int) + // index1;
                sizeof(int); // index2;
        }
    }

    struct Intersection
    {
        public int info;
        public int leftIndex;
        public VertexData point;
        public VertexData extraPoint;

        public static int Size()
        {
            return
                sizeof(int) +
                sizeof(uint) +
                VertexData.Size() +
                VertexData.Size();
        }
    };

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint, int submesh = -1)
    {
        CutMesh(planeNormal, planePoint, out int[] side1, out int[] side2, out int[] extremes,
            -1, submesh);
    }

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint, float minArea, int submesh = -1)
    {
        CutMesh(planeNormal, planePoint, out int[] side1, out int[] side2, out int[] extremes,
            minArea, submesh);
    }

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, int submesh = -1)
    {
        CutMesh(planeNormal, planePoint, out side1, out side2, out extremes,
            -1, submesh);
    }

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, float minArea, int submesh = -1)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);

        if (submesh < 0)
        {
            List<int> side1l = new List<int>();
            List<int> side2l = new List<int>();
            List<int> extremesl = new List<int>();
            for (int i = 0; i < subMeshCount; i++)
            {
                CutMesh_Internal(planeNormal, planePoint,
                    out side1, out side2, out extremes, i, minArea);
                side1l.AddRange(side1);
                side2l.AddRange(side2);
                extremesl.AddRange(extremes);
            }
            side1 = side1l.ToArray();
            side2 = side2l.ToArray();
            extremes = extremesl.ToArray();
        }
        else CutMesh_Internal(planeNormal, planePoint,
            out side1, out side2, out extremes, submesh, minArea);
    }

    void CutMesh_Internal(Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, int submesh, float minArea = -1f)
    {
        GetPlaneCutData(planeNormal, planePoint, submesh,
            out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);

        //
        //

        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(intersectionsBuff, cutsDataBuff,
                minArea, (int)mesh.GetIndexCount(submesh));

        RebuildMeshFromCutData(intersectionsBuff, cutsDataBuff, submesh,
            out side1, out side2, out extremes);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint, Vector3 upDirection,
        float squareSize, int submesh = -1)
    {
        CutMesh_Square(planeNormal, planePoint, upDirection, squareSize,
            out int[] side1, out int[] side2, out int[] extremes, -1, submesh);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint, Vector3 upDirection,
        float squareSize, float minArea, int submesh = -1)
    {
        CutMesh_Square(planeNormal, planePoint, upDirection, squareSize,
            out int[] side1, out int[] side2, out int[] extremes, minArea, submesh);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, int submesh = -1)
    {
        CutMesh_Square(planeNormal, planePoint, upDirection, squareSize,
            out side1, out side2, out extremes, -1, submesh);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, float minArea, int submesh = -1)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);

        if (submesh < 0)
        {
            List<int> side1l = new List<int>();
            List<int> side2l = new List<int>();
            List<int> extremesl = new List<int>();
            for (int i = 0; i < subMeshCount; i++)
            {
                CutMesh_Square_Internal(planeNormal, planePoint, upDirection, squareSize,
                    out side1, out side2, out extremes, i, minArea);
                side1l.AddRange(side1);
                side2l.AddRange(side2);
                extremesl.AddRange(extremes);
            }
            side1 = side1l.ToArray();
            side2 = side2l.ToArray();
            extremes = extremesl.ToArray();
        }
        else CutMesh_Square_Internal(planeNormal, planePoint, upDirection, squareSize,
            out side1, out side2, out extremes, submesh, minArea);
    }

    public void CutMesh_Square_Internal(Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, int submesh, float minArea = -1f)
    {
        GetPlaneCutData(planeNormal, planePoint, submesh,
            out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);

        //
        if (squareSize > 0f)
            Compute_GetTriangleCutDatas_SquareCut(intersectionsBuff, cutsDataBuff,
                upDirection, squareSize, submesh);
        //

        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(intersectionsBuff, cutsDataBuff,
                minArea, (int)mesh.GetIndexCount(submesh));

        RebuildMeshFromCutData(intersectionsBuff, cutsDataBuff, submesh,
            out side1, out side2, out extremes);
    }

    void GetPlaneCutData(Vector3 planeNormal, Vector3 planePoint, int submesh,
        out ComputeBuffer intersectionsBuff, out ComputeBuffer triangleCutsDataBuff)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        //Get edges
        ComputeBuffer edgesBuff = new ComputeBuffer(indexCount, Edge.Size());
        Compute_GetEdges(edgesBuff, submesh);
        Compute_ProccessEdges(edgesBuff);

        //Get intersections
        intersectionsBuff = new ComputeBuffer(indexCount, Intersection.Size());
        Compute_GetIntersections(edgesBuff, intersectionsBuff,
            planeNormal, planePoint, indexCount);

        triangleCutsDataBuff = new ComputeBuffer(
            indexCount / 3, TriangleCutProperties.Size());
        Compute_GetTriangleCutDatas(intersectionsBuff, triangleCutsDataBuff, submesh);
    }

    void RebuildMeshFromCutData(ComputeBuffer intersectionsBuff, ComputeBuffer triangleCutsDataBuff,
        int submesh, out int[] side1, out int[] side2, out int[] extremes)
    {
        Intersection[] interArr = new Intersection[intersectionsBuff.count];
        intersectionsBuff.GetData(interArr);
        TriangleCutProperties[] cutsArr = new TriangleCutProperties[triangleCutsDataBuff.count];
        triangleCutsDataBuff.GetData(cutsArr);

        //Structure vertices to add
        List<int> side1l = new List<int>();
        List<int> side2l = new List<int>();
        List<int> extremesl = new List<int>();
        List<VertexData> verticesToAdd = new List<VertexData>();
        for (int i = 0; i < interArr.Length; i++)
            if (interArr[i].info == -1)
            {
                int vertexID = vertexCount + verticesToAdd.Count;

                interArr[i].info = vertexID;
                verticesToAdd.Add(interArr[i].point);
                verticesToAdd.Add(interArr[i].point);

                side1l.Add(vertexID);
                side2l.Add(vertexID + 1);
            }
            else if (interArr[i].info == -2)
            {
                int vertexID = vertexCount + verticesToAdd.Count;

                interArr[i].info = vertexID;
                verticesToAdd.Add(interArr[i].extraPoint);

                extremesl.Add(vertexID);
            }
        side1 = side1l.ToArray();
        side2 = side2l.ToArray();
        extremes = extremesl.ToArray();

        //Structure indices to add
        List<uint> indicesToAdd = new List<uint>();
        List<uint> trianglesToRemove = new List<uint>();
        for (int i = 0; i < cutsArr.Length; i++)
        {
            Vector3Int[] tris;
            if (cutsArr[i].triCount > 1)
            {
                trianglesToRemove.Add((uint)i * 3);
                tris = cutsArr[i].InterpretIndexes(interArr);
                for (int j = 0; j < tris.Length; j++)
                    if (tris[j].x >= 0)
                    {
                        indicesToAdd.Add((uint)tris[j].x);
                        indicesToAdd.Add((uint)tris[j].y);
                        indicesToAdd.Add((uint)tris[j].z);
                    }
            }
        }

        //Add vertices
        AddVertices(verticesToAdd);

        //Replace triangles
        for (int i = 0; i < indicesToAdd.Count; i += 3)
        {
            if (trianglesToRemove.Count > 0)
            {
                int ind = (int)trianglesToRemove[0];
                trianglesToRemove.RemoveAt(0);
                triangleData[submesh][ind] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                triangleData[submesh][ind + 1] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                triangleData[submesh][ind + 2] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                i -= 3;
            }
            else break;
        }

        //Add triangles
        AddIndices(indicesToAdd, submesh);
    }

    void Compute_GetEdges(ComputeBuffer edgesDataBuff, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = cuttingCompute.FindKernel(getEdgesKernel);
        cuttingCompute.SetInt("indexStart", indexStart);
        cuttingCompute.SetInt("indexCount", indexCount);
        cuttingCompute.SetInt("indexStride", indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", indexBuffer);
        cuttingCompute.SetBuffer(ki, "edges", edgesDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    void Compute_ProccessEdges(ComputeBuffer edgesDataBuff)
    {
        int ki = cuttingCompute.FindKernel(proccessEdgesKernel);
        cuttingCompute.SetBuffer(ki, "edges", edgesDataBuff);

        int threadGroups = Mathf.CeilToInt(edgesDataBuff.count / Numthreads_2D);
        cuttingCompute.Dispatch(ki, threadGroups, threadGroups, 1);
    }

    void Compute_GetIntersections(
        ComputeBuffer edgesDataBuff, ComputeBuffer intersectionsBuff,
        Vector3 planeNormal, Vector3 planePoint, int indexCount)
    {
        cuttingCompute.SetVector("planeNormal", planeNormal);
        cuttingCompute.SetVector("planePoint", planePoint);

        int ki = cuttingCompute.FindKernel(getIntersectionsKernel);
        cuttingCompute.SetInt("vertexStride", vertexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "vertices", vertexBuffer);
        cuttingCompute.SetBuffer(ki, "edges", edgesDataBuff);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            indexCount / Numthreads_Large), 1, 1);
    }

    void Compute_GetTriangleCutDatas(
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = cuttingCompute.FindKernel(getTriangleCutDataKernel);
        cuttingCompute.SetInt("indexStart", indexStart);
        cuttingCompute.SetInt("indexCount", indexCount);
        cuttingCompute.SetInt("indexStride", indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    void Compute_GetTriangleCutDatas_SquareCut(
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff,
        Vector3 upDirection, float size, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        cuttingCompute.SetVector("upDirection", upDirection);
        cuttingCompute.SetFloat("size", size);

        int ki = cuttingCompute.FindKernel(getTriangleCutDataSquareKernel);
        cuttingCompute.SetInt("indexStart", indexStart);
        cuttingCompute.SetInt("indexCount", indexCount);
        cuttingCompute.SetInt("indexStride", indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    void Compute_CleanNullAreaTriangles(ComputeBuffer intersectionsBuff,
        ComputeBuffer cutsDataBuff, float minArea, int indexCount)
    {
        cuttingCompute.SetFloat("minArea", minArea);

        int ki = cuttingCompute.FindKernel(cleanNullAreaTrianglesInIntersectionKernel);
        cuttingCompute.SetInt("vertexStride", vertexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "vertices", vertexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }
    #endregion

    #region Utilities
    public void CleanNullAreaTriangles(float minArea)
    {
        if (_genericCompute == null)
            _genericCompute = (ComputeShader)Resources.Load(genericComputeShaderName);

        for (int i = 0; i < subMeshCount; i++)
        {
            int indexCount = (int)mesh.GetIndexCount(i);
            ComputeBuffer toClean = new ComputeBuffer(indexCount / 3, sizeof(uint));
            Compute_CleanNullAreaTriangles(toClean, minArea, i);

            uint[] nullTris = new uint[toClean.count];
            toClean.GetData(nullTris);

            int toRemove = 0;
            for (int j = 0; j < nullTris.Length; j++)
                if (nullTris[j] > 0) toRemove += 3;
            NativeArray<uint> newIndexData = new NativeArray<uint>(
                indexCount - toRemove, Allocator.Persistent);

            int n = 0;
            for (int j = 0; j < indexCount; j++)
            {
                int ind = j / 3;
                if (nullTris[ind] < 1)
                {
                    newIndexData[n] = triangleData[i][j];
                    n++;
                }
            }

            triangleData[i].Dispose();
            triangleData[i] = newIndexData;
        }
        UpdateMeshData();
    }

    void Compute_CleanNullAreaTriangles(ComputeBuffer toClean, float minArea, int submesh = 0)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        _genericCompute.SetFloat("minArea", minArea);

        int ki = _genericCompute.FindKernel(cleanNullAreaTrianglesKernel);
        _genericCompute.SetInt("vertexStride", vertexBuffer.stride);
        _genericCompute.SetInt("vertexCount", vertexBuffer.count);
        _genericCompute.SetBuffer(ki, "vertices", vertexBuffer);
        _genericCompute.SetInt("indexStart", indexStart);
        _genericCompute.SetInt("indexCount", indexCount);
        _genericCompute.SetInt("indexStride", indexBuffer.stride);
        _genericCompute.SetBuffer(ki, "indices", indexBuffer);
        _genericCompute.SetBuffer(ki, "toClean", toClean);

        _genericCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    public void UpdateVertexDataToCPU()
    {
        VertexData[] vertices = new VertexData[vertexCount];
        vertexBuf.GetData(vertices);
        for (int i = 0; i < vertices.Length; i++)
            vertexData[i] = vertices[i];
        UpdateVertexData();
    }

    public void UpdateIndexDataToCPU()
    {
        uint[] indices = new uint[totalIndexCount];
        indexBuf.GetData(indices);
        int prev = 0;
        int tri = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            if (i - prev == triangleData[tri].Length)
            {
                prev += triangleData[tri].Length;
                tri++;
            }
            triangleData[tri][i - prev] = indices[i];
        }
        UpdateTrianglesData();
    }

    public void UpdateMeshDataToCPU()
    {
        UpdateVertexDataToCPU();
        UpdateIndexDataToCPU();
    }

    public void SetNull()
    {
        mesh = null;
        original = null;
    }

    public ComputeBuffer SubmeshVertexMask(int submesh)
    {
        if (_genericCompute == null)
            _genericCompute = (ComputeShader)Resources.Load(genericComputeShaderName);

        ComputeBuffer mask = new ComputeBuffer(vertexCount, sizeof(uint));
        if (submesh < 0)
        {
            Compute_FillMask(mask);
            return mask;
        }
        else
        {
            Compute_ClearMask(mask);
            if (submesh >= subMeshCount)
                return mask;
            else
            {
                Compute_SubmeshVertexMask(mask, submesh);
                return mask;
            }
        }
    }

    void Compute_ClearMask(ComputeBuffer mask)
    {
        int ki = _genericCompute.FindKernel(clearMaskKernel);
        _genericCompute.SetBuffer(ki, "mask", mask);

        _genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Numthreads_Small), 1, 1);
    }

    void Compute_FillMask(ComputeBuffer mask)
    {
        int ki = _genericCompute.FindKernel(fillMaskKernel);
        _genericCompute.SetBuffer(ki, "mask", mask);

        _genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Numthreads_Small), 1, 1);
    }

    void Compute_SubmeshVertexMask(ComputeBuffer mask, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = _genericCompute.FindKernel(getSubmeshMaskKernel);
        _genericCompute.SetInt("indexStart", indexStart);
        _genericCompute.SetInt("indexCount", indexCount);
        _genericCompute.SetInt("indexStride", indexBuffer.stride);
        _genericCompute.SetBuffer(ki, "indices", indexBuffer);
        _genericCompute.SetBuffer(ki, "mask", mask);

        _genericCompute.Dispatch(ki, Mathf.CeilToInt(
            indexCount / Numthreads_Small), 1, 1);
    }

    public static void Compute_ResetDisplacement(ComputeBuffer displacement)
    {
        ComputeShader compute = genericCompute;
        int ki = compute.FindKernel(computeShader_ResetDisplacementKernel);

        compute.SetInt("dispStride", displacement.stride);
        compute.SetInt("dispSize", displacement.count);
        compute.SetBuffer(ki, "displacement", displacement);

        compute.Dispatch(ki, Mathf.CeilToInt(
            displacement.count / Numthreads_Small), 1, 1);
    }

    public static void Compute_ResetColors(ComputeBuffer colors)
    {
        ComputeShader compute = genericCompute;
        int ki = compute.FindKernel(computeShader_ResetColorsKernel);

        compute.SetInt("colorsStride", colors.stride);
        compute.SetInt("colorsSize", colors.count);
        compute.SetBuffer(ki, "colors", colors);

        compute.Dispatch(ki, Mathf.CeilToInt(
            colors.count / Numthreads_Small), 1, 1);
    }

    public static void Compute_ApplyDisplacement(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement)
    {
        ComputeShader compute = genericCompute;

        int ki = compute.FindKernel(computeShader_ApplyDisplacementKernel);

        compute.SetInt("dispStride", displacement.stride);
        compute.SetInt("dispSize", displacement.count);
        compute.SetBuffer(ki, "displacement", displacement);

        compute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        compute.SetInt("vertexCount", mesh.vertexBuffer.count);
        compute.SetBuffer(ki, "vertices", mesh.vertexBuffer);

        compute.SetBuffer(ki, "mask", mask);

        compute.Dispatch(ki, Mathf.CeilToInt(
            mesh.vertexCount / Numthreads_Small), 1, 1);
    }

    public static void Compute_ResetVertexData(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement)
    {
        ComputeShader compute = genericCompute;

        int ki = compute.FindKernel(computeShader_ResetVerticesKernel);

        compute.SetInt("dispStride", displacement.stride);
        compute.SetInt("dispSize", displacement.count);
        compute.SetBuffer(ki, "displacement", displacement);

        compute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        compute.SetInt("vertexCount", mesh.vertexBuffer.count);
        compute.SetBuffer(ki, "vertices", mesh.vertexBuffer);

        compute.SetBuffer(ki, "mask", mask);

        compute.Dispatch(ki, Mathf.CeilToInt(
            mesh.vertexCount / Numthreads_Small), 1, 1);
    }

    public static void Compute_ResetVertexData(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement, ComputeBuffer colors)
    {
        ComputeShader compute = genericCompute;

        int ki = compute.FindKernel(computeShader_ResetVerticesAndColorKernel);

        compute.SetInt("dispStride", displacement.stride);
        compute.SetInt("dispSize", displacement.count);
        compute.SetBuffer(ki, "displacement", displacement);

        compute.SetInt("colorsStride", colors.stride);
        compute.SetInt("colorsSize", colors.count);
        compute.SetBuffer(ki, "colors", colors);

        compute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        compute.SetInt("vertexCount", mesh.vertexBuffer.count);
        compute.SetBuffer(ki, "vertices", mesh.vertexBuffer);

        compute.SetBuffer(ki, "mask", mask);

        compute.Dispatch(ki, Mathf.CeilToInt(
            mesh.vertexCount / Numthreads_Small), 1, 1);
    }

    public static void Compute_RecordVertices(ComputableMesh mesh, ComputeBuffer recorded,
        Matrix4x4 transform)
    {
        ComputeShader compute = genericCompute;

        compute.SetMatrix("modelMatrix", transform);

        int ki = compute.FindKernel(computeShader_RecordVerticesKernel);

        compute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        compute.SetInt("vertexCount", mesh.vertexBuffer.count);
        compute.SetBuffer(ki, "vertices", mesh.vertexBuffer);

        compute.SetInt("dispStride", recorded.stride);
        compute.SetInt("dispSize", recorded.count);
        compute.SetBuffer(ki, "displacement", recorded);

        compute.Dispatch(ki, Mathf.CeilToInt(
            recorded.count / Numthreads_Small), 1, 1);
    }
    #endregion

    #region Addition
    void AddVertices<T>(T vertices) where T : IEnumerable<VertexData>
    {
        VertexData[] v = vertices.ToArray();
        NativeArray<VertexData> newVertexData = new NativeArray<VertexData>(
            vertexCount + v.Length, Allocator.Persistent);
        for (int i = 0; i < vertexCount; i++)
            newVertexData[i] = vertexData[i];
        for (int i = 0; i < v.Length; i++)
            newVertexData[vertexCount + i] = v[i];
        vertexData.Dispose();
        vertexData = newVertexData;

        UpdateVertexData();
    }

    void AddIndices<T>(T indices, int submesh = 0) where T : IEnumerable<uint>
    {
        uint[] t = indices.ToArray();
        int indexCount = (int)mesh.GetIndexCount(submesh);
        NativeArray<uint> newIndexData = new NativeArray<uint>(
            indexCount + t.Length, Allocator.Persistent);
        for (int i = 0; i < indexCount; i++)
            newIndexData[i] = triangleData[submesh][i];
        for (int i = 0; i < t.Length; i++)
            newIndexData[indexCount + i] = t[i];
        triangleData[submesh].Dispose();
        triangleData[submesh] = newIndexData;

        UpdateTrianglesData();
    }
    #endregion

    #region Automatic modularity
    static ComputableFilterCollection filtersProcessor;
    static Dictionary<Component, UnityAction> startActions;
    static Dictionary<Component, UnityAction> finishActions;
    static Dictionary<Mesh, UnityAction> meshFinishActions;

    public static ComputableMesh[] Get(Component comp, string nameSufix = "_Computable")
    {
        return Get(comp, false, nameSufix);
    }

    public static ComputableMesh[] Get(Component comp, bool reinitialize, string nameSufix = "_Computable")
    {
        if (comp == null)
            return null;

        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        if (filter.isNull)
            return null;

        if (filter.isMeshFilter) return new ComputableMesh[] { Get(filter.filter, comp) };
        else return filter.customRenderer.enabled ?
                filter.customRenderer.GetComputables(comp) : new ComputableMesh[0];
    }

    public static void StopUsing(Component comp)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        if (!filter.isNull)
        {
            if (filter.isMeshFilter)
            {
                StopUsing(filter.filter, comp);
                if (startActions.NotNullContainsKey(comp))
                    startActions.Remove(comp);
                if (finishActions.NotNullContainsKey(comp))
                    finishActions.Remove(comp);
            }
            else
            {
                if (startActions.NotNullContainsKey(comp))
                {
                    filter.customRenderer.startRendering.RemoveListener(startActions[comp]);
                    startActions.Remove(comp);
                }
                if (finishActions.NotNullContainsKey(comp))
                {
                    filter.customRenderer.finishedRendering.RemoveListener(finishActions[comp]);
                    finishActions.Remove(comp);
                }
                filter.customRenderer.StopUseByComponent(comp);
            }
        }
    }

    public static Matrix4x4 LocalToWorldMatrix(Component comp, int id)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);
        return filter.LocalToWorldMatrix(id);
    }

    public static Matrix4x4 WorldToLocalMatrix(Component comp, int id)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);
        return filter.WorldToLocalMatrix(id);
    }

    public static bool IsRenderingAgentEnabled(Component comp)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        if (filter.isNull)
            return false;

        if (filter.isMeshFilter) return true;
        else return filter.customRenderer.enabled;
    }

    public static bool IsRenderingAgentFilter(Component comp)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        return filter.isMeshFilter;
    }

    public static bool FilterMeshChanged(Component comp)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        if (filter.isNull)
            return true;

        if (filter.isMeshFilter)
            return filtersProcessor.MeshChanged(filter.filter);

        return false;
    }

    public static void SetRenderingEvent_Start(Component comp, UnityAction method)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);
        startActions = startActions.CreateAdd(comp, method);
        if (!filter.isMeshFilter)
        {
            filter.customRenderer.startRendering.RemoveListener(method);
            filter.customRenderer.startRendering.AddListener(method);
        }
    }

    public static void SetRenderingEvent_Finished(Component comp, UnityAction method)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);
        finishActions = finishActions.CreateAdd(comp, method);
        if (!filter.isMeshFilter)
        {
            filter.customRenderer.finishedRendering.RemoveListener(method);
            filter.customRenderer.finishedRendering.AddListener(method);
        }
    }

    public static void PrepareStartRendering(Component comp)
    {
        ComputableFilter filter = ComputableFilter.Get(comp.gameObject);

        if (filter.isMeshFilter)
        {
            Mesh m = filtersProcessor.OriginalMesh(filter.filter);
            if (m != null)
            {
                if (meshFinishActions.NotNullContainsKey(m))
                {
                    meshFinishActions[m].Invoke();
                    meshFinishActions.Remove(m);
                }
                meshFinishActions = meshFinishActions.CreateAdd(m, finishActions[comp]);
            }
        }
    }

    //By MeshFilter
    static ComputableMesh Get(MeshFilter filter, Component comp, string nameSufix = "_Computable")
    {
        return Get(filter, comp, false, nameSufix);
    }

    static ComputableMesh Get(MeshFilter filter, Component comp, bool reinitialize, string nameSufix = "_Computable")
    {
        if (!filtersProcessor.isInitialized)
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
        }
        else filtersProcessor.CleanNullValues();

        bool validMeshExists = false;
        if (filtersProcessor.MeshChanged(filter))
            filtersProcessor.SmartRemove(filter);
        else validMeshExists = true;

        ComputableMesh mesh;
        if (validMeshExists)
        {
            mesh = filtersProcessor.GetComputable(filter);

            if (reinitialize)
                mesh.Initialize(filter.sharedMesh, filter.sharedMesh.name + nameSufix);
            else mesh.mesh.name = filter.sharedMesh.name + nameSufix;

            filtersProcessor.SetUseByComponent(filter, comp);

            return mesh;
        }
        else
        {
            if (filter.sharedMesh != null)
            {
                return filtersProcessor.Create(filter, comp,
                    filter.sharedMesh, filter.sharedMesh.name + nameSufix);
            }
            else return null;
        }
    }

    static void StopUsing(MeshFilter filter, Component comp)
    {
        filtersProcessor.StopUsing(filter, comp);
    }

    static void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        filtersProcessor.SubstituteFilterMeshes();
    }

    static void EndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        filtersProcessor.RestoreFilterMeshes();
        if (!meshFinishActions.IsNullOrEmpty())
        {
            filtersProcessor.CleanNullValues();
            foreach (KeyValuePair<Mesh, UnityAction> pair in meshFinishActions)
                pair.Value.Invoke();
            meshFinishActions.SmartClear();
        }
    }

    struct ComputableFilterCollection
    {
        Dictionary<Mesh, ComputableMesh> meshes;
        Dictionary<MeshFilter, List<Component>> usedBy;
        Dictionary<MeshFilter, Mesh> originals;
        public bool isInitialized;

        public bool MeshChanged(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return true;

            if (filter.sharedMesh != originals[filter])
                return true;

            return false;
        }

        public void SmartRemove(MeshFilter filter)
        {
            if (originals.NotNullContainsKey(filter))
            {
                Mesh original = originals[filter];
                originals.SmartRemove(filter);
                usedBy.SmartRemove(filter);
                if (!originals.Values.Contains(original))
                    meshes.SmartRemove(original);
            }
        }

        public void CleanNullValues()
        {
            if (!originals.IsNullOrEmpty())
            {
                //Register meshes that are linked to null filters
                List<Mesh> hanging = new List<Mesh>();
                foreach (KeyValuePair<MeshFilter, Mesh> pair in originals)
                    if ((pair.Key == null) && (pair.Value != null))
                        hanging.Add(pair.Value);

                //Remove null filters
                originals.ClearNulls();
                usedBy.ClearNulls();

                //Remove filters with null meshes
                List<MeshFilter> empty = new List<MeshFilter>();
                foreach (KeyValuePair<MeshFilter, Mesh> pair in originals)
                    if (pair.Value == null)
                        empty.Add(pair.Key);
                foreach (MeshFilter filter in empty)
                    originals.Remove(filter);

                //Check hanging meshes
                foreach (Mesh mesh in hanging)
                    if (!originals.Values.Contains(mesh))
                    {
                        meshes[mesh].SetNull();
                        meshes.Remove(mesh);
                    }

                //Nullify computables from null meshes
                foreach (KeyValuePair<Mesh, ComputableMesh> pair in meshes)
                    if ((pair.Key == null) && (pair.Value != null))
                        pair.Value.SetNull();

                //Remove null meshes
                meshes.ClearNulls();

                //Remove meshes with null computables
                List<Mesh> toRemove = new List<Mesh>();
                foreach (KeyValuePair<Mesh, ComputableMesh> pair in meshes)
                    if ((pair.Value == null) || (pair.Value.mesh == null))
                        toRemove.Add(pair.Key);
                foreach (Mesh mesh in toRemove)
                    meshes.Remove(mesh);
            }
        }

        public ComputableMesh GetComputable(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return null;

            if (!meshes.NotNullContainsKey(filter.sharedMesh))
                return null;

            return meshes[filter.sharedMesh];
        }

        public void SetUseByComponent(MeshFilter filter, Component comp)
        {
            usedBy = usedBy.CreateAdd(filter, comp);
        }

        public ComputableMesh Create(MeshFilter filter, Component comp, string name, int vCount, int tCount)
        {
            ComputableMesh mesh = new ComputableMesh(name, vCount, tCount);
            Create(filter, comp, mesh);
            return mesh;
        }

        public ComputableMesh Create(MeshFilter filter, Component comp, Mesh mesh, string name)
        {
            ComputableMesh computable = new ComputableMesh(mesh, name);
            Create(filter, comp, computable);
            return computable;
        }

        public void StopUsing(MeshFilter filter, Component comp)
        {
            if (usedBy.NotNullContainsKey(filter))
            {
                List<Component> list = usedBy[filter];
                list.SmartRemove(comp);
                if (list.Count <= 0)
                    SmartRemove(filter);
            }
        }

        public void ResetCompletely()
        {
            MeshFilter[] filters = originals.Keys.ToArray();
            foreach (MeshFilter filter in filters)
                for (int i = usedBy[filter].Count - 1; i >= 0; i--)
                    StopUsing(filter, usedBy[filter][i]);
        }

        public void SubstituteFilterMeshes()
        {
            MeshFilter[] filters = originals.Keys.ToArray();

            foreach (MeshFilter filter in filters)
                if (filter != null)
                    filter.sharedMesh = meshes[originals[filter]];
        }

        public void RestoreFilterMeshes()
        {
            MeshFilter[] filters = originals.Keys.ToArray();

            foreach (MeshFilter filter in filters)
                if (filter != null)
                    filter.sharedMesh = meshes[originals[filter]].original;
        }

        public Mesh OriginalMesh(MeshFilter filter)
        {
            if (!originals.NotNullContainsKey(filter))
                return null;
            return originals[filter];
        }

        void Create(MeshFilter filter, Component comp, ComputableMesh mesh)
        {
            originals = originals.CreateAdd(filter, filter.sharedMesh);
            meshes = meshes.CreateAdd(filter.sharedMesh, mesh);
            SetUseByComponent(filter, comp);
            isInitialized = true;
        }
    }

    struct ComputableFilter : IEquatable<ComputableFilter>
    {
        static Dictionary<GameObject, ComputableFilter> filters = null;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public CustomRenderer customRenderer;
        public bool isMeshFilter { get { return (filter != null); } }
        public bool isNull { get { return (filter == null) && (customRenderer == null); } }
        public Matrix4x4 LocalToWorldMatrix(int id)
        {
            return isMeshFilter ? filter.transform.localToWorldMatrix :
                customRenderer.LocalToWorldMatrix(id);
        }
        public Matrix4x4 WorldToLocalMatrix(int id)
        {
            return isMeshFilter ? filter.transform.worldToLocalMatrix :
                customRenderer.WorldToLocalMatrix(id);
        }

        public ComputableFilter(MeshFilter filter)
        {
            this.filter = filter;
            renderer = filter.gameObject.GetComponent<MeshRenderer>();
            customRenderer = null;
        }

        public ComputableFilter(CustomRenderer customRenderer)
        {
            filter = null;
            renderer = null;
            this.customRenderer = customRenderer;
        }

        public ComputableFilter(GameObject gameObject)
        {
            filter = gameObject.GetComponent<MeshFilter>();
            renderer = null;
            customRenderer = null;
            if (filter == null)
                customRenderer = gameObject.GetComponent<CustomRenderer>();
            else
                renderer = filter.gameObject.GetComponent<MeshRenderer>();
        }

        public static ComputableFilter Get(GameObject gameObject)
        {
            ClearNulls();

            ComputableFilter filter;
            if (filters.NotNullContainsKey(gameObject))
                filter = filters[gameObject];
            else
            {
                filter = new ComputableFilter(gameObject);
                filters = filters.CreateAdd(gameObject, filter);
            }
            return filter;
        }

        static void ClearNulls()
        {
            if (filters != null)
            {
                filters = filters.ClearNulls();
                List<GameObject> gos = new List<GameObject>();
                foreach (KeyValuePair<GameObject, ComputableFilter> pair in filters)
                    if (pair.Value.isNull)
                        gos.Add(pair.Key);
                foreach (GameObject go in gos)
                    filters.Remove(go);
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is ComputableFilter)) return false;
            return Equals((ComputableFilter)other);
        }

        public bool Equals(ComputableFilter other)
        {
            return (filter == other.filter)
                && (customRenderer == other.customRenderer);
        }

        public override int GetHashCode()
        {
            return isMeshFilter ? filter.GetHashCode() : customRenderer.GetHashCode();
        }

        public static bool operator ==(ComputableFilter o1, ComputableFilter o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(ComputableFilter o1, ComputableFilter o2)
        {
            return !o1.Equals(o2);
        }
    }
    #endregion
}
