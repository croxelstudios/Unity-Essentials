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

    protected override void BlockSet(MaterialPropertyBlock block, float value)
    {
        block.SetFloat(propertyName, value);
    }

    protected override void MaterialSet(Material mat, float value)
    {
        mat.SetFloat(propertyName, value);
    }

    protected override float NeutralAdd()
    {
        return 0f;
    }

    protected override float NeutralMult()
    {
        return 1f;
    }

    protected override float Combine_Average(float current, float next, int count)
    {
        return current + (next / count);
    }

    protected override float Combine_Multiply(float current, float next)
    {
        return current * next;
    }

    protected override float Combine_Add(float current, float next)
    {
        return current + next;
    }

    protected override float Combine_Subtract(float current, float next)
    {
        return current - next;
    }

    public virtual void SetFloat(float n)
    {
        value = n;
        UpdateBehaviour();
    }

    protected override float GetProperty(RendMat renMat)
    {
        return renMat.sharedMaterial.GetFloat(propertyName);
    }
}
