using UnityEngine;
using System;
using System.Collections.Generic;
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

    static Dictionary<SharedMatProp, List<ArbitraryProperty>> affectingProperties;

    Dictionary<SharedMatProp, float> ofValue;
    Dictionary<SharedMatProp, int> oiValue;
    Dictionary<SharedMatProp, Color> ocValue;
    Dictionary<SharedMatProp, Vector4> ovValue;
    Dictionary<SharedMatProp, Texture> otObject;

    ValueBlender<float> fBlender;
    ValueBlender<int> iBlender;
    ValueBlender<Color> cBlender;
    ValueBlender<Vector4> vBlender;

    ValueBlender<float> ofBlender;
    ValueBlender<int> oiBlender;
    ValueBlender<Color> ocBlender;
    ValueBlender<Vector4> ovBlender;

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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

        ofValue = null;
        oiValue = null;
        ocValue = null;
        ovValue = null;
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

    public void SetAlpha(float value, bool useBlackValue)
    {
        switch (type)
        {
            case PropertyType.Float:
                floatValue = value;
                break;
            case PropertyType.Int:
                intValue = Mathf.FloorToInt(value * 100f);
                break;
            case PropertyType.Color:
                if (useBlackValue)
                {
                    Color.RGBToHSV(colorValue, out float h, out float s, out float v);
                    colorValue = Color.HSVToRGB(h, s, value);
                }
                else colorValue.a = value;
                break;
            case PropertyType.Vector:
                if (useBlackValue)
                {
                    Color.RGBToHSV((Color)vectorValue, out float h, out float s, out float v);
                    vectorValue = (Vector4)Color.HSVToRGB(h, s, value);
                }
                else vectorValue.w = value;
                break;
        }
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

    public float GetAlpha(bool useBlackValue = false)
    {
        switch (type)
        {
            case PropertyType.Float:
                return floatValue;
            case PropertyType.Int:
                return intValue / 100f;
            case PropertyType.Color:
                if (useBlackValue)
                {
                    Color.RGBToHSV(colorValue, out float h, out float s, out float v);
                    return v;
                }
                else return colorValue.a;
            case PropertyType.Vector:
                if (useBlackValue)
                {
                    Color.RGBToHSV((Color)vectorValue, out float h, out float s, out float v);
                    return v;
                }
                else return vectorValue.w;
            default:
                return 1f;
        }
    }

    void SaveOriginal(Material material)
    {
        SharedMatProp matProp = new SharedMatProp(material, propertyName);

        if ((material != null) && material.HasProperty(propertyName))
        {
            switch (type)
            {
                case PropertyType.Float:
                    ofValue = ofValue.CreateAdd(matProp, material.GetFloat(propertyName));
                    break;
                case PropertyType.Int:
                    oiValue = oiValue.CreateAdd(matProp, material.GetInt(propertyName));
                    break;
                case PropertyType.Color:
                    ocValue = ocValue.CreateAdd(matProp, material.GetColor(propertyName));
                    break;
                case PropertyType.Vector:
                    ovValue = ovValue.CreateAdd(matProp, material.GetVector(propertyName));
                    break;
                case PropertyType.Texture:
                    otObject = otObject.CreateAdd(matProp, material.GetTexture(propertyName));
                    break;
            }
        }
    }

    public void SetProperty(Material material, BlendMode blendMode = BlendMode.Multiply, bool blendWithOriginal = false)
    {
        if ((material != null) && material.HasProperty(propertyName))
        {
            SharedMatProp matProp = new SharedMatProp(material, propertyName);

            switch (type)
            {
                case PropertyType.Texture:
                    SaveOriginal(material);
                    break;
                default:
                    affectingProperties = affectingProperties.CreateIfNull();
                    if (!affectingProperties.ContainsKey(matProp))
                    {
                        SaveOriginal(material);
                        affectingProperties = affectingProperties.CreateAdd(matProp, this);
                    }
                    else affectingProperties[matProp].TryAdd(this);
                    break;
            }

            switch (type)
            {
                case PropertyType.Float:
                    fBlender = fBlender.CreateRegister(this, matProp, floatValue, blendMode);
                    if (blendWithOriginal)
                        ofBlender =
                            ofBlender.CreateRegister(this, matProp, SavedFloat(matProp), blendMode);
                    material.SetFloat(propertyName, fBlender.GetBlend());
                    break;
                case PropertyType.Int:
                    iBlender = iBlender.CreateRegister(this, matProp, intValue, blendMode);
                    if (blendWithOriginal)
                        oiBlender =
                            oiBlender.CreateRegister(this, matProp, SavedInt(matProp), blendMode);
                    material.SetInt(propertyName, iBlender.GetBlend());
                    break;
                case PropertyType.Color:
                    cBlender = cBlender.CreateRegister(this, matProp, colorValue, blendMode);
                    if (blendWithOriginal)
                        ocBlender =
                            ocBlender.CreateRegister(this, matProp, SavedColor(matProp), blendMode);
                    material.SetColor(propertyName, cBlender.GetBlend());
                    break;
                case PropertyType.Vector:
                    vBlender = vBlender.CreateRegister(this, matProp, vectorValue, blendMode);
                    if (blendWithOriginal)
                        ovBlender =
                            ovBlender.CreateRegister(this, matProp, SavedVector(matProp), blendMode);
                    material.SetVector(propertyName, vBlender.GetBlend());
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
                    if (fBlender != null) fBlender.Dispose();
                    if (ofBlender != null) ofBlender.Dispose();
                    break;
                case PropertyType.Int:
                    if (iBlender != null) iBlender.Dispose();
                    if (oiBlender != null) oiBlender.Dispose();
                    break;
                case PropertyType.Color:
                    if (cBlender != null) cBlender.Dispose();
                    if (ocBlender != null) ocBlender.Dispose();
                    break;
                case PropertyType.Vector:
                    if (vBlender != null) vBlender.Dispose();
                    if (ovBlender != null) ovBlender.Dispose();
                    break;
            }

            SharedMatProp matProp = new SharedMatProp(material, propertyName);

            bool shouldReset = false;
            switch (type)
            {
                case PropertyType.Texture:
                    shouldReset = true;
                    break;
                default:
                    affectingProperties.SmartRemoveClear(matProp, this);
                    if (!affectingProperties.NotNullContainsKey(matProp))
                        shouldReset = true;
                    break;
            }

            if (shouldReset)
                switch (type)
                {
                    case PropertyType.Float:
                        material.SetFloat(propertyName, SavedFloat(matProp));
                        break;
                    case PropertyType.Int:
                        material.SetInt(propertyName, SavedInt(matProp));
                        break;
                    case PropertyType.Color:
                        material.SetColor(propertyName, SavedColor(matProp));
                        break;
                    case PropertyType.Vector:
                        material.SetVector(propertyName, SavedVector(matProp));
                        break;
                    case PropertyType.Texture:
                        material.SetTexture(propertyName, SavedTexture(matProp));
                        break;
                }
        }
    }

    float SavedFloat(SharedMatProp matProp)
    {
        return ofValue.NotNullContainsKey(matProp) ? ofValue[matProp] : 0f;
    }

    int SavedInt(SharedMatProp matProp)
    {
        return oiValue.NotNullContainsKey(matProp) ? oiValue[matProp] : 0;
    }

    Color SavedColor(SharedMatProp matProp)
    {
        return ocValue.NotNullContainsKey(matProp) ? ocValue[matProp] : Color.white;
    }

    Vector4 SavedVector(SharedMatProp matProp)
    {
        return ovValue.NotNullContainsKey(matProp) ? ovValue[matProp] : Vector4.zero;
    }

    Texture SavedTexture(SharedMatProp matProp)
    {
        return otObject.NotNullContainsKey(matProp) ? otObject[matProp] : null;
    }

    public bool CanBlend()
    {
        return type != PropertyType.Texture;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ArbitraryProperty))]
public class ArbitraryProperty_Drawer : PropertyDrawer
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
                vValue.vector4Value = EditorGUI.Vector4Field(pos, new GUIContent("Vector"), vValue.vector4Value);
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
                return EditorGUIUtility.singleLineHeight;
            case 4:
                return EditorGUI.GetPropertyHeight(tObject, new GUIContent("Texture"));
            default:
                return EditorGUI.GetPropertyHeight(fValue, new GUIContent("Value"));
        }
    }
}
#endif
