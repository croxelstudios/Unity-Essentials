using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using static UnityEngine.LightAnchor;

public class ComputableMesh
{
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
    NativeArray<uint> triangleData;
    GraphicsBuffer vertexBuf;
    GraphicsBuffer indexBuf;

    Vector3[] tmpParticleVert;
    Vector3[] tmpParticleNorm;

    struct VertexData //TO DO: Posibility of using less data? ->
                      //Would require different versions of compute shaders
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

    static ComputeShader genericCompute;
    const string genericComputeShaderName = "ComputableMeshGenericCompute";
    const string cleanNullAreaTrianglesKernel = "CleanNullAreaTriangles";

    #region Initialize
    public ComputableMesh(string name, int vCount, int tCount)
    {
        Initialize(name, vCount, tCount);
    }

    public ComputableMesh(Mesh meshToCopy, string name)
    {
        Initialize(meshToCopy, name);
    }

    public void Initialize(string name, int vCount, int tCount)
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
        triangleData = new NativeArray<uint>(tCount, Allocator.Persistent);
        SetTriangles(triangleData);
    }

    public void Initialize(Mesh meshToCopy, string name)
    {
        Initialize(name, meshToCopy.vertexCount, meshToCopy.triangles.Length);
        CopyMesh(meshToCopy, 0, 0);
    }
    #endregion

    #region Automatic methods
    public void CopyMesh(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        Prepare_CopyMesh(meshToCopy, vOffset, indexOffset);

        UpdateMeshData();
    }

    public void CopyTriangles(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        Prepare_CopyTriangles(meshToCopy, vOffset, indexOffset);

        UpdateTrianglesData();
    }

    public void SetIndex(int id, uint newTri)
    {
        Prepare_SetIndex(id, newTri);
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
    public void Prepare_CopyMesh(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        Prepare_CopyTriangles(meshToCopy, vOffset, indexOffset);

        Prepare_CopyAllVertexData(meshToCopy, vOffset, indexOffset);
    }

    public void Prepare_CopyTriangles(Mesh meshToCopy, int vOffset, int indexOffset)
    {
        int tCount = meshToCopy.triangles.Length;
        ProcMesh.RegisterArbitraryMesh(meshToCopy, vOffset, indexOffset, ref triangleData);
    }

    public void Prepare_SetIndex(int id, uint newTri)
    {
        triangleData[id] = newTri;
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

    void SetTriangles(NativeArray<uint> triangles)
    {
        int count = triangles.Length;

        bool changeSize = count != indexCount;
        if (changeSize)
            mesh.SetIndexBufferParams(count, IndexFormat.UInt32);

        MeshUpdateFlags updateFlags = MeshUpdateFlags.DontRecalculateBounds;
        if (count < indexCount) updateFlags |= MeshUpdateFlags.DontValidateIndices;
        mesh.SetIndexBufferData(triangles, 0, 0, count, updateFlags);

        //Submesh //TO DO: Support for more than one submesh?
        if (changeSize)
        {
            SubMeshDescriptor submesh = new SubMeshDescriptor(0, count);
            mesh.SetSubMesh(0, submesh);
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

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint, float minArea = -1f)
    {
        CutMesh(planeNormal, planePoint, out int[] side1, out int[] side2, out int[] extremes, minArea);
    }

    public void CutMesh(Vector3 planeNormal, Vector3 planePoint,
        out int[] side1, out int[] side2, out int[] extremes, float minArea = -1f)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);

        GetPlaneCutData(planeNormal, planePoint, out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);

        //
        //

        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(intersectionsBuff, cutsDataBuff, minArea);

        RebuildMeshFromCutData(intersectionsBuff, cutsDataBuff, out side1, out side2, out extremes);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint, Vector3 upDirection, float squareSize, float minArea = -1f)
    {
        CutMesh_Square(planeNormal, planePoint, upDirection, squareSize,
            out int[] side1, out int[] side2, out int[] extremes, minArea);
    }

    public void CutMesh_Square(Vector3 planeNormal, Vector3 planePoint,
        Vector3 upDirection, float squareSize, out int[] side1, out int[] side2,
        out int[] extremes, float minArea = -1f)
    {
        if (cuttingCompute == null)
            cuttingCompute = (ComputeShader)Resources.Load(cutMeshComputeShaderName);
        
        GetPlaneCutData(planeNormal, planePoint, out ComputeBuffer intersectionsBuff, out ComputeBuffer cutsDataBuff);
        
        //
        if (squareSize > 0f)
            Compute_GetTriangleCutDatas_SquareCut(intersectionsBuff, cutsDataBuff, upDirection, squareSize);
        //
        
        if (minArea > 0f)
            Compute_CleanNullAreaTriangles(intersectionsBuff, cutsDataBuff, minArea);

        RebuildMeshFromCutData(intersectionsBuff, cutsDataBuff, out side1, out side2, out extremes);
    }

    void GetPlaneCutData(Vector3 planeNormal, Vector3 planePoint, out ComputeBuffer intersectionsBuff, out ComputeBuffer triangleCutsDataBuff)
    {
        //Get edges
        ComputeBuffer edgesBuff = new ComputeBuffer(indexCount, Edge.Size());
        Compute_GetEdges(edgesBuff);
        Compute_ProccessEdges(edgesBuff);

        //Get intersections
        intersectionsBuff = new ComputeBuffer(indexCount, Intersection.Size());
        Compute_GetIntersections(edgesBuff, intersectionsBuff,
            planeNormal, planePoint);

        triangleCutsDataBuff = new ComputeBuffer(
            indexCount / 3, TriangleCutProperties.Size());
        Compute_GetTriangleCutDatas(intersectionsBuff, triangleCutsDataBuff);
    }

    void RebuildMeshFromCutData(ComputeBuffer intersectionsBuff, ComputeBuffer triangleCutsDataBuff,
        out int[] side1, out int[] side2, out int[] extremes)
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
                triangleData[ind] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                triangleData[ind + 1] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                triangleData[ind + 2] = indicesToAdd[i];
                indicesToAdd.RemoveAt(i);
                i -= 3;
            }
            else break;
        }

        //Add triangles
        AddIndices(indicesToAdd);
    }

    void Compute_GetEdges(ComputeBuffer edgesDataBuff)
    {
        int ki = cuttingCompute.FindKernel(getEdgesKernel);
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
        Vector3 planeNormal, Vector3 planePoint)
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
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff)
    {
        int ki = cuttingCompute.FindKernel(getTriangleCutDataKernel);
        cuttingCompute.SetInt("indexStride", indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    void Compute_GetTriangleCutDatas_SquareCut(
        ComputeBuffer intersectionsBuff, ComputeBuffer cutsDataBuff,
        Vector3 upDirection, float size)
    {
        cuttingCompute.SetVector("upDirection", upDirection);
        cuttingCompute.SetFloat("size", size);

        int ki = cuttingCompute.FindKernel(getTriangleCutDataSquareKernel);
        cuttingCompute.SetInt("indexStride", indexBuffer.stride);
        cuttingCompute.SetBuffer(ki, "indices", indexBuffer);
        cuttingCompute.SetBuffer(ki, "intersections", intersectionsBuff);
        cuttingCompute.SetBuffer(ki, "cutsData", cutsDataBuff);

        cuttingCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Numthreads_Small), 1, 1);
    }

    void Compute_CleanNullAreaTriangles(ComputeBuffer intersectionsBuff,
        ComputeBuffer cutsDataBuff, float minArea)
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
        if (genericCompute == null)
            genericCompute = (ComputeShader)Resources.Load(genericComputeShaderName);

        ComputeBuffer toClean = new ComputeBuffer(indexCount / 3, sizeof(uint));
        Compute_CleanNullAreaTriangles(toClean, minArea);

        uint[] nullTris = new uint[toClean.count];
        toClean.GetData(nullTris);

        int toRemove = 0;
        for (int i = 0; i < nullTris.Length; i++)
            if (nullTris[i] > 0) toRemove += 3;
        NativeArray<uint> newIndexData = new NativeArray<uint>(
            indexCount - toRemove, Allocator.Persistent);
        int n = 0;
        for (int i = 0; i < indexCount; i++)
        {
            int ind = i / 3;
            if (nullTris[ind] < 1)
            {
                newIndexData[n] = triangleData[i];
                n++;
            }
        }

        triangleData.Dispose();
        triangleData = newIndexData;
        UpdateMeshData();
    }

    void Compute_CleanNullAreaTriangles(ComputeBuffer toClean, float minArea)
    {
        genericCompute.SetFloat("minArea", minArea);

        int ki = genericCompute.FindKernel(cleanNullAreaTrianglesKernel);
        genericCompute.SetInt("vertexStride", vertexBuffer.stride);
        genericCompute.SetBuffer(ki, "vertices", vertexBuffer);
        genericCompute.SetInt("indexStride", indexBuffer.stride);
        genericCompute.SetBuffer(ki, "indices", indexBuffer);
        genericCompute.SetBuffer(ki, "toClean", toClean);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
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
        uint[] indices = new uint[indexCount];
        indexBuf.GetData(indices);
        for (int i = 0; i < vertices.Length; i++)
            triangleData[i] = indices[i];
        UpdateTrianglesData();
    }

    public void UpdateMeshDataToCPU()
    {
        UpdateVertexDataToCPU();
        UpdateIndexDataToCPU();
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

    void AddIndices<T>(T indices) where T : IEnumerable<uint>
    {
        uint[] t = indices.ToArray();
        NativeArray<uint> newIndexData = new NativeArray<uint>(
            indexCount + t.Length, Allocator.Persistent);
        for (int i = 0; i < indexCount; i++)
            newIndexData[i] = triangleData[i];
        for (int i = 0; i < t.Length; i++)
            newIndexData[indexCount + i] = t[i];
        triangleData.Dispose();
        triangleData = newIndexData;

        UpdateTrianglesData();
    }
    #endregion
}
