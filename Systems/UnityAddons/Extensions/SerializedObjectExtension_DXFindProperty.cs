#if UNITY_EDITOR
using UnityEditor;

public static class SerializedObjectExtension_DXFindProperty
{
    public static SerializedProperty DXFindPropertyRelative(this SerializedProperty prop, string propertyPath)
    {
        string[] pathSteps = propertyPath.PathSteps();
        return prop.DXFindPropertyRelative(pathSteps);
    }

    public static SerializedProperty DXFindPropertyRelative(this SerializedProperty prop, string[] pathSteps)
    {
        for (int i = 0; i < pathSteps.Length; i++)
        {
            string name = pathSteps[i].BreakDownArrayVariableName(out int index);

            SerializedObject obj = prop.SerializedObjectFromReference();
            if (obj != null) prop = obj.FindProperty(name);
            else prop = prop.FindPropertyRelative(name);

            if (index >= 0) prop = prop.GetArrayElementAtIndex(index);
        }

        return prop;
    }

    public static SerializedProperty DXFindProperty(this SerializedObject obj, string propertyPath)
    {
        string[] pathSteps = propertyPath.PathSteps();
        return obj.DXFindProperty(pathSteps);
    }

    public static SerializedProperty DXFindProperty(this SerializedObject obj, string[] pathSteps)
    {
        SerializedProperty pr = null;
        for (int i = 0; i < pathSteps.Length; i++)
        {
            string name = pathSteps[i].BreakDownArrayVariableName(out int index);

            if (pr != null)
            {
                obj = pr.SerializedObjectFromReference();
                if (obj != null) pr = null;
            }

            if (pr == null) pr = obj.FindProperty(name);
            else pr = pr.FindPropertyRelative(name);

            if (index >= 0) pr = pr.GetArrayElementAtIndex(index);
        }

        return pr;
    }
}
#endif
