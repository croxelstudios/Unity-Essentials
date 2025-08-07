using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ArbitraryProperty
{
    public string propertyName = "_Color";
    public PropertyType type = PropertyType.Color;

    [SerializeField]
    float floatValue = 0f;
    [SerializeField]
    int intValue = 0;
    [SerializeField]
    Color colorValue = Color.white;
    [SerializeField]
    Vector4 vectorValue = Vector4.zero;
    [SerializeField]
    Texture textureObject = null;

    float ofValue = 0f;
    int oiValue = 0;
    Color ocValue = Color.white;
    Vector4 ovValue = Vector4.zero;
    Texture otObject = null;

    public enum PropertyType { Float, Int, Color, Vector, Texture };

    public ArbitraryProperty(string propertyName, float value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Float;
        floatValue = value;
        intValue = 0;
        colorValue = Color.white;
        vectorValue = Vector4.zero;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, int value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Int;
        floatValue = 0f;
        intValue = value;
        colorValue = Color.white;
        vectorValue = Vector4.zero;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, Color value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Color;
        floatValue = 0f;
        intValue = 0;
        colorValue = value;
        vectorValue = Vector4.zero;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, Vector4 value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Vector;
        floatValue = 0f;
        intValue = 0;
        colorValue = Color.white;
        vectorValue = value;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, Vector3 value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Vector;
        floatValue = 0f;
        intValue = 0;
        colorValue = Color.white;
        vectorValue = value;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, Vector2 value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Vector;
        floatValue = 0f;
        intValue = 0;
        colorValue = Color.white;
        vectorValue = value;
        textureObject = null;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public ArbitraryProperty(string propertyName, Texture value)
    {
        this.propertyName = propertyName;
        type = PropertyType.Texture;
        floatValue = 0f;
        intValue = 0;
        colorValue = Color.white;
        vectorValue = Vector4.zero;
        textureObject = value;

        ofValue = 0f;
        oiValue = 0;
        ocValue = Color.white;
        ovValue = Vector4.zero;
        otObject = null;
    }

    public void SetFloat(float value)
    {
        type = PropertyType.Float;
        floatValue = value;
    }

    public void SetInt(int value)
    {
        type = PropertyType.Int;
        intValue = value;
    }

    public void SetColor(Color color)
    {
        type = PropertyType.Color;
        colorValue = color;
    }

    public void SetVector(Vector4 vector)
    {
        type = PropertyType.Vector;
        vectorValue = vector;
    }

    public void SetVector(Vector3 vector)
    {
        SetVector((Vector4)vector);
    }

    public void SetVector(Vector2 vector)
    {
        SetVector((Vector4)vector);
    }

    public void SetTexture(Texture texture)
    {
        type = PropertyType.Texture;
        textureObject = texture;
    }

    public float GetFloat()
    {
        return floatValue;
    }

    public int GetInt()
    {
        return intValue;
    }

    public Color GetColor()
    {
        return colorValue;
    }

    public Vector4 GetVector()
    {
        return vectorValue;
    }

    public Texture GetTexture()
    {
        return textureObject;
    }

    public void SaveOriginal(Material material)
    {
        switch (type)
        {
            case PropertyType.Float:
                ofValue = material.GetFloat(propertyName);
                break;
            case PropertyType.Int:
                oiValue = material.GetInt(propertyName);
                break;
            case PropertyType.Color:
                ocValue = material.GetColor(propertyName);
                break;
            case PropertyType.Vector:
                ovValue = material.GetVector(propertyName);
                break;
            case PropertyType.Texture:
                otObject = material.GetTexture(propertyName);
                break;
        }
    }

    public void SetProperty(Material material)
    {
        if ((material != null) && material.HasProperty(propertyName))
        {
            switch (type)
            {
                case PropertyType.Float:
                    material.SetFloat(propertyName, floatValue);
                    break;
                case PropertyType.Int:
                    material.SetInt(propertyName, intValue);
                    break;
                case PropertyType.Color:
                    material.SetColor(propertyName, colorValue);
                    break;
                case PropertyType.Vector:
                    material.SetVector(propertyName, vectorValue);
                    break;
                case PropertyType.Texture:
                    material.SetTexture(propertyName, textureObject);
                    break;
            }
        }
    }

    public void ResetProperty(Material material)
    {
        if ((material != null) && material.HasProperty(propertyName))
        {
            switch (type)
            {
                case PropertyType.Float:
                    material.SetFloat(propertyName, ofValue);
                    break;
                case PropertyType.Int:
                    material.SetInt(propertyName, oiValue);
                    break;
                case PropertyType.Color:
                    material.SetColor(propertyName, ocValue);
                    break;
                case PropertyType.Vector:
                    material.SetVector(propertyName, ovValue);
                    break;
                case PropertyType.Texture:
                    material.SetTexture(propertyName, otObject);
                    break;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ArbitraryProperty))]
public class MaterialPropertyTweaker_Inspector : PropertyDrawer
{
    SerializedProperty propertyName, type, fValue, iValue, cValue, vValue, tObject;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        propertyName = property.FindPropertyRelative("propertyName");

        type = property.FindPropertyRelative("type");

        fValue = property.FindPropertyRelative("floatValue");
        iValue = property.FindPropertyRelative("intValue");
        cValue = property.FindPropertyRelative("colorValue");
        vValue = property.FindPropertyRelative("vectorValue");
        tObject = property.FindPropertyRelative("textureObject");

        float height = EditorGUI.GetPropertyHeight(propertyName, new GUIContent("Property Name"));
        height += EditorGUI.GetPropertyHeight(type, new GUIContent("Type"));
        height += GetHeight();
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        propertyName = property.FindPropertyRelative("propertyName");

        type = property.FindPropertyRelative("type");

        fValue = property.FindPropertyRelative("floatValue");
        iValue = property.FindPropertyRelative("intValue");
        cValue = property.FindPropertyRelative("colorValue");
        vValue = property.FindPropertyRelative("vectorValue");
        tObject = property.FindPropertyRelative("textureObject");
        
        position.height = EditorGUI.GetPropertyHeight(propertyName, new GUIContent("Property Name"));
        EditorGUI.PropertyField(position, propertyName, new GUIContent("Property Name"));
        position.y += EditorGUI.GetPropertyHeight(propertyName, new GUIContent("Property Name"));
        position.height = EditorGUI.GetPropertyHeight(type, new GUIContent("Type"));
        EditorGUI.PropertyField(position, type, new GUIContent("Type"));
        position.y += EditorGUI.GetPropertyHeight(type, new GUIContent("Type"));
        position.height = GetHeight();
        ShowProperties(position);
    }

    void ShowProperties(Rect pos)
    {
        switch (type.intValue)
        {
            case 1:
                EditorGUI.PropertyField(pos, iValue, new GUIContent("Value"));
                break;
            case 2:
                EditorGUI.PropertyField(pos, cValue, new GUIContent("Color"));
                break;
            case 3:
                EditorGUI.PropertyField(pos, vValue, new GUIContent("Vector"));
                break;
            case 4:
                EditorGUI.PropertyField(pos, tObject, new GUIContent("Texture"));
                break;
            default:
                EditorGUI.PropertyField(pos, fValue, new GUIContent("Value"));
                break;
        }
    }

    float GetHeight()
    {
        switch (type.intValue)
        {
            case 1:
                return EditorGUI.GetPropertyHeight(iValue, new GUIContent("Value"));
            case 2:
                return EditorGUI.GetPropertyHeight(cValue, new GUIContent("Color"));
            case 3:
                return EditorGUI.GetPropertyHeight(vValue, new GUIContent("Vector"));
            case 4:
                return EditorGUI.GetPropertyHeight(tObject, new GUIContent("Texture"));
            default:
                return EditorGUI.GetPropertyHeight(fValue, new GUIContent("Value"));
        }
    }
}
#endif
