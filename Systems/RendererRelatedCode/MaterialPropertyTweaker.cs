using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MaterialPropertyTweaker : MonoBehaviour
{
    [SerializeField]
    Material material = null;
    [SerializeField]
    bool update = true;
    [SerializeField]
    string propertyName = "_Color";
    [SerializeField]
    PropertyType type = PropertyType.Color;

    [SerializeField]
    [HideInInspector]
    float fValue = 0f;
    [SerializeField]
    [HideInInspector]
    int iValue = 0;
    [SerializeField]
    [HideInInspector]
    Color cValue = Color.white;
    [SerializeField]
    [HideInInspector]
    Vector4 vValue = Vector4.zero;
    [SerializeField]
    [HideInInspector]
    Texture tObject = null;

    float ofValue = 0f;
    int oiValue = 0;
    Color ocValue = Color.white;
    Vector4 ovValue = Vector4.zero;
    Texture otObject = null;

    void OnEnable()
    {
        SaveOriginal();
        SetProperty();
    }

    void OnDisable()
    {
        ResetProperty();
    }

    void Update()
    {
        if (update) SetProperty();
    }

    void SaveOriginal()
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

    void SetProperty()
    {
        if ((material != null) && material.HasProperty(propertyName))
        {
            switch (type)
            {
                case PropertyType.Float:
                    material.SetFloat(propertyName, fValue);
                    break;
                case PropertyType.Int:
                    material.SetInt(propertyName, iValue);
                    break;
                case PropertyType.Color:
                    material.SetColor(propertyName, cValue);
                    break;
                case PropertyType.Vector:
                    material.SetVector(propertyName, vValue);
                    break;
                case PropertyType.Texture:
                    material.SetTexture(propertyName, tObject);
                    break;
            }
        }
    }

    void ResetProperty()
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

    public void SetColor(Color color)
    {
        float a = cValue.a;
        cValue = color;
        cValue.a = a;
    }

    public Color GetColor()
    {
        return cValue;
    }

    public void SetFloat(float value)
    {
        fValue = value;
    }

    public float GetFloat()
    {
        return fValue;
    }

    public void SetVector(Vector4 value)
    {
        vValue = value;
    }

    public Vector4 GetVector()
    {
        return vValue;
    }

    public void SetVector(Vector3 value)
    {
        vValue = value;
    }

    public void SetVector(Vector2 value)
    {
        vValue = value;
    }

    public void SetInt(int value)
    {
        iValue = value;
    }

    public int GetInt()
    {
        return iValue;
    }

    public void SetTexture(Texture texture)
    {
        tObject = texture;
    }

    public Texture GetTexture()
    {
        return tObject;
    }

    enum PropertyType { Float, Int, Color, Vector, Texture };

#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialPropertyTweaker))]
    public class MaterialPropertyTweaker_Inspector : Editor
    {
        SerializedProperty type, fValue, iValue, cValue, vValue, tObject;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            type = serializedObject.FindProperty("type");

            fValue = serializedObject.FindProperty("fValue");
            iValue = serializedObject.FindProperty("iValue");
            cValue = serializedObject.FindProperty("cValue");
            vValue = serializedObject.FindProperty("vValue");
            tObject = serializedObject.FindProperty("tObject");

            ShowProperties();

            serializedObject.ApplyModifiedProperties();
        }

        void ShowProperties()
        {
            switch (type.intValue)
            {
                case 1:
                    EditorGUILayout.PropertyField(iValue, new GUIContent("Value"));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(cValue, new GUIContent("Color"));
                    break;
                case 3:
                    EditorGUILayout.PropertyField(vValue, new GUIContent("Vector"));
                    break;
                case 4:
                    EditorGUILayout.PropertyField(tObject, new GUIContent("Texture"));
                    break;
                default:
                    EditorGUILayout.PropertyField(fValue, new GUIContent("Value"));
                    break;
            }
        }
    }
#endif
}
