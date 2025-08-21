#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class BaseInspectorInjector
{
    /*
    const string CustomButtonText = "My Custom Button";
    const string CustomButtonClassName = "unity-inspector-my-custom-button";
    const string CustomButtonStyleSheet = "MyCustomButton";

    const string UnityInspectorClassName = "unity-inspector-main-container";
    const string AddComponentButtonClassName = "unity-inspector-add-component-button";
    */

    static Dictionary<VisualElement, EditorWindow> windowsDic;
    static Dictionary<VisualElement, EditorWindow> references;

    static BaseInspectorInjector()
    {
        EditorApplication.delayCall += OnUpdate;
        //EditorApplication.update += RemoveInjections;
    }

    private static void OnUpdate()
    {
        /*
        var inspectorWindowArray = TryGetInspectorWindows();
        if (inspectorWindowArray.Length == 0) return;

        foreach (var inspectorWindow in inspectorWindowArray)
        {
            AddCustomButton(inspectorWindow);
        }
        */

        RemoveInjections();

        if (windowsDic != null)
        {
            foreach (KeyValuePair<VisualElement, EditorWindow> pair in windowsDic)
            {
                if (!pair.Value.rootVisualElement.Contains(pair.Key))
                {
                    pair.Value.rootVisualElement.Add(pair.Key);
                    references = references.CreateAdd(pair.Key, pair.Value);
                }
            }
            windowsDic.Clear();
        }

        EditorApplication.delayCall += OnUpdate;
    }

    public static void RemoveInjections()
    {
        if (references != null)
        {
            List<VisualElement> toRemove = new List<VisualElement>();
            foreach (KeyValuePair<VisualElement, EditorWindow> pair in references)
                if ((windowsDic == null) || !windowsDic.ContainsKey(pair.Key))
                    toRemove.Add(pair.Key);
            foreach (VisualElement ve in toRemove)
            {
                references[ve].rootVisualElement.Remove(ve);
                references.Remove(ve);
            }
        }
    }

    public static void AddVisualElement(VisualElement element, EditorWindow window)
    {
        windowsDic = windowsDic.CreateAdd(element, window);
    }

    struct WindowReference
    {
        public EditorWindow window;
        public object reference;

        public WindowReference(EditorWindow window, object reference)
        {
            this.window = window;
            this.reference = reference;
        }
    }
}

/*
private static void OnClick()
{
Debug.Log("Click.");
}

private static void AddCustomButton(EditorWindow editorWindow)
{
var addComponentButton = GetAddComponentButton(editorWindow.rootVisualElement);
if (addComponentButton == null || addComponentButton.childCount < 1) return;

var customButton = GetCustomButton(addComponentButton);
if (customButton != null) return;

var button = new Button(OnClick)
{
    text = CustomButtonText
};
button.AddToClassList(CustomButtonClassName);
var styleSheet = Resources.Load<StyleSheet>(CustomButtonStyleSheet);
if (styleSheet)
{
    button.styleSheets.Add(styleSheet);
}

addComponentButton.Add(button);
}

private static EditorWindow[] TryGetInspectorWindows()
{
    return Resources
        .FindObjectsOfTypeAll<EditorWindow>()
        .Where(window => window.rootVisualElement.Q(className: UnityInspectorClassName) != null)
        .ToArray();
}

private static VisualElement GetAddComponentButton(VisualElement rootVisualElement)
{
    return rootVisualElement
        .Q(className: AddComponentButtonClassName);
}

private static VisualElement GetCustomButton(VisualElement rootVisualElement)
{
    return rootVisualElement
        .Q(className: CustomButtonClassName);
}
*/
#endif