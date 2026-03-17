#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class RemoveAddAttributes
{
    static Dictionary<string, List<Type>> toRemove;
    static Dictionary<string, List<Attribute>> toAdd;

    public static void Remove(SerializedProperty property, params Type[] attributes)
    {
        Remove(property.GetMemberInfo(), attributes);
    }

    public static void Remove(MemberInfo memberInfo, params Type[] attributes)
    {
        Remove(KeyFrom(memberInfo), attributes);
    }

    public static void Remove(string path, params Type[] attributes)
    {
        for (int i = 0; i < attributes.Length; i++)
            toRemove = toRemove.CreateAdd(path, attributes[i]);
    }

    public static void Add(SerializedProperty property, params Attribute[] attributes)
    {
        Add(property.GetMemberInfo(), attributes);
    }

    public static void Add(MemberInfo memberInfo, params Attribute[] attributes)
    {
        Add(KeyFrom(memberInfo), attributes);
    }

    public static void Add(string path, params Attribute[] attributes)
    {
        for (int i = 0; i < attributes.Length; i++)
            toAdd = toAdd.CreateAdd(path, attributes[i]);
    }

    public static void Restore(SerializedProperty property)
    {
        Restore(property.GetMemberInfo());
    }

    public static void Restore(MemberInfo memberInfo)
    {
        Restore(KeyFrom(memberInfo));
    }

    public static void Restore(SerializedProperty property, params Type[] attributes)
    {
        Restore(property.GetMemberInfo(), attributes);
    }

    public static void Restore(MemberInfo memberInfo, params Type[] attributes)
    {
        Restore(KeyFrom(memberInfo), attributes);
    }

    public static void Restore(SerializedProperty property, params Attribute[] attributes)
    {
        Restore(property.GetMemberInfo(), attributes);
    }

    public static void Restore(MemberInfo memberInfo, params Attribute[] attributes)
    {
        Restore(KeyFrom(memberInfo), attributes);
    }

    public static void Restore(string path)
    {
        toRemove.SmartRemove(path);
        toAdd.SmartRemove(path);
    }

    public static void Restore(string path, params Type[] attributes)
    {
        for (int i = 0; i < attributes.Length; i++)
            toRemove.SmartRemove(path, attributes[i]);
    }

    public static void Restore(string path, params Attribute[] attributes)
    {
        for (int i = 0; i < attributes.Length; i++)
            toAdd.SmartRemove(path, attributes[i]);
    }

    static string KeyFrom(MemberInfo member)
    {
        return member.DeclaringType.FullName + ":" + member.Name;
    }

    public static IEnumerable<Type> AttributesToRemove(MemberInfo member)
    {
        return AttributesToRemove(KeyFrom(member));
    }

    public static IEnumerable<Type> AttributesToRemove(string memberKey)
    {
        return toRemove[memberKey];
    }

    public static IEnumerable<Attribute> AttributesToAdd(MemberInfo member)
    {
        return AttributesToAdd(KeyFrom(member));
    }

    public static IEnumerable<Attribute> AttributesToAdd(string memberKey)
    {
        return toAdd[memberKey];
    }

    public static bool RemoveInPropertyPath(MemberInfo member)
    {
        return RemoveInPropertyPath(KeyFrom(member));
    }

    public static bool RemoveInPropertyPath(string memberKey)
    {
        return toRemove.NotNullContainsKey(memberKey);
    }

    public static bool AddInPropertyPath(MemberInfo member)
    {
        return AddInPropertyPath(KeyFrom(member));
    }

    public static bool AddInPropertyPath(string memberKey)
    {
        return toAdd.NotNullContainsKey(memberKey);
    }
}

#if ODIN_INSPECTOR
[ResolverPriority(-5)]
public class RemoveAddAttributesProcessor : OdinAttributeProcessor
{
    bool containsRemove;
    bool containsAdd;

    public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
    {
        containsRemove = RemoveAddAttributes.RemoveInPropertyPath(member);
        containsAdd = RemoveAddAttributes.AddInPropertyPath(member);
        return (member != null) && (containsRemove || containsAdd);
    }

    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member == null) return;

        if (containsRemove)
        {
            Type[] attTypes = RemoveAddAttributes.AttributesToRemove(member).ToArray();
            attributes.RemoveAll(a => attTypes.Contains(a.GetType()));
        }
        if (containsAdd)
        {
            Attribute[] atts = RemoveAddAttributes.AttributesToAdd(member).ToArray();
            foreach (Attribute att in atts)
                attributes.Add(att);
        }
    }
}
#endif
#endif
