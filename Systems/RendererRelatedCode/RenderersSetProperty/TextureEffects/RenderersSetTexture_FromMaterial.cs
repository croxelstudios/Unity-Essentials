using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteAlways]
public class RenderersSetTexture_FromMaterial : RenderersSetTexture
{
    [Space]
    [SerializeField]
    Texture2D _defaultTexture = null;
    public Texture2D defaultTexture
    { get { return _defaultTexture; } set { _defaultTexture = value; } }
    [SerializeField]
    [InlineProperty]
    ProcTex[] preProcessors = null;
    [SerializeField]
    [HideLabel]
    [InlineProperty]
    ProcTex proceduralTexture = new ProcTex(null);
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    string propName;

    protected override void OnEnable()
    {
        for (int i = 0; i < preProcessors.Length; i++)
            preProcessors[i].UpdatePropertyModifications();
        proceduralTexture.UpdatePropertyModifications();

        if (timeMode == RenderingTimeModeOrOnEnable.OnEnable)
            UpdateRT();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < preProcessors.Length; i++)
            preProcessors[i].Release();
        proceduralTexture.Release();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif

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

    #region Set Custom Properties
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
        proceduralTexture.SetFloat(propName, value);
    }

    public void SetInt(string propName, int value)
    {
        proceduralTexture.SetInt(propName, value);
    }

    public void SetColor(string propName, Color color)
    {
        proceduralTexture.SetColor(propName, color);
    }

    public void SetVector(string propName, Vector4 vector)
    {
        proceduralTexture.SetVector(propName, vector);
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
        proceduralTexture.SetTexture(propName, texture);
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
        ResetProperty(propName);
    }

    public void ResetProperty(string propName)
    {
        proceduralTexture.ResetProperty(propName);
    }
    #endregion

    public void UpdateRT()
    {
        Texture texture = defaultTexture;
        for (int i = 0; i < preProcessors.Length; i++)
        {
            if (preProcessors[i].IsValid())
            {
                if (texture != null)
                    preProcessors[i].SetTexture("_MainTex", texture);
                preProcessors[i].Update();
                texture = preProcessors[i].rt;
            }
        }

        if (proceduralTexture.IsValid())
        {
            if (texture != null)
                proceduralTexture.SetTexture("_MainTex", texture);
            proceduralTexture.Update();
            _texture = proceduralTexture.rt;
        }
    }
}
