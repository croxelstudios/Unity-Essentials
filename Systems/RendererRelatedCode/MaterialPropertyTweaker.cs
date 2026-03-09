using UnityEngine;
using Sirenix.OdinInspector;

//TO DO: Add support for MaterialSource as in MaterialPerCameraTweaker
[ExecuteAlways]
public class MaterialPropertyTweaker : MonoBehaviour
{
    [SerializeField]
    [OnValueChanged("SetProperty")]
    Material material = null;
    [SerializeField]
    [OnValueChanged("SetProperty", true)]
    ArbitraryProperty property = new ArbitraryProperty("_Color", Color.white);
    [Indent]
    [SerializeField]
    [OnValueChanged("SetProperty")]
    [ShowIf("@property.CanBlend()")]
    BlendMode blendMode = BlendMode.Multiply;
    [OnValueChanged("SetProperty")]
    [ShowIf("@property.CanBlend()")]
    bool blendWithOriginal = false;
    [SerializeField]
    [OnValueChanged("SetProperty")]
    int priority = 0;

    PriorityHandler<SharedMatProp> priorityHandler;

    void OnEnable()
    {
        SetProperty();
    }

    void OnDisable()
    {
        if (priorityHandler != null)
            priorityHandler.Dispose();
        ResetProperty();
    }

    void SetProperty()
    {
        ResetProperty();
        priorityHandler = priorityHandler.CreateRegister(
            new SharedMatProp(material, property.propertyName), priority, SetProperty);
        if (priorityHandler.CanAct())
            property.SetProperty(material, blendMode, blendWithOriginal);
    }

    public void ResetProperty()
    {
        property.ResetProperty(material);
    }

    #region Setters
    public void SetColor(Color color)
    {
        property.SetColor(color);
        SetProperty();
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

    public void SetVector(Vector4 value)
    {
        property.SetVector(value);
        SetProperty();
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

    public void SetTexture(Texture texture)
    {
        property.SetTexture(texture);
        SetProperty();
    }
    #endregion

    #region Getters
    public Color GetColor()
    {
        return property.GetColor();
    }

    public float GetAlpha(bool useBlackValue = false)
    {
        return property.GetAlpha(useBlackValue);
    }

    public float GetFloat()
    {
        return property.GetFloat();
    }

    public Vector4 GetVector()
    {
        return property.GetVector();
    }

    public int GetInt()
    {
        return property.GetInt();
    }

    public Texture GetTexture()
    {
        return property.GetTexture();
    }
    #endregion
}
