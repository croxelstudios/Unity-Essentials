using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetFloat4 : BRenderersSetBlendedProperty<Vector4>
{
    [OnValueChanged("UpdateBehaviour")]
    public Vector4 value = new Vector4(1f, 1f, 0f, 0f);
    public Vector4 Value { get { return value; } protected set { Set(value); } }
    protected override Vector4 tValue { get { return Value; } set { Value = value; } }

    void Reset()
    {
        propertyName = "_MainTex_ST";
        blendMode = BlendMode.Average;
    }

    protected override void BlockSet(MaterialPropertyBlock block, Vector4 value)
    {
        block.SetVector(propertyName, value);
    }

    protected override void MaterialSet(Material mat, Vector4 value)
    {
        mat.SetVector(propertyName, value);
    }

    protected override Vector4 NeutralAdd()
    {
        return Vector4.zero;
    }

    protected override Vector4 NeutralMult()
    {
        return Vector4.one;
    }

    protected override Vector4 Combine_Average(Vector4 current, Vector4 next, int count)
    {
        return current + (next / count);
    }

    protected override Vector4 Combine_Multiply(Vector4 current, Vector4 next)
    {
        return Vector4.Scale(current, next);
    }

    protected override Vector4 Combine_Add(Vector4 current, Vector4 next)
    {
        return current + next;
    }

    protected override Vector4 Combine_Subtract(Vector4 current, Vector4 next)
    {
        return current - next;
    }

    public void SetX(float n)
    {
        Set(new Vector4(n, value.y, value.z, value.w));
    }

    public void SetY(float n)
    {
        Set(new Vector4(value.x, n, value.z, value.w));
    }

    public void SetZ(float n)
    {
        Set(new Vector4(value.x, value.y, n, value.w));
    }

    public void SetW(float n)
    {
        Set(new Vector4(value.x, value.y, value.z, n));
    }

    public void Set(Vector2 value)
    {
        Set(new Vector4(value.x, value.y, this.value.z, this.value.w));
    }

    public void SetZW(Vector2 value)
    {
        Set(new Vector4(this.value.x, this.value.y, value.x, value.y));
    }

    public void Set(Vector3 value)
    {
        Set(new Vector4(value.x, value.y, value.z, this.value.w));
    }

    public void Set(Vector4 value)
    {
        this.value = value;
        UpdateBehaviour();
    }
}
