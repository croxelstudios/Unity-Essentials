using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetFloat : BRenderersSetBlendedProperty<float>
{
    [SerializeField]
    [DisableIf("propertyIsReadOnly")]
    [OnValueChanged("UpdateBehaviour")]
    protected float value = 0.5f;
    public float Value { get { return value; } protected set { SetFloat(value); } }
    protected override float tValue { get { return value; } set { this.value = value; } }

    void Reset()
    {
        propertyName = "_Cutout";
        blendMode = BlendMode.Average;
    }

    protected override void BlockSet(MaterialPropertyBlock block, float value, string propertyName)
    {
        block.SetFloat(propertyName, value);
    }

    protected override void MaterialSet(Material mat, float value, string propertyName)
    {
        mat.SetFloat(propertyName, value);
    }

    public virtual void SetFloat(float n)
    {
        value = n;
        UpdateBehaviour();
    }

    protected override float GetProperty(Material material, string propertyName)
    {
        return material.GetFloat(propertyName);
    }
}
