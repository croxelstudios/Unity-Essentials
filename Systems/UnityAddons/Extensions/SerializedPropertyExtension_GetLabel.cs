#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class SerializedPropertyExtension_GetLabel
{
    public static string GetLabel(this SerializedProperty property)
    {
        string label = property.displayName;
#if ODIN_INSPECTOR
        MemberInfo member = property.GetMemberInfo();
        Attribute[] atts = member?.GetCustomAttributes(true) as Attribute[];
        LabelTextAttribute ltAtt =
            atts.FirstOrDefault(a => a.GetType() == typeof(LabelTextAttribute)) as LabelTextAttribute;
        if (ltAtt != null)
            label = ltAtt.Text;
#endif
        return label;
    }
}
#endif
