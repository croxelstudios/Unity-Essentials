using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor : BRenderersSetBlendedProperty<Color>
{
    [SerializeField]
    bool setAlphaOnly = false;
    [SerializeField]
    [DisableIf("propertyIsReadOnly")]
    [OnValueChanged("UpdateBehaviour")]
    Color _color = Color.white;
    public Color color { get { return _color; } protected set { SetColor(value); } }
    protected override Color tValue { get { return _color; } set { _color = value; } }

    void Reset()
    {
        propertyName = "_BaseColor";
    }

    protected override Color PreprocessValue(RendMatProp rendMat, Color value)
    {
        Color newVal = value;
        if (setAlphaOnly)
        {
            if (Count(rendMat) <= 0)
                newVal = GetProperty(rendMat.sharedMaterial, rendMat.property);
            else
            {
                switch (blendMode)
                {   
                    case BlendMode.Average:
                        newVal = Color.clear;
                        break;
                    case BlendMode.Add:
                        newVal = Color.clear;
                        break;
                    case BlendMode.Subtract:
                        newVal = Color.white;
                        break;
                    default:
                        newVal = Color.white;
                        break;
                }
            }

            newVal.a = value.a;
        }
        return newVal;
    }

    protected override void BlockSet(MaterialPropertyBlock block, Color value, string propertyName)
    {
        block.SetColor(propertyName, value);
    }

    protected override void MaterialSet(Material mat, Color value, string propertyName)
    {
        mat.SetColor(propertyName, value);
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

    protected override Color GetProperty(Material material, string propertyName)
    {
        return material.GetColor(propertyName);
    }
}
