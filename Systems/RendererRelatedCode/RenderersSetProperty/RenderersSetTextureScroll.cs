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

    Vector2 direction;
    Vector2 currentOffset;
    protected override void Init()
    {
        if (propertyName == "") propertyName = "_MainTex";
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
        block.SetVector(fullPropertyName, tilingOffset);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        //TO DO
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        string fullPropertyName = propertyName + "_ST";
        Vector4 tilingOffset = rend.sharedMaterials[mat].GetVector(fullPropertyName);

        tilingOffset.z = currentOffset.x;
        tilingOffset.w = currentOffset.y;
        rend.materials[mat].SetVector(fullPropertyName, tilingOffset);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        //TO DO
        base.VResetProperty(rend, mat);
    }
}
