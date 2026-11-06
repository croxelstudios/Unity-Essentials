#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class SerializedPropertyExtension_SerializedObjectFromReference
{
    public static SerializedObject SerializedObjectFromReference(this SerializedProperty property)
    {
        if ((property != null) && (property.propertyType == SerializedPropertyType.ObjectReference))
        {
            int length = property.serializedObject.targetObjects.Length;
            List<Object> comps = new();
            for (int j = 0; j < length; j++)
            {
                SerializedObject obj = new(property.serializedObject.targetObjects[j]);

                SerializedProperty p = obj.FindProperty(property.propertyPath);

                Object comp = p.objectReferenceValue;

                if (comp != null) comps.Add(comp);
            }

            return new(comps.ToArray());
        }
        else return null;
    }
}
#endif
