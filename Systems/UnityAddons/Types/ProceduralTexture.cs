using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct ProceduralTexture
{
    [SerializeField]
    Material textureMaterial;
    public readonly Material material => textureMaterial;
    [SerializeField]
    [Min(1)]
    int textureResolution;
    [SerializeField]
    TextureWrapMode wrapMode;
    [SerializeField]
    FilterMode filterMode;
    [SerializeField]
    int depth;
    [SerializeField]
    RenderTextureFormat format;
    [SerializeField]
    bool clear;

    [HideInInspector]
    public RenderTexture rt;

    public ProceduralTexture(Material textureMaterial)
    {
        this.textureMaterial = textureMaterial;
        textureResolution = 360;
        wrapMode = TextureWrapMode.Clamp;
        filterMode = FilterMode.Point;
        depth = 0;
        format = RenderTextureFormat.ARGB32;
        clear = false;
        rt = null;
    }

    public ProceduralTexture(Material textureMaterial, int textureResolution, TextureWrapMode wrapMode, FilterMode filterMode,
        int depth = 0, RenderTextureFormat format = RenderTextureFormat.ARGB32, bool clear = false)
    {
        this.textureMaterial = textureMaterial;
        this.textureResolution = textureResolution;
        this.wrapMode = wrapMode;
        this.filterMode = filterMode;
        this.depth = depth;
        this.format = format;
        this.clear = clear;
        rt = null;
    }

    public bool IsValid()
    {
        return textureMaterial != null;
    }

    public void Update(string name = "ProceduralTexture")
    {
        if (clear) rt.Clear();
        textureMaterial.Blit(ref rt, name, textureResolution, depth, format);
        rt.wrapMode = wrapMode;
        rt.filterMode = filterMode;
    }

    public void Update(IEnumerable<ArbitraryProperty> propertyModifications, string name = "ProceduralTexture")
    {
        foreach (ArbitraryProperty property in propertyModifications)
        {
            property.SaveOriginal(material);
            property.SetProperty(material);
        }
        Update(name);
        foreach (ArbitraryProperty property in propertyModifications)
            property.ResetProperty(material);
    }

    public void Release()
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
    }
}

[Serializable]
struct ProcTex
{
    [SerializeField]
    [HideLabel]
    [InlineProperty]
    ProceduralTexture proceduralTexture;
    [SerializeField]
    [OnValueChanged("UpdatePropertyModifications")]
    ArbitraryProperty[] propertyModifications;
    public RenderTexture rt { get { return proceduralTexture.rt; } }

    Dictionary<string, ArbitraryProperty> propertiesToModify;

    public ProcTex(Material material,
        ArbitraryProperty[] propertyModifications = null)
    {
        proceduralTexture = new ProceduralTexture(material);
        this.propertyModifications = propertyModifications;
        propertiesToModify = new Dictionary<string, ArbitraryProperty>();
        UpdatePropertyModifications();
    }

    public ProcTex(ProceduralTexture proceduralTexture,
        ArbitraryProperty[] propertyModifications = null)
    {
        this.proceduralTexture = proceduralTexture;
        this.propertyModifications = propertyModifications;
        propertiesToModify = new Dictionary<string, ArbitraryProperty>();
        UpdatePropertyModifications();
    }

    public void Release()
    {
        propertiesToModify.SmartClear();
        proceduralTexture.Release();
    }

    public bool IsValid()
    {
        return proceduralTexture.IsValid();
    }

    public void Update()
    {
        if (!propertiesToModify.IsNullOrEmpty())
            proceduralTexture.Update(propertiesToModify.Values);
        else proceduralTexture.Update();
    }

    #region Set Custom Properties
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

    public void ResetProperty(string propName)
    {
        if (!propName.IsNullOrEmpty())
            propertiesToModify.Remove(propName);
    }
    #endregion

    public void UpdatePropertyModifications()
    {
        if (!propertiesToModify.IsNullOrEmpty())
            for (int i = 0; i < propertyModifications.Length; i++)
                SetArbitraryProperty(propertyModifications[i]);
    }
}
