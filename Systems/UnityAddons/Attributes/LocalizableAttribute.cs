using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizableAttribute : PropertyAttribute
{

}

[Serializable]
public struct StringField : IEquatable<StringField>
{
    Component component;
    int number;
    [SerializeField]
    string path;
    public string name;

    public StringField(Component component, int number, string path)
    {
        this.component = component;
        this.number = number;
        this.path = path;
        name = component.GetType().Name + ((number > 0) ? "(" + number + ")" : "") + "." + path;
    }

    public string Get()
    {
        return ReflectionTools.GetValue<string>(component, path);
    }

    public void Set(string text)
    {
        ReflectionTools.SetValue(component, path, text);
    }

    public bool IsNull()
    {
        return component == null;
    }

    public override bool Equals(object other)
    {
        if (!(other is StringField)) return false;
        return Equals((StringField)other);
    }

    public bool Equals(StringField other)
    {
        return name == other.name;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }

    public static bool operator ==(StringField o1, StringField o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(StringField o1, StringField o2)
    {
        return !o1.Equals(o2);
    }
}

public class StringFields
{
    GameObject gameObj;
    List<StringField> stringMembers;
    Dictionary<Component, Vector2Int> components;
#if UNITY_EDITOR
    public static event GameObjectDelegate changesPublishedOn;
    public delegate void GameObjectDelegate(GameObject obj);
#endif

    static Dictionary<Type, int> auxCompNumber;

    public StringFields(GameObject gameObj)
    {
        this.gameObj = gameObj;
        stringMembers = null;
        components = null;
        ResetMembers();
#if UNITY_EDITOR
        ObjectChangeEvents.changesPublished -= ChangesPublished;
        ObjectChangeEvents.changesPublished += ChangesPublished;
        changesPublishedOn += ChangesPublishedOn;
#endif
    }

    public StringField[] Get()
    {
        return stringMembers.ToArray();
    }

    public void ResetMembers()
    {
        stringMembers = new List<StringField>();
        components = new Dictionary<Component, Vector2Int>();
        Component[] comps = gameObj.GetComponents<Component>();

        auxCompNumber = auxCompNumber.ClearOrCreate();
        foreach (Component comp in comps)
        {
            Type type = comp.GetType();
            int number = 0;
            if (auxCompNumber.ContainsKey(type))
                number = auxCompNumber[type];
            AddComponent(comp, number);
            auxCompNumber.Set(type, number + 1);
        }
    }

    public void AddComponent(Component component, int number = 0)
    {
        StringField[] members = component.GetLocalizables(number);
        components.Add(component, new Vector2Int(stringMembers.Count, stringMembers.Count + members.Length));
        stringMembers.AddRange(members);
    }

#if UNITY_EDITOR
    public void Dispose()
    {
        changesPublishedOn -= ChangesPublishedOn;
    }

    static void ChangesPublished(ref ObjectChangeEventStream stream)
    {
        for (int i = 0; i < stream.length; i++)
        {
            if (stream.GetEventType(i) == ObjectChangeKind.ChangeGameObjectStructure)
            {
                stream.GetChangeGameObjectStructureEvent(i, out ChangeGameObjectStructureEventArgs data);
                GameObject go = EditorUtility.EntityIdToObject(data.instanceId) as GameObject;
                changesPublishedOn?.Invoke(go);
            }
        }
    }

    void ChangesPublishedOn(GameObject obj)
    {
        if (obj == gameObj)
            ResetMembers();
    }
#endif
}

public static class GameObjectExtension_GetLocalizables
{
    static Dictionary<int, StringFields> localizables;
#if UNITY_EDITOR
    static event InstanceIDDelegate destroyedObject;
    delegate void InstanceIDDelegate(int instanceId);
#endif

    const BindingFlags FLAGS =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public static StringField[] GetLocalizables(this Component component, int number = 0)
    {
        return GetLocalizables(component, number, "", component.GetType().GetFields(FLAGS));
    }

    static StringField[] GetLocalizables(Component component, int number, string path, FieldInfo[] fields)
    {
        List<StringField> result = new List<StringField>();
        for (int i = 0; i < fields.Length; i++)
        {
            Type type = fields[i].FieldType;
            string thisPath = path + fields[i].Name;
            if (HasAttribute(fields[i]))
            {
                if (type == typeof(string))
                {
                    result.Add(new StringField(component, number, thisPath));
                    continue;
                }
                else if (type != typeof(string[])) Debug.LogError(
                        "LocalizableAttribute can only be applied to string members. Member " + fields[i].Name +
                        " in component " + component.name + " is not a string.");
            }
            if (ShouldRecurseInto(type))
                result.AddRange(
                    GetLocalizables(component, number, thisPath + ".", fields[i].FieldType.GetFields(FLAGS)));
        }
        return result.ToArray();
    }

    static bool HasAttribute(MemberInfo info)
    {
        return info.GetCustomAttribute<LocalizableAttribute>() != null;
    }

    static bool ShouldRecurseInto(Type type)
    {
        if (type == null) return false;
        if (!type.IsValueType) return false;
        if (type == typeof(string)) return false;
        if (type.IsPrimitive || type.IsEnum) return false;
        return true;
    }

    public static StringField[] GetLocalizables(this GameObject obj)
    {
        int instanceId = obj.GetInstanceID();

        if (localizables.NotNullContainsKey(instanceId))
        {
            StringField[] fields = localizables[instanceId].Get();
            bool cacheUsable = true;
            foreach (StringField field in fields)
                if (field.IsNull())
                {
                    localizables.Remove(instanceId);
                    cacheUsable = false;
                    break;
                }
            if (cacheUsable)
                return fields;
        }

        if (!localizables.NotNullContainsKey(instanceId))
        {
#if UNITY_EDITOR
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            ObjectChangeEvents.changesPublished += ChangesPublished;
            destroyedObject -= DestroyedGameObject;
            destroyedObject += DestroyedGameObject;
#endif
            localizables = localizables.CreateAdd(instanceId, new StringFields(obj));
        }

        return localizables[instanceId].Get();
    }

#if UNITY_EDITOR
    static void ChangesPublished(ref ObjectChangeEventStream stream)
    {
        for (int i = 0; i < stream.length; i++)
        {
            if (stream.GetEventType(i) == ObjectChangeKind.DestroyGameObjectHierarchy)
            {
                stream.GetDestroyGameObjectHierarchyEvent(i, out DestroyGameObjectHierarchyEventArgs e);
                destroyedObject?.Invoke(e.instanceId);
            }
        }
    }

    static void DestroyedGameObject(int instanceId)
    {
        foreach (KeyValuePair<int, StringFields> pair in localizables)
        {
            if (pair.Key == instanceId)
            {
                pair.Value.Dispose();
                localizables.Remove(instanceId);
                break;
            }
        }
    }
#endif
}
