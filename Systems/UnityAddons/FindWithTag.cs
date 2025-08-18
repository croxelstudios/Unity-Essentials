using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

public static class FindWithTag
{
    #region Any
    public static T Any<T>(params string[] tags) where T : Object
    {
        const string name = "Component";
        const string parameter = "string[]";

        if (typeof(T) == typeof(GameObject))
            return GameObject(tags) as T;

        if (typeof(T) == typeof(Transform))
            return Transform(tags) as T;

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string[]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T));

        object result = genericMethod.Invoke(
                null,
                new object[] { tags }
            );
        return (T)result;
    }

    public static T Anys<T>(params string[] tags) where T : Object
    {
        const string name = "Components";
        const string parameter = "string[]";

        if (typeof(T) == typeof(GameObject))
            return GameObjects(tags) as T;

        if (typeof(T) == typeof(Transform))
            return Transforms(tags) as T;

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string[]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T));

        object result = genericMethod.Invoke(
                null,
                new object[] { tags }
            );
        return (T)result;
    }

    public static T Any<T>(params string[][] tags) where T : Object
    {
        const string name = "Component";
        const string parameter = "string[][]";

        if (typeof(T) == typeof(GameObject))
            return GameObject(tags) as T;

        if (typeof(T) == typeof(Transform))
            return Transform(tags) as T;

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string[][]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T));

        object result = genericMethod.Invoke(
                null,
                new object[] { tags }
            );
        return (T)result;
    }

    public static T Anys<T>(params string[][] tags) where T : Object
    {
        const string name = "Components";
        const string parameter = "string[][]";

        if (typeof(T) == typeof(GameObject))
            return GameObjects(tags) as T;

        if (typeof(T) == typeof(Transform))
            return Transforms(tags) as T;

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string[][]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T));

        object result = genericMethod.Invoke(
                null,
                new object[] { tags }
            );
        return (T)result;
    }

    public static T Any<T>(string tag, params string[] extraTags) where T : Object
    {
        const string name = "Component";
        const string parameter = "string, string[]";

        if (typeof(T) == typeof(GameObject))
            return GameObject(tag, extraTags) as T;

        if (typeof(T) == typeof(Transform))
            return Transform(tag, extraTags) as T;

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(string[]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T));

        object result = genericMethod.Invoke(
                null,
                new object[] { tag, extraTags }
            );
        return (T)result;
    }

    public static T[] Anys<T>(string tag, params string[] extraTags) where T : Object
    {
        const string name = "Components";
        const string parameter = "string, string[]";

        if (typeof(T) == typeof(GameObject))
            return GameObjects(tag, extraTags) as T[];

        if (typeof(T) == typeof(Transform))
            return Transforms(tag, extraTags) as T[];

        //Invoke Component method by reflection
        MethodInfo baseMethod = typeof(FindWithTag)
                .GetMethod(
                    name,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(string[]) },
                    null
                );
        if (baseMethod == null)
            throw new InvalidOperationException("FindWithTag." + name + "<T>(" + parameter + ") wasn't found.");
        MethodInfo genericMethod = baseMethod.MakeGenericMethod(typeof(T[]));

        object result = genericMethod.Invoke(
                null,
                new object[] { tag, extraTags }
            );
        return (T[])result;
    }
    #endregion

    #region GameObject
    public static GameObject GameObject(params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        GameObject result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = UnityEngine.GameObject.FindGameObjectWithTag(tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static GameObject[] GameObjects(params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<GameObject> list = new List<GameObject>();
        foreach (string tag in tags)
            list.AddRange(UnityEngine.GameObject.FindGameObjectsWithTag(tag));
        return list.ToArray();
    }

    public static GameObject GameObject(params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        GameObject result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = GameObject(tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static GameObject[] GameObjects(params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(GameObjects(tags[i]));
        return list.ToArray();
    }

    public static GameObject GameObject(string tag, params string[] extraTags)
    {
        GameObject result = GameObject(new string[] { tag });
        if (result == null) result = GameObject(extraTags);
        return result;
    }

    public static GameObject[] GameObjects(string tag, params string[] extraTags)
    {
        List<GameObject> list = new List<GameObject>();
        list.AddRange(GameObjects(new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(GameObjects(extraTags));
        return list.ToArray();
    }

    public static GameObject GoCheckEmpty(string tag)
    {
        GameObject target = null;
        if (tag != "") target = GameObject(tag);
        return target;
    }

    public static GameObject[] GosCheckEmpty(string tag)
    {
        GameObject[] targets = null;
        if (tag != "") targets = GameObjects(tag);
        return targets;
    }
    #endregion

    #region Transform
    public static Transform Transform(bool onlyParents, params string[] tags)
    {
        Transform[] result = Transforms(onlyParents, tags);
        return result.NullOrEmpty() ? null : result[0];
    }

    public static Transform[] Transforms(bool onlyParents, params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<Transform> list = new List<Transform>();
        foreach (string tag in tags)
        {
            GameObject[] objs = UnityEngine.GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < objs.Length; i++)
                list.Add(objs[i].transform);
        }
        Transform[] transforms = list.ToArray();
        return onlyParents ? transforms.RemoveChildren() : transforms;
    }

    public static Transform Transform(bool onlyParents, params string[][] tags)
    {
        Transform[] result = Transforms(onlyParents, tags);
        return result.NullOrEmpty() ? null : result[0];
    }

    public static Transform[] Transforms(bool onlyParents, params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<Transform> list = new List<Transform>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(Transforms(false, tags[i]));
        Transform[] transforms = list.ToArray();
        return onlyParents ? transforms.RemoveChildren() : transforms;
    }

    public static Transform Transform(string tag, bool onlyParents, params string[] extraTags)
    {
        Transform[] result = Transforms(tag, onlyParents, extraTags);
        return result.NullOrEmpty() ? null : result[0];
    }

    public static Transform[] Transforms(string tag, bool onlyParents, params string[] extraTags)
    {
        List<Transform> list = new List<Transform>();
        list.AddRange(Transforms(false, new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(Transforms(false, extraTags));
        Transform[] transforms = list.ToArray();
        return onlyParents ? transforms.RemoveChildren() : transforms;
    }

    public static Transform Transform(params string[] tags)
    {
        return Transform(true, tags);
    }

    public static Transform[] Transforms(params string[] tags)
    {
        return Transforms(true, tags);
    }

    public static Transform Transform(params string[][] tags)
    {
        return Transform(true, tags);
    }

    public static Transform[] Transforms(params string[][] tags)
    {
        return Transforms(true, tags);
    }

    public static Transform Transform(string tag, params string[] extraTags)
    {
        return Transform(tag, true, extraTags);
    }

    public static Transform[] Transforms(string tag, params string[] extraTags)
    {
        return Transforms(tag, true, extraTags);
    }

    public static Transform TrCheckEmpty(string tag, bool onlyParents = true)
    {
        Transform target = null;
        if (tag != "") target = Transform(tag, onlyParents);
        return target;
    }

    public static Transform[] TrsCheckEmpty(string tag, bool onlyParents = true)
    {
        Transform[] targets = null;
        if (tag != "") targets = Transforms(tag, onlyParents);
        return targets;
    }
    #endregion

    #region Component
    public static T Component<T>(params string[] tags) where T : Component
    {
        GameObject[] objs = GameObjects(tags);

        if (objs == null)
            return null;

        T component = null;
        foreach (GameObject obj in objs)
        {
            component = obj.GetComponent<T>();
            if (component != null) break;
        }
        return component;
    }

    public static T[] Components<T>(params string[] tags) where T : Component
    {
        GameObject[] objs = GameObjects(tags);

        if (objs == null)
            return null;

        List<T> components = new List<T>();
        foreach (GameObject obj in objs)
        {
            T component = obj.GetComponent<T>();
            if (component != null) components.Add(component);
        }
        return components.ToArray();
    }

    public static T Component<T>(params string[][] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        T result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = Component<T>(tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static T[] Components<T>(params string[][] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<T> list = new List<T>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(Components<T>(tags[i]));
        return list.ToArray();
    }

    public static T Component<T>(string tag, params string[] extraTags) where T : Component
    {
        T result = Component<T>(new string[] { tag });
        if (result == null) result = Component<T>(extraTags);
        return result;
    }

    public static T[] Components<T>(string tag, params string[] extraTags) where T : Component
    {
        List<T> list = new List<T>();
        list.AddRange(Components<T>(new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(Components<T>(extraTags));
        return list.ToArray();
    }

    public static T CompCheckEmpty<T>(string tag) where T : Component
    {
        T target = null;
        if (tag != "") target = Component<T>(tag);
        return target;
    }

    public static T[] CompsCheckEmpty<T>(string tag) where T : Component
    {
        T[] targets = null;
        if (tag != "") targets = Components<T>(tag);
        return targets;
    }
    #endregion

    #region OnlyEnabled
    public static T OnlyEnabled<T>(params string[] tags) where T : Component
    {
        T[] objs = Components<T>(tags);

        if (objs == null)
            return null;

        T component = null;
        foreach (T obj in objs)
        {
            if (obj.IsEnabled())
            {
                component = obj;
                break;
            }
        }
        return component;
    }

    public static T[] OnlyEnableds<T>(params string[] tags) where T : Component
    {
        T[] objs = Components<T>(tags);

        if (objs == null)
            return null;

        List<T> components = new List<T>();
        foreach (T obj in objs)
            if (obj.IsEnabled())
                components.Add(obj);
        return components.ToArray();
    }

    public static T OnlyEnabled<T>(params string[][] tags) where T : Component
    {
        T result = null;
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
            {
                result = OnlyEnabled<T>(tags[i]);
                if (result != null) break;
            }
        return result;
    }

    public static T[] OnlyEnableds<T>(params string[][] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<T> list = new List<T>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(OnlyEnableds<T>(tags[i]));
        return list.ToArray();
    }

    public static T OnlyEnabled<T>(string tag, params string[] extraTags) where T : Component
    {
        T result = OnlyEnabled<T>(new string[] { tag });
        if (result == null) result = OnlyEnabled<T>(extraTags);
        return result;
    }

    public static T[] OnlyEnableds<T>(string tag, params string[] extraTags) where T : Component
    {
        List<T> list = new List<T>();
        list.AddRange(OnlyEnableds<T>(new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(OnlyEnableds<T>(extraTags));
        return list.ToArray();
    }
    #endregion

    #region In children
    public static GameObject GameObjectInChildren(GameObject obj, params string[] tags)
    {
        return TransformInChildren(obj, tags).gameObject;
    }

    public static GameObject[] GameObjectsInChildren(GameObject obj, params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<GameObject> list = new List<GameObject>();

        Transform[] children = obj.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (tags.Contains(children[i].tag))
                list.Add(children[i].gameObject);
        }
        return list.ToArray();
    }

    public static GameObject GameObjectInChildren(GameObject obj, params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        GameObject result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = GameObjectInChildren(obj, tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static GameObject[] GameObjectsInChildren(GameObject obj, params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(GameObjectsInChildren(obj, tags[i]));
        return list.ToArray();
    }

    public static GameObject GameObjectInChildren(GameObject obj, string tag, params string[] extraTags)
    {
        GameObject result = GameObject(new string[] { tag });
        if (result == null) result = GameObjectInChildren(obj, extraTags);
        return result;
    }

    public static GameObject[] GameObjectsInChildren(GameObject obj, string tag, params string[] extraTags)
    {
        List<GameObject> list = new List<GameObject>();
        list.AddRange(GameObjectsInChildren(obj, new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(GameObjectsInChildren(obj, extraTags));
        return list.ToArray();
    }

    public static Transform TransformInChildren(GameObject obj, params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        Transform[] children = obj.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (tags.Contains(children[i].tag))
                return children[i];
        }
        return null;
    }

    public static Transform[] TransformsInChildren(GameObject obj, params string[] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<Transform> list = new List<Transform>();

        Transform[] children = obj.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (tags.Contains(children[i].tag))
                list.Add(children[i]);
        }
        return list.ToArray();
    }

    public static Transform TransformInChildren(GameObject obj, params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        Transform result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = TransformInChildren(obj, tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static Transform[] TransformsInChildren(GameObject obj, params string[][] tags)
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<Transform> list = new List<Transform>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(TransformsInChildren(obj, tags[i]));
        return list.ToArray();
    }

    public static Transform TransformInChildren(GameObject obj, string tag, params string[] extraTags)
    {
        Transform result = TransformInChildren(obj, new string[] { tag });
        if (result == null) result = TransformInChildren(obj, extraTags);
        return result;
    }

    public static Transform[] TransformsInChildren(GameObject obj, string tag, params string[] extraTags)
    {
        List<Transform> list = new List<Transform>();
        list.AddRange(TransformsInChildren(obj, new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(TransformsInChildren(obj, extraTags));
        return list.ToArray();
    }

    public static T ComponentInChildren<T>(GameObject obj, params string[] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        T[] children = obj.GetComponentsInChildren<T>();
        for (int i = 0; i < children.Length; i++)
        {
            if (tags.Contains(children[i].tag))
                return children[i];
        }
        return null;
    }

    public static T[] ComponentsInChildren<T>(GameObject obj, params string[] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<T> list = new List<T>();

        T[] children = obj.GetComponentsInChildren<T>();
        for (int i = 0; i < children.Length; i++)
        {
            if (tags.Contains(children[i].tag))
                list.Add(children[i]);
        }
        return list.ToArray();
    }

    public static T ComponentInChildren<T>(GameObject obj, params string[][] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        T result = null;
        for (int i = 0; i < tags.Length; i++)
        {
            result = ComponentInChildren<T>(obj, tags[i]);
            if (result != null) break;
        }
        return result;
    }

    public static T[] ComponentsInChildren<T>(GameObject obj, params string[][] tags) where T : Component
    {
        if (tags.IsNullOrEmpty())
            return null;

        List<T> list = new List<T>();
        for (int i = 0; i < tags.Length; i++)
            list.AddRange(ComponentsInChildren<T>(obj, tags[i]));
        return list.ToArray();
    }

    public static T ComponentInChildren<T>(GameObject obj, string tag, params string[] extraTags) where T : Component
    {
        T result = ComponentInChildren<T>(obj, new string[] { tag });
        if (result == null) result = ComponentInChildren<T>(obj, extraTags);
        return result;
    }

    public static T[] ComponentsInChildren<T>(GameObject obj, string tag, params string[] extraTags) where T : Component
    {
        List<T> list = new List<T>();
        list.AddRange(ComponentsInChildren<T>(obj, new string[] { tag }));
        if (!extraTags.IsNullOrEmpty()) list.AddRange(ComponentsInChildren<T>(obj, extraTags));
        return list.ToArray();
    }
    #endregion
}

public enum ByTagUpdateMode { DontUpdate, UpdateWhenNull, UpdateWhenNullOrInactive }
