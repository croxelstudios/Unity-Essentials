using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MaterialEditorAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MaterialEditorAttribute))]
public class MaterialEditorAttribute_Drawer : PropertyDrawer
{
    MaterialEditor[] materialEditors;
    IMGUIContainer[] containers;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent(label.text));

        if (materialEditors == null)
            materialEditors = new MaterialEditor[1];
        if (containers == null)
            containers = new IMGUIContainer[1];

        Material mat = property.objectReferenceValue as Material;
        if (mat != null)
        {
            for (int i = 1; i < materialEditors.Length; i++)
                if (materialEditors[i] != null)
                    Object.DestroyImmediate(materialEditors[i]);

            Editor matEd = materialEditors[0];
            Editor.CreateCachedEditor(mat, typeof(MaterialEditor), ref matEd);
            if (containers[0] == null) containers[0] = CreateIMGUI(matEd);
            Editor refEd = null;
            AddIMGUI(property.GetContainingEditorWindow(ref refEd), containers[0]);
        }
        else if (property.arrayElementType == typeof(Material).Name)
        {
            for (int i = property.arraySize; i < materialEditors.Length; i++)
                if (materialEditors[i] != null)
                    Object.DestroyImmediate(materialEditors[i]);

            if (materialEditors.Length < property.arraySize)
                materialEditors = new MaterialEditor[property.arraySize];
            if (containers.Length < property.arraySize)
                containers = new IMGUIContainer[property.arraySize];

            for (int i = 0; i < property.arraySize; i++)
            {
                property.Next(true);
                mat = property.objectReferenceValue as Material;
                if (mat != null)
                {
                    Editor matEd = materialEditors[i];
                    Editor.CreateCachedEditor(mat, typeof(MaterialEditor), ref matEd);
                    if (containers[i] == null) containers[i] = CreateIMGUI(matEd);
                    Editor refEd = null;
                    AddIMGUI(property.GetContainingEditorWindow(ref refEd), containers[i]);
                }
            }
        }
    }

    void AddIMGUI(EditorWindow window, IMGUIContainer container)
    {
        BaseInspectorInjector.AddVisualElement(container, window);
    }

    IMGUIContainer CreateIMGUI(Editor editor)
    {
        IMGUIContainer imgui = new IMGUIContainer(() =>
        {
            // Prueba a dibujar algo sencillo
            EditorGUILayout.LabelField("Test del MaterialEditor");
            if (editor != null)
                editor.OnInspectorGUI();
        });
        imgui.style.height = 300;
        imgui.style.width = 300;
        return imgui;
    }
}
#endif
/*
void AddVisualElement(EditorWindow window, VisualElement visualElement, string name)
{
    VisualElement element = window.rootVisualElement.Q(className: name);
    if (element != null) return;

    visualElement.AddToClassList(name);
}

string GenerateClassName(Material material)
{
    return material.GetHashCode() + "_VisualElementClass";
}*/
