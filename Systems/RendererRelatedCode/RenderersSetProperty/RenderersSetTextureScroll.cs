using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetTextureScroll : BRenderersSetProperty
{
    [Range(-180f, 180f)]
    public float angle = 0f;
    [SerializeField]
    float _moveSpeed = 1f;
    public float moveSpeed
    {
        get { return _moveSpeed; }
        set { _moveSpeed = value; }
    }
    public Vector2 referenceOffset = Vector2.zero;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    Vector2 direction;
    Vector2 currentOffset;

    static Dictionary<RendMatProp, Vector4> originals;

    void Reset()
    {
        propertyName = "_MainTex";
    }

    void LateUpdate()
    {
        UpdateBehaviour();
    }

    protected override void Init()
    {
        originals = new Dictionary<RendMatProp, Vector4>();
        currentOffset = referenceOffset;
        base.Init();
    }

    protected override void UpdateBehaviour()
    {
        float deltaTime = timeMode.DeltaTime();

        float angleRad = Mathf.Deg2Rad * angle;
        direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * moveSpeed;

        currentOffset.x += deltaTime * direction.x;
        currentOffset.y += deltaTime * direction.y;
        //currentOffset.x %= 1f;
        //currentOffset.y %= 1f;
        base.UpdateBehaviour();
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        string fullPropertyName = propertyName + "_ST";
        Vector4 tilingOffset = rend.sharedMaterials[mat].GetVector(fullPropertyName);
        
        tilingOffset.z = currentOffset.x;
        tilingOffset.w = currentOffset.y;

        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, block.GetVector(propertyName));
        block.SetVector(fullPropertyName, tilingOffset);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat)) block.SetVector(propertyName, originals[rendMat]);
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        string fullPropertyName = propertyName + "_ST";
        Vector4 tilingOffset = rend.sharedMaterials[mat].GetVector(fullPropertyName);

        tilingOffset.z = currentOffset.x;
        tilingOffset.w = currentOffset.y;

        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, rend.materials[mat].GetVector(propertyName));
        rend.materials[mat].SetVector(fullPropertyName, tilingOffset);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat))
            rend.materials[mat].SetVector(propertyName, originals[rendMat]);
        base.VResetProperty(rend, mat);
    }
}
