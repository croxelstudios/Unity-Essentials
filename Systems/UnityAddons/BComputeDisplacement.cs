using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class BComputeDisplacement : MonoBehaviour
{
    [OnValueChanged("NullMask")]
    [SerializeField]
    bool specificSubmesh = false;
    [ShowIf("@specificSubmesh")]
    [OnValueChanged("NullMask")]
    [SerializeField]
    uint submesh = 0;

    bool rendering;
    protected ComputableMesh[] meshes;
    protected ComputeBuffer[] mask;
    protected ComputeBuffer[] displacement;

    void OnEnable()
    {
        ComputableMesh.SetRenderingEvent_Start(this, BeginRendering);
        ComputableMesh.SetRenderingEvent_Finished(this, FinishRendering);
    }

    void OnDisable()
    {
        ResetMeshesData();
        ComputableMesh.StopUsing(this);
    }

    void LateUpdate()
    {
        //TO DO: [Optimized unified buffer version] Can I call these methods on OnEnable?
        meshes = ComputableMesh.Get(this);
    }

    void OnWillRenderObject()
    {
        //Checks if the rendering agent is being rendered and is a filter.
        //If the agent is a CustomRenderer, the rendering moment is handled by itself.
        if ((!rendering) && ComputableMesh.IsRenderingAgentAFilter(this))
            BeginRendering();
    }

    //TO DO: [Optimized unified buffer version]
    //This method will just call the apply displacement compute by default and replace mesh data
    //with the unified buffer data.
    //Can also call a virtual method to allow the appliance of custom behaviours to the mesh
    //such as the wind colors.
    void BeginRendering()
    {
        rendering = true;

        //TO DO: [Optimized unified buffer version]
        //These will be called only once per frame in the "OptimizedLateUpdate"
        //function and be unified buffers.
        mask = UpdateBufferArray(mask);
        displacement = UpdateBufferArray(displacement);

        OnUpdate();

        //TO DO: [Optimized unified buffer version]
        //These will be called only once per frame in the "OptimizedLateUpdate"
        //function and be unified buffers.
        //Computes called per mesh on OnUpdateMesh() should also work on unified buffers.
        for (int i = 0; i < meshes.Length; i++)
        {
            //Gets a mask indicating which vertices are affected according to submesh.
            if ((mask[i] == null) || (mask[i].count != meshes[i].vertexCount))
                mask[i] = meshes[i].SubmeshVertexMask(specificSubmesh ? (int)submesh : -1);

            //Initializes displacement buffer.
            if ((displacement[i] == null) || (displacement[i].count != meshes[i].vertexCount))
            {
                displacement[i] = new ComputeBuffer(meshes[i].vertexCount, sizeof(float) * 3);
                Compute_ResetDisplacement(displacement[i]);
            }

            OnUpdateMesh(i);
        }

        if ((meshes != null) && (mask != null))
            for (int i = 0; i < meshes.Length; i++)
                OnBeginRenderingMesh(i);
    }

    void FinishRendering()
    {
        if (rendering && (mask != null))
        {
            rendering = false;
            ResetMeshesData();
        }
    }

    void ResetMeshesData()
    {
        if (!meshes.IsNullOrEmpty())
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].mesh == null)
                    meshes[i] = meshes[i].Destroy();
                if ((meshes[i] != null) &&
                    ((!mask.IsNullOrEmpty()) && (mask[i] != null)) &&
                    ((!displacement.IsNullOrEmpty()) && (displacement[i] != null)))
                    ResetData(i);
            }
    }

    protected void NullMask()
    {
        mask = null;
    }

    protected ComputeBuffer[] UpdateBufferArray(ComputeBuffer[] buffer)
    {
        if ((buffer == null) || (meshes.Length != buffer.Length))
            buffer = buffer.Resize(meshes.Length);
        return buffer;
    }

    protected float SizeBasedScaling(Mesh mesh, float offset)
    {
        Bounds b = mesh.bounds;
        float topHeight = b.center.y + b.extents.y - offset;
        float bottomHeight = b.size.y - topHeight;
        float max = Mathf.Max(topHeight, bottomHeight);
        if (max == 0f) max = 1f;
        return 1 / max;
    }

    protected virtual void ResetData(int i)
    {

    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnUpdateMesh(int i)
    {

    }

    protected virtual void OnBeginRenderingMesh(int i)
    {

    }

    //--------------------------------------------------------------

    static ComputeShader _displacementCompute;
    public static ComputeShader displacementCompute
    {
        get
        {
            if (_displacementCompute == null)
                _displacementCompute = (ComputeShader)Resources.Load(displacementComputeShaderName);
            return _displacementCompute;
        }
    }
    const string displacementComputeShaderName = "VertexDisplacementGenericCompute";

    const string computeShader_ApplyDisplacementKernel = "ApplyDisplacement";
    const string computeShader_ResetDisplacementKernel = "ResetDisplacement";
    const string computeShader_ResetColorsKernel = "ResetColors";
    const string computeShader_ResetVerticesKernel = "ResetVertices";
    const string computeShader_ResetVerticesAndColorKernel = "ResetVerticesAndColor";
    const string computeShader_RecordVerticesKernel = "RecordVertices";
    const float Numthreads_Large = 512;
    const float Numthreads_Small = 128;
    const float Numthreads_2D = 16;

    protected static void Compute_ResetDisplacement(ComputeBuffer displacement)
    {
        ComputeShader compute = displacementCompute;
        int ki = compute.FindKernel(computeShader_ResetDisplacementKernel);

        compute.SetInt("dispStride", displacement.stride);
        compute.SetInt("dispSize", displacement.count);
        compute.SetBuffer(ki, "displacement", displacement);

        compute.Dispatch(ki, Mathf.CeilToInt(
            displacement.count / Numthreads_Small), 1, 1);
    }

    protected static void Compute_ResetColors(ComputeBuffer colors)
    {
        ComputeShader compute = displacementCompute;
        int ki = compute.FindKernel(computeShader_ResetColorsKernel);

        compute.SetInt("colorsStride", colors.stride);
        compute.SetInt("colorsSize", colors.count);
        compute.SetBuffer(ki, "colors", colors);

        compute.Dispatch(ki, Mathf.CeilToInt(
            colors.count / Numthreads_Small), 1, 1);
    }

    protected static void Compute_ApplyDisplacement(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement)
    {
        ComputeShader compute = displacementCompute;

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

    protected static void Compute_ResetVertexData(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement)
    {
        ComputeShader compute = displacementCompute;

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

    protected static void Compute_ResetVertexData(ComputableMesh mesh, ComputeBuffer mask,
        ComputeBuffer displacement, ComputeBuffer colors)
    {
        ComputeShader compute = displacementCompute;

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

    protected static void Compute_RecordVertices(ComputableMesh mesh, ComputeBuffer recorded,
        Matrix4x4 transform)
    {
        ComputeShader compute = displacementCompute;

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
}
