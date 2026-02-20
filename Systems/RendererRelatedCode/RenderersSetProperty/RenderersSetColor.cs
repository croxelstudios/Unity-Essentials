using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor : BRenderersSetBlendedProperty<Color>
{
    [SerializeField]
    [DisableIf("propertyIsReadOnly")]
    [OnValueChanged("UpdateBehaviour")]
    Color _color = Color.white;
    public Color color { get { return _color; } protected set { SetColor(value);  } }
    protected override Color tValue { get { return _color; } set { _color = value; } }

    void Reset()
    {
        propertyName = "_BaseColor";
    }

    protected override void BlockSet(MaterialPropertyBlock block, Color value)
    {
        block.SetColor(propertyName, value);
    }

    protected override void MaterialSet(Material mat, Color value)
    {
        mat.SetColor(propertyName, value);
    }

    protected override Color NeutralAdd()
    {
        return Color.black;
    }

    protected override Color NeutralMult()
    {
        return Color.white;
    }

    protected override Color Combine_Average(Color current, Color next, int count)
    {
        float alpha = current.a;
        current += (next / count);
        current.a = alpha * next.a;
        return current;
    }

    protected override Color Combine_Multiply(Color current, Color next)
    {
        return current * next;
    }

    protected override Color Combine_Add(Color current, Color next)
    {
        float alpha = current.a;
        current += next;
        current.a = alpha * next.a;
        return current;
    }

    protected override Color Combine_Subtract(Color current, Color next)
    {
        float alpha = current.a;
        current -= next;
        current.a = alpha * next.a;
        return current;
    }

    public virtual void SetColor(Color color)
    {
        _color = color;
        UpdateBehaviour();
    }

    public virtual void SetAlpha(float alpha)
    {
        _color.a = alpha;
        UpdateBehaviour();
    }

    protected override Color GetProperty(Material material)
    {
        return material.GetColor(propertyName);
    }
}
