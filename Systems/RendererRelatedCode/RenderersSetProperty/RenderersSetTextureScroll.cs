using Sirenix.OdinInspector;
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
    [SerializeField]
    [OnValueChanged("UpdateRefOffset")]
    public Vector2 _referenceOffset = Vector2.zero;
    public Vector2 referenceOffset
    {
        get { return _referenceOffset; }
        set { _referenceOffset = value; UpdateRefOffset(); }
    }
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
        UpdateRefOffset();
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

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        string fullPropertyName = propertyName + "_ST";
        Vector4 tilingOffset = rendMat.sharedMaterial.GetVector(fullPropertyName);

        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, tilingOffset);

        tilingOffset.z = currentOffset.x;
        tilingOffset.w = currentOffset.y;

        block.SetVector(fullPropertyName, tilingOffset);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        if (originals.ContainsKey(rendMat))
            block.SetVector(rendMat.property, originals[rendMat]);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        string fullPropertyName = propertyName + "_ST";
        Vector4 tilingOffset = rendMat.sharedMaterial.GetVector(fullPropertyName);

        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, tilingOffset);

        tilingOffset.z = currentOffset.x;
        tilingOffset.w = currentOffset.y;

        rendMat.material.SetVector(fullPropertyName, tilingOffset);
    }

    protected override void VResetProperty(RendMatProp rendMat)
    {
        if (originals.ContainsKey(rendMat))
            rendMat.material.SetVector(rendMat.property, originals[rendMat]);
        base.VResetProperty(rendMat);
    }

    void UpdateRefOffset()
    {
        currentOffset = _referenceOffset;
    }
}
