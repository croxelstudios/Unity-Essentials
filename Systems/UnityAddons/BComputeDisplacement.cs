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

    protected ComputableMesh[] meshes;
    protected ComputeBuffer[] mask;
    protected ComputeBuffer[] displacement;

    void OnDisable()
    {
        ResetMeshesData();
        ComputableMesh.StopUsing(this);
    }

    void LateUpdate()
    {
        meshes = ComputableMesh.Get(this);
        ComputableMesh.SetRenderingEvent_Start(this, BeginRendering);
        ComputableMesh.SetRenderingEvent_Finished(this, FinishRendering);

        mask = UpdateBufferArray(mask);
        displacement = UpdateBufferArray(displacement);

        OnUpdate();

        for (int i = 0; i < meshes.Length; i++)
        {
            if ((mask[i] == null) || (mask[i].count != meshes[i].vertexCount))
                mask[i] = meshes[i].SubmeshVertexMask(specificSubmesh ? (int)submesh : -1);

            if ((displacement[i] == null) || (displacement[i].count != meshes[i].vertexCount))
            {
                displacement[i] = new ComputeBuffer(meshes[i].vertexCount, sizeof(float) * 3);
                ComputableMesh.Compute_ResetDisplacement(displacement[i]);
            }

            OnUpdateMesh(i);
        }
    }

    void BeginRendering()
    {
        if ((meshes != null) && (mask != null))
            for (int i = 0; i < meshes.Length; i++)
                OnBeginRenderingMesh(i);
    }

    void FinishRendering()
    {
        if (mask != null)
            ResetMeshesData();
    }

    void ResetMeshesData()
    {
        if (meshes != null)
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].mesh == null)
                    meshes[i] = meshes[i].Destroy();
                if (meshes[i] != null)
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

    bool rendering;

    void OnWillRenderObject()
    {
        if ((!rendering) && ComputableMesh.IsRenderingAgentFilter(this))
        {
            rendering = true;
            ComputableMesh.PrepareStartRendering(this);
            BeginRendering();
        }
    }

    void OnRenderObject()
    {
        if (rendering)
            rendering = false;
    }
}
