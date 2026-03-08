using UnityEngine;
using Sirenix.OdinInspector;

//TO DO: Add support for MaterialSource as in MaterialPerCameraTweaker
[ExecuteAlways]
public class MaterialPropertyTweaker : MonoBehaviour
{
    [SerializeField]
    [OnValueChanged("OnUpdate")]
    Material material = null;
    [SerializeField]
    [OnValueChanged("OnUpdate", true)]
    ArbitraryProperty property = new ArbitraryProperty("_Color", Color.white);
    //[Indent]
    //[SerializeField]
    //[OnValueChanged("OnUpdate")]
    //[ShowIf("@property.CanBlend()")]
    //BlendMode blendMode = BlendMode.Multiply;
    //[OnValueChanged("OnUpdate")]
    //[ShowIf("@property.CanBlend()")]
    //bool blendWithOriginal = false;
    [SerializeField]
    [OnValueChanged("OnUpdate")]
    int priority = 0;
    //BlendMode oldBlendMode;

    MatPropModifier priorityHandler;

    void OnEnable()
    {
        priorityHandler = new MatPropModifier(material, property.propertyName, priority, OnUpdate);
        property.SaveOriginal(material);
        OnUpdate();
    }

    void OnDisable()
    {
        priorityHandler.Dispose();
        ResetProperty();
    }

    void OnUpdate()
    {
        if (priorityHandler.CanAct()) SetProperty();
    }

    public void SetProperty()
    {
        property.SetProperty(material);
    }

    public void ResetProperty()
    {
        property.ResetProperty(material);
    }

    public void SetColor(Color color)
    {
        property.SetColor(color);
        SetProperty();
    }

    public Color GetColor()
    {
        return property.GetColor();
    }

    public float GetAlpha(bool useBlackValue = false)
    {
        return property.GetAlpha(useBlackValue);
    }

    public void SetFloat(float value)
    {
        property.SetFloat(value);
        SetProperty();
    }

    public void SetAlpha(float value)
    {
        SetAlpha(value, false);
        SetProperty();
    }

    public void SetBlackValue(float value)
    {
        SetAlpha(value, true);
        SetProperty();
    }

    public void SetAlpha(float value, bool useBlackValue)
    {
        property.SetAlpha(value, useBlackValue);
        SetProperty();
    }

    public float GetFloat()
    {
        return property.GetFloat();
    }

    public void SetVector(Vector4 value)
    {
        property.SetVector(value);
        SetProperty();
    }

    public Vector4 GetVector()
    {
        return property.GetVector();
    }

    public void SetVector(Vector3 value)
    {
        property.SetVector(value);
        SetProperty();
    }

    public void SetVector(Vector2 value)
    {
        property.SetVector(value);
        SetProperty();
    }

    public void SetInt(int value)
    {
        property.SetInt(value);
        SetProperty();
    }

    public int GetInt()
    {
        return property.GetInt();
    }

    public void SetTexture(Texture texture)
    {
        property.SetTexture(texture);
        SetProperty();
    }

    public Texture GetTexture()
    {
        return property.GetTexture();
    }
}
