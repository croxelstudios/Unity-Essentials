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
                ComputableMesh.Compute_ResetDisplacement(displacement[i]);
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
}
