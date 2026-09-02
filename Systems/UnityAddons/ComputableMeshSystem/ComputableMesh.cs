using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public class ComputableBase<T> : Wrapper, IDisposable where T : Object
{
    public GraphicsBuffer vertexBuffer
    { get { return VertexBuffer(); } }
    public GraphicsBuffer indexBuffer
    { get { return IndexBuffer(); } }

    public ComputableBase()
    {
    }

    public ComputableBase(T source, string name)
    {
    }

    public virtual void Initialize(T source, string name)
    {

    }

    public virtual GraphicsBuffer VertexBuffer()
    {
        return null;
    }

    public virtual GraphicsBuffer IndexBuffer()
    {
        return null;
    }

    public virtual void BakeToCPU()
    {
    }

    public virtual T GetValue()
    {
        return null;
    }

    public virtual T GetOriginal()
    {
        return null;
    }

    public virtual void Dispose()
    {
    }
}

public class ComputableMesh : ComputableBase<Mesh>
{
    public Mesh original { get; private set; }
    public Mesh mesh { get; private set; }

    public CBuffersCollection auxBuffers;
    NativeArray<VertexData> vertexData;
    NativeArray<uint>[] triangleData;
    GraphicsBuffer vertexBuf;
    GraphicsBuffer indexBuf;

    Vector3[] tmpParticleVert;
    Vector3[] tmpParticleNorm;

    //public static implicit operator Mesh(ComputableMesh m) => m.mesh;

    public struct VertexData //TO DO: Posibility of using less data? ->
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
    const string genericComputeShaderName = "ComputableMeshGenericCompute";
    const string cleanNullAreaTrianglesKernel = "CleanNullAreaTriangles";
    const string clearMaskKernel = "ClearMask";
    const string fillMaskKernel = "FillMask";
    const string getSubmeshMaskKernel = "GetSubmeshMask";

    public override GraphicsBuffer VertexBuffer()
    {
        vertexBuf ??= mesh.GetVertexBuffer(0);
        return vertexBuf;
    }

    public override GraphicsBuffer IndexBuffer()
    {
        indexBuf ??= mesh.GetIndexBuffer();
        return indexBuf;
    }

    public override void BakeToCPU()
    {
        BakeVertexDataToCPU();
        BakeIndexDataToCPU();
    }

    public uint GetIndexStart(int submesh)
    {
        return (submesh < 0) ? 0 : mesh.GetIndexStart(submesh);
    }

    public uint GetIndexCount(int submesh)
    {
        return (submesh < 0) ? (uint)indexCount : mesh.GetIndexCount(submesh);
    }

    #region Initialize
    public ComputableMesh(Mesh meshToCopy, string name) : base(meshToCopy, name)
    {
        Initialize(meshToCopy, name);
    }

    public ComputableMesh(string name, int vCount, int tCount)
    {
        Initialize(name, vCount, tCount);
    }

    public ComputableMesh(NativeArray<VertexData> vertexData, NativeArray<uint>[] triangleData, string name)
    {
        Initialize(vertexData, triangleData, name);
    }

    public void Initialize(string name, int vCount, params int[] tCount)
    {
        //Vertex data setup
        vertexData = new NativeArray<VertexData>(vCount, Allocator.Persistent);

        //Triangles
        triangleData = new NativeArray<uint>[tCount.Length];
        for (int i = 0; i < tCount.Length; i++)
            triangleData[i] = new NativeArray<uint>(tCount[i], Allocator.Persistent);

        Initialize(vertexData, triangleData, name);
    }

    public override void Initialize(Mesh meshToCopy, string name = "")
    {
        GetDataFromCopy(meshToCopy, name);
        original = meshToCopy;
    }

    public void InitializeFromOriginal()
    {
        Initialize(original);
    }

    public void GetDataFromCopy(Mesh meshToCopy, string name = "")
    {
        int[] tCount = new int[meshToCopy.subMeshCount];
        for (int i = 0; i < tCount.Length; i++)
            tCount[i] = (int)meshToCopy.GetIndexCount(i);
        if (name.IsNullOrEmpty())
            name = this.name;
        Initialize(name, meshToCopy.vertexCount, tCount);
        for (int i = 0; i < meshToCopy.subMeshCount; i++)
            CopyMesh(meshToCopy, 0, 0, i, i);
    }

    public void Initialize(NativeArray<VertexData> vertexData, NativeArray<uint>[] triangleData, string name = "")
    {
        mesh = mesh.ClearOrCreate();

        mesh.name = name;
        mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

        //Vertex data setup
        SetMeshData(vertexData);

        //Triangles
        SetTriangles(triangleData);
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

    public void CopyPositionNormal(Mesh meshToCopy, int vOffset, Vector3 offset, Matrix4x4 mat)
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
        Vector3[] vertices = meshToCopy.vertices;
        Vector3[] normals = meshToCopy.normals;
        Vector4[] tangents = meshToCopy.tangents;
        Color[] colors = meshToCopy.colors;
        Vector2[] uv = meshToCopy.uv;
        for (uint i = 0; i < vCount; i++)
            VData_SetData((int)(vOffset + i), vertices[i],
                normals.Length > i ? normals[i] : Vector3.zero,
                tangents.Length > i ? tangents[i] : Vector3.zero,
                colors.Length > i ? colors[i] : Color.white,
                uv.Length > i ? uv[i] : Vector2.zero);
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

        ProcMesh.PositionArbitraryMesh(meshToCopy, ref tmpParticleVert, ref tmpParticleNorm, offset, mat);

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
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
        mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Length, 0, MeshUpdateFlags.DontRecalculateBounds);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        vertexBuf = null;
    }

    public void SetTriangles(NativeArray<uint>[] triangles)
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

    public string name
    {
        get { return mesh.name; }
        set { mesh.name = value; }
    }

    public Bounds bounds { get { return mesh.bounds; } set { mesh.bounds = value; } }

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
        vertexBuf = vertexBuf.ReleaseToNull();
    }

    public void ResetIndexBuffer()
    {
        indexBuf = indexBuf.ReleaseToNull();
    }

    public void ResetVertexColors()
    {
        SetVertexColors(0, vertexCount, Color.white);
    }

    #region Utilities
    public void CleanNullAreaTriangles(float minArea)
    {
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

        genericCompute.SetFloat("minArea", minArea);

        int ki = genericCompute.FindKernel(cleanNullAreaTrianglesKernel);
        genericCompute.SetInt("vertexStride", vertexBuffer.stride);
        genericCompute.SetInt("vertexCount", vertexBuffer.count);
        genericCompute.SetBuffer(ki, "vertices", vertexBuffer);
        genericCompute.SetInt("indexStart", indexStart);
        genericCompute.SetInt("indexCount", indexCount);
        genericCompute.SetInt("indexStride", indexBuffer.stride);
        genericCompute.SetBuffer(ki, "indices", indexBuffer);
        genericCompute.SetBuffer(ki, "toClean", toClean);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    public void BakeVertexDataToCPU()
    {
        VertexData[] vertices = new VertexData[vertexCount];
        vertexBuffer.GetData(vertices);
        if (!vertexData.IsCreated)
            vertexData = new NativeArray<VertexData>(vertexCount, Allocator.Persistent);
        for (int i = 0; i < vertices.Length; i++)
            vertexData[i] = vertices[i];
        UpdateVertexData();
    }

    public void BakeIndexDataToCPU()
    {
        uint[] indices = new uint[totalIndexCount];
        indexBuffer.GetData(indices);
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

    public void SetNull()
    {
        mesh = null;
        original = null;
    }

    /// <summary>
    /// Generates a compute buffer of booleans indicating which vertices belong to the specified submesh.
    /// Negative submesh index will generate a mask including all vertices.
    /// </summary>
    /// <param name="submesh"></param>
    /// <returns></returns>
    public ComputeBuffer SubmeshVertexMask(int submesh)
    {
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
        int ki = genericCompute.FindKernel(clearMaskKernel);
        genericCompute.SetBuffer(ki, "mask", mask);
        genericCompute.SetInt("vertexCount", mask.count);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    void Compute_FillMask(ComputeBuffer mask)
    {
        int ki = genericCompute.FindKernel(fillMaskKernel);
        genericCompute.SetBuffer(ki, "mask", mask);
        genericCompute.SetInt("vertexCount", mask.count);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    void Compute_SubmeshVertexMask(ComputeBuffer mask, int submesh)
    {
        int indexStart = (int)mesh.GetIndexStart(submesh);
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = genericCompute.FindKernel(getSubmeshMaskKernel);
        genericCompute.SetInt("indexStart", indexStart);
        genericCompute.SetInt("indexCount", indexCount);
        genericCompute.SetInt("indexStride", indexBuffer.stride);
        genericCompute.SetBuffer(ki, "indices", indexBuffer);
        genericCompute.SetBuffer(ki, "mask", mask);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            indexCount / Computables.Numthreads_Small), 1, 1);
    }
    #endregion

    #region Addition
    public void AddVertices<T>(T vertices) where T : IEnumerable<VertexData>
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

    public void AddIndices<T>(T indices, int submesh = 0) where T : IEnumerable<uint>
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

    public void ReplaceIndex(int place, uint newIndex, int submesh = 0)
    {
        triangleData[submesh][place] = newIndex;
    }
    #endregion

    public override Mesh GetValue()
    {
        return mesh;
    }

    public override Mesh GetOriginal()
    {
        return original;
    }

    protected override bool IsNull()
    {
        return mesh == null;
    }

    public override void Dispose()
    {
        auxBuffers.Release();
        if (vertexData.IsCreated)
            vertexData.Dispose();
        if (!triangleData.IsNullOrEmpty())
            for (int i = 0; i < triangleData.Length; i++)
                if (triangleData[i].IsCreated)
                    triangleData[i].Dispose();
        triangleData = null;
        ResetVertexBuffer();
        ResetIndexBuffer();
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Object.DestroyImmediate(mesh);
        else
#endif
            Object.Destroy(mesh);
        GC.SuppressFinalize(this);
    }

    public ComputableMesh Destroy()
    {
        Dispose();
        return null;
    }

    ~ComputableMesh()
    {
        //Debug.LogWarning("ComputableMesh was not disposed properly, calling Dispose() in finalizer.");
        Dispose();
    }
}

public class ComputableSprite : ComputableBase<Sprite>
{
    const string MESHSUFFIX = "_m";

    Sprite sprite;
    public ComputableMesh mesh;
    public Sprite original;
    public Bounds bounds { get { return mesh.bounds; } }
    public int indexCount { get { return (int)mesh.indexCount; } }
    public int totalIndexCount { get { return (int)mesh.totalIndexCount; } }
    public int vertexCount { get { return mesh.vertexCount; } }
    public int subMeshCount { get { return mesh.subMeshCount; } }
    public Vector3[] vertices { get { return mesh.vertices; } }

    public static implicit operator Sprite(ComputableSprite s) => s.sprite;

    public GraphicsBuffer positions;
    public GraphicsBuffer normals;
    public GraphicsBuffer tangents;
    public GraphicsBuffer colors;
    public GraphicsBuffer uvs;

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
    const string genericComputeShaderName = "ComputableMeshGenericCompute";
    const string constructVertexDataKernel = "ConstructVertexDataBuffer";
    const string extractVertexDataKernel = "ExtractVertexDataBuffers";

    public string name
    {
        get { return sprite.name; }
        set
        {
            sprite.name = value;
            mesh.name = value + MESHSUFFIX;
        }
    }

    public ComputableSprite(Sprite spriteToCopy, string name) : base(spriteToCopy, name)
    {
        Initialize(spriteToCopy, name);
    }

    public override GraphicsBuffer VertexBuffer()
    {
        return mesh.vertexBuffer;
    }

    public override GraphicsBuffer IndexBuffer()
    {
        return mesh.indexBuffer;
    }

    public override void BakeToCPU()
    {
        mesh.BakeToCPU();
    }

    public override void Initialize(Sprite spriteToCopy, string name)
    {
        if (sprite == null)
            Object.DestroyImmediate(sprite);

        Rect rect = spriteToCopy.rect;
        Vector2 normalizedPivot = new Vector2(
            spriteToCopy.pivot.x / rect.width,
            spriteToCopy.pivot.y / rect.height
        );

        NativeArray<ComputableMesh.VertexData> vData = CPU_ConstructVertexData(spriteToCopy);
        NativeArray<uint>[] iData = CPU_ConstructIndexData(spriteToCopy);

        if (mesh != null)
            mesh.Initialize(vData, iData, name + MESHSUFFIX);
        else mesh = new ComputableMesh(vData, iData, name + MESHSUFFIX);

        vData.Dispose();
        iData[0].Dispose();

        sprite = Sprite.Create(spriteToCopy.texture, rect, normalizedPivot,
                                      spriteToCopy.pixelsPerUnit, 0, SpriteMeshType.Tight, spriteToCopy.border);
        InitializeBuffers();
        SetMeshDataOnSprite();

        original = spriteToCopy;
    }

    void InitializeBuffers()
    {
        positions = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 3);
        normals = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 3);
        tangents = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 4);
        colors = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 4);
        uvs = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 2);
    }

    public Sprite GetSprite()
    {
        SetMeshDataOnSprite();
        return sprite;
    }

    void SetMeshDataOnSprite()
    {
        Compute_ExtractVertexData(positions, normals, tangents, colors, uvs);

        SetBufferOnSprite<Vector3>(sprite, positions, VertexAttribute.Position);
        SetBufferOnSprite<Vector3>(sprite, normals, VertexAttribute.Normal);
        SetBufferOnSprite<Vector4>(sprite, tangents, VertexAttribute.Tangent);
        SetBufferOnSprite<Color>(sprite, colors, VertexAttribute.Color);
        SetBufferOnSprite<Vector2>(sprite, uvs, VertexAttribute.TexCoord0);
    }

    void SetBufferOnSprite<T>(Sprite sprite, GraphicsBuffer buffer, VertexAttribute attribute) where T : struct
    {
        if (sprite.HasVertexAttribute(attribute))
        {
            NativeArray<T> array = new NativeArray<T>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            AsyncGPUReadback.RequestIntoNativeArray(ref array, buffer, (AsyncGPUReadbackRequest req) =>
            {
                if (req.hasError)
                {
                    Debug.LogError("Readback failed");
                    array.Dispose();
                    array = default;
                    return;
                }

                sprite.SetVertexAttribute(attribute, array);

                array.Dispose();
                array = default;
            });
        }
    }

    NativeArray<ComputableMesh.VertexData> CPU_ConstructVertexData(Sprite sprite)
    {
        int vCount = sprite.GetVertexCount();
        if (vCount > 0)
        {
            NativeArray<ComputableMesh.VertexData> vData =
                new NativeArray<ComputableMesh.VertexData>(vCount, Allocator.Temp);

            bool hasPosition = sprite.HasVertexAttribute(VertexAttribute.Position);
            bool hasNormal = sprite.HasVertexAttribute(VertexAttribute.Normal);
            bool hasTangent = sprite.HasVertexAttribute(VertexAttribute.Tangent);
            bool hasColor = sprite.HasVertexAttribute(VertexAttribute.Color);
            bool hasUV = sprite.HasVertexAttribute(VertexAttribute.TexCoord0);
            NativeSlice<Vector3> sprPositions = hasPosition ?
                sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position) :
                new NativeSlice<Vector3>();
            NativeSlice<Vector3> sprNormals = hasNormal ?
                sprite.GetVertexAttribute<Vector3>(VertexAttribute.Normal) :
                new NativeSlice<Vector3>();
            NativeSlice<Vector4> sprTangents = hasTangent ?
                sprite.GetVertexAttribute<Vector4>(VertexAttribute.Tangent) :
                new NativeSlice<Vector4>();
            NativeSlice<Color> sprColors = hasColor ?
                sprite.GetVertexAttribute<Color>(VertexAttribute.Color) :
                new NativeSlice<Color>();
            NativeSlice<Vector2> sprUVs = hasUV ?
                sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0) :
                new NativeSlice<Vector2>();

            for (int i = 0; i < vCount; i++)
            {
                ComputableMesh.VertexData data = new ComputableMesh.VertexData(
                    hasPosition ? sprPositions[i] : Vector3.zero,
                    hasNormal ? sprNormals[i] : Vector3.up,
                    hasTangent ? sprTangents[i] : new Vector4(1, 0, 0, 1),
                    hasColor ? sprColors[i] : Color.white,
                    hasUV ? sprUVs[i] : Vector2.zero
                    );

                vData[i] = data;
            }

            return vData;
        }
        else return new NativeArray<ComputableMesh.VertexData>(0, Allocator.Temp);
    }

    NativeArray<uint>[] CPU_ConstructIndexData(Sprite sprite)
    {
        int iCount = sprite.vertices.Length;
        NativeArray<uint>[] iData = new NativeArray<uint>[]
            { new NativeArray<uint>(iCount, Allocator.Temp) };
        NativeArray<ushort> indices = sprite.GetIndices();

        for (int i = 0; i < iCount; i++)
            iData[0][i] = indices[i];

        return iData;
    }

    protected void Compute_ConstructVertexData(
        GraphicsBuffer positions,
        GraphicsBuffer normals,
        GraphicsBuffer tangents,
        GraphicsBuffer colors,
        GraphicsBuffer uvs
        )
    {
        int vertexCount = mesh.vertexBuffer.count;
        int ki = genericCompute.FindKernel(constructVertexDataKernel);
        genericCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        genericCompute.SetInt("vertexCount", vertexCount);
        genericCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        genericCompute.SetBuffer(ki, "positions", positions);
        genericCompute.SetBuffer(ki, "normals", normals);
        genericCompute.SetBuffer(ki, "tangents", tangents);
        genericCompute.SetBuffer(ki, "colors", colors);
        genericCompute.SetBuffer(ki, "uvs", uvs);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    protected void Compute_ExtractVertexData(
        GraphicsBuffer positions,
        GraphicsBuffer normals,
        GraphicsBuffer tangents,
        GraphicsBuffer colors,
        GraphicsBuffer uvs
        )
    {
        int vertexCount = mesh.vertexBuffer.count;
        int ki = genericCompute.FindKernel(extractVertexDataKernel);
        genericCompute.SetInt("vertexStride", mesh.vertexBuffer.stride);
        genericCompute.SetInt("vertexCount", vertexCount);
        genericCompute.SetBuffer(ki, "vertices", mesh.vertexBuffer);
        genericCompute.SetBuffer(ki, "positions", positions);
        genericCompute.SetBuffer(ki, "normals", normals);
        genericCompute.SetBuffer(ki, "tangents", tangents);
        genericCompute.SetBuffer(ki, "colors", colors);
        genericCompute.SetBuffer(ki, "uvs", uvs);

        genericCompute.Dispatch(ki, Mathf.CeilToInt(
            vertexCount / Computables.Numthreads_Small), 1, 1);
    }

    public override Sprite GetValue()
    {
        return GetSprite();
    }

    public override Sprite GetOriginal()
    {
        return original;
    }

    protected override bool IsNull()
    {
        return sprite == null;
    }

    public override void Dispose()
    {
        mesh.Dispose();
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Object.DestroyImmediate(sprite);
        else
#endif
            Object.Destroy(sprite);
        GC.SuppressFinalize(this);
    }

    public ComputableSprite Destroy()
    {
        Dispose();
        return null;
    }

    ~ComputableSprite()
    {
        //Debug.LogWarning("ComputableMesh was not disposed properly, calling Dispose() in finalizer.");
        Dispose();
    }
}
