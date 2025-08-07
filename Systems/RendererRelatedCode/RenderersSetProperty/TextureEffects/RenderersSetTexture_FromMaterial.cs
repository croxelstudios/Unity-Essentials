using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class RenderersSetTexture_FromMaterial : RenderersSetTexture
{
    Dictionary<string, ArbitraryProperty> propertiesToModify;

    [Space]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    ProceduralTexture proceduralTexture = new ProceduralTexture(null);
    [SerializeField]
    [OnValueChanged("UpdatePropertyModifications")]
    ArbitraryProperty[] propertyModifications = null;
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    string propName;

    protected override void OnEnable()
    {
        if (!propertyModifications.IsNullOrEmpty())
        {
            propertiesToModify = new Dictionary<string, ArbitraryProperty>();
            for (int i = 0; i < propertyModifications.Length; i++)
                propertiesToModify.Add(propertyModifications[i].propertyName, propertyModifications[i]);
        }

        if (timeMode == RenderingTimeModeOrOnEnable.OnEnable)
            UpdateRT();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        propertiesToModify.SmartClear();
        proceduralTexture.Release();
    }

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
    }

    protected override void UpdateBehaviour()
    {
        UpdateRT();
        base.UpdateBehaviour();
    }

    public void SetPropertyName(string name)
    {
        propName = name;
    }

    public void SetFloat(float value)
    {
        SetFloat(propName, value);
    }

    public void SetInt(int value)
    {
        SetInt(propName, value);
    }

    public void SetColor(Color color)
    {
        SetColor(propName, color);
    }

    public void SetVector(Vector4 vector)
    {
        SetVector(propName, vector);
    }

    public void SetVector(Vector3 vector)
    {
        SetVector(propName, vector);
    }

    public void SetVector(Vector2 vector)
    {
        SetVector(propName, vector);
    }

    public void SetTexture(Texture texture)
    {
        SetTexture(propName, texture);
    }

    public void SetFloat(string propName, float value)
    {
        if (!propName.IsNullOrEmpty())
        {
            propertiesToModify = propertiesToModify.CreateIfNull();
            if (propertiesToModify.TryGetValue(propName, out ArbitraryProperty prop))
                prop.SetFloat(value);
            else propertiesToModify.Add(propName, new ArbitraryProperty(propName, value));
        }
    }

    public void SetInt(string propName, int value)
    {
        if (!propName.IsNullOrEmpty())
        {
            propertiesToModify = propertiesToModify.CreateIfNull();
            if (propertiesToModify.TryGetValue(propName, out ArbitraryProperty prop))
                prop.SetInt(value);
            else propertiesToModify.Add(propName, new ArbitraryProperty(propName, value));
        }
    }

    public void SetColor(string propName, Color color)
    {
        if (!propName.IsNullOrEmpty())
        {
            propertiesToModify = propertiesToModify.CreateIfNull();
            if (propertiesToModify.TryGetValue(propName, out ArbitraryProperty prop))
                prop.SetColor(color);
            else propertiesToModify.Add(propName, new ArbitraryProperty(propName, color));
        }
    }

    public void SetVector(string propName, Vector4 vector)
    {
        if (!propName.IsNullOrEmpty())
        {
            propertiesToModify = propertiesToModify.CreateIfNull();
            if (propertiesToModify.TryGetValue(propName, out ArbitraryProperty prop))
                prop.SetVector(vector);
            else propertiesToModify.Add(propName, new ArbitraryProperty(propName, vector));
        }
    }

    public void SetVector(string propName, Vector3 vector)
    {
        SetVector(propName, (Vector4)vector);
    }

    public void SetVector(string propName, Vector2 vector)
    {
        SetVector(propName, (Vector4)vector);
    }

    public void SetTexture(string propName, Texture texture)
    {
        if (!propName.IsNullOrEmpty())
        {
            propertiesToModify = propertiesToModify.CreateIfNull();
            if (propertiesToModify.TryGetValue(propName, out ArbitraryProperty prop))
                prop.SetTexture(texture);
            else propertiesToModify.Add(propName, new ArbitraryProperty(propName, texture));
        }
    }

    public void SetArbitraryProperty(ArbitraryProperty property)
    {
        switch (property.type)
        {
            case ArbitraryProperty.PropertyType.Int:
                SetInt(property.propertyName, property.GetInt());
                break;
            case ArbitraryProperty.PropertyType.Color:
                SetColor(property.propertyName, property.GetColor());
                break;
            case ArbitraryProperty.PropertyType.Vector:
                SetVector(property.propertyName, property.GetVector());
                break;
            case ArbitraryProperty.PropertyType.Texture:
                SetTexture(property.propertyName, property.GetTexture());
                break;
            default:
                SetFloat(property.propertyName, property.GetFloat());
                break;
        }
    }

    public void ResetProperty()
    {
        if (!propName.IsNullOrEmpty())
            propertiesToModify.Remove(propName);
    }

    public void UpdateRT()
    {
        if (proceduralTexture.IsValid())
        {
            if (!propertiesToModify.IsNullOrEmpty())
                foreach (KeyValuePair<string, ArbitraryProperty> kvp in propertiesToModify)
                {
                    kvp.Value.SaveOriginal(proceduralTexture.material);
                    kvp.Value.SetProperty(proceduralTexture.material);
                }
            proceduralTexture.Update();
            if (!propertiesToModify.IsNullOrEmpty())
                foreach (KeyValuePair<string, ArbitraryProperty> kvp in propertiesToModify)
                    kvp.Value.ResetProperty(proceduralTexture.material);
            _texture = proceduralTexture.rt;
        }
    }

    public void UpdatePropertyModifications()
    {
        if (!propertiesToModify.IsNullOrEmpty())
            for (int i = 0; i < propertyModifications.Length; i++)
                SetArbitraryProperty(propertyModifications[i]);
    }
}
