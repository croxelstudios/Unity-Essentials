using UnityEngine;

public static class ComputableMeshExtension_GetEdges
{
    static ComputeShader _edgesCompute;
    public static ComputeShader edgesCompute
    {
        get
        {
            if (_edgesCompute == null)
                _edgesCompute = (ComputeShader)Resources.Load(edgesComputeShaderName);
            return _edgesCompute;
        }
    }
    const string edgesComputeShaderName = "ComputableMeshEdgesCompute";
    const string getEdgesKernel = "GetEdges";
    const string proccessEdgesKernel = "ProccessDuplicateEdges";

    static void Compute_GetEdges(ComputableMesh mesh, ComputeBuffer edgesDataBuff, int submesh)
    {
        int indexCount = (int)mesh.GetIndexCount(submesh);

        int ki = edgesCompute.FindKernel(getEdgesKernel);
        edgesCompute.SetInt("indexStart", (int)mesh.GetIndexStart(submesh));
        edgesCompute.SetInt("indexCount", indexCount);
        edgesCompute.SetInt("indexStride", mesh.indexBuffer.stride);
        edgesCompute.SetBuffer(ki, "indices", mesh.indexBuffer);
        edgesCompute.SetBuffer(ki, "edges", edgesDataBuff);

        edgesCompute.Dispatch(ki, Mathf.CeilToInt(
            (indexCount / 3f) / Computables.Numthreads_Small), 1, 1);
    }

    static void Compute_ProccessEdges(ComputeBuffer edgesDataBuff)
    {
        int ki = edgesCompute.FindKernel(proccessEdgesKernel);
        edgesCompute.SetBuffer(ki, "edges", edgesDataBuff);

        int maxElementsPerDispatch = (int)Computables.Numthreads_2D * Computables.MaxGroups;

        for (int start = 0; start < edgesDataBuff.count; start += maxElementsPerDispatch)
        {
            int count = Mathf.Min(
                maxElementsPerDispatch,
                edgesDataBuff.count - start
            );

            edgesCompute.SetInt("indexStart", start);
            edgesCompute.SetInt("indexCount", count);

            int threadGroups = Mathf.CeilToInt(count / Computables.Numthreads_2D);

            edgesCompute.Dispatch(ki, threadGroups, threadGroups, 1);
        }
    }

    public static ComputeBuffer GetEdges(this ComputableMesh mesh, int submesh = -1)
    {
        ComputeBuffer edgesBuff = new((int)mesh.GetIndexCount(submesh), CompMesh_Edge.Size());
        Compute_GetEdges(mesh, edgesBuff, submesh);
        Compute_ProccessEdges(edgesBuff);
        return edgesBuff;
    }
}

struct CompMesh_Edge
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
