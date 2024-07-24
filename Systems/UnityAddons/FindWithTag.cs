using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class FindWithTag
{
    #region GameObject
    public static GameObject GameObject(params string[] tags)
    {
        GameObject result = null;
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
            {
                result = UnityEngine.GameObject.FindGameObjectWithTag(tags[i]);
                if (result != null) break;
            }
        return result;
    }

    public static GameObject[] GameObjects(params string[] tags)
    {
        List<GameObject> list = new List<GameObject>();
        if (tags != null)
            foreach (string tag in tags)
                list.AddRange(UnityEngine.GameObject.FindGameObjectsWithTag(tag));
        return list.ToArray();
    }

    public static GameObject GameObject(params string[][] tags)
    {
        GameObject result = null;
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
            {
                result = GameObject(tags[i]);
                if (result != null) break;
            }
        return result;
    }

    public static GameObject[] GameObjects(params string[][] tags)
    {
        List<GameObject> list = new List<GameObject>();
        if (tags != null)
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
        list.AddRange(GameObjects(extraTags));
        return list.ToArray();
    }
    #endregion

    #region Transform
    public static Transform Transform(params string[] tags)
    {
        GameObject result = GameObject(tags);
        return result.transform;
    }

    public static Transform[] Transforms(params string[] tags)
    {
        List<Transform> list = new List<Transform>();
        if (tags != null)
            foreach (string tag in tags)
            {
                GameObject[] objs = UnityEngine.GameObject.FindGameObjectsWithTag(tag);
                for (int i = 0; i < objs.Length; i++)
                    list.Add(objs[i].transform);
            }
        return list.ToArray();
    }

    public static Transform Transform(params string[][] tags)
    {
        Transform result = null;
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
            {
                result = Transform(tags[i]);
                if (result != null) break;
            }
        return result;
    }

    public static Transform[] Transforms(params string[][] tags)
    {
        List<Transform> list = new List<Transform>();
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
                list.AddRange(Transforms(tags[i]));
        return list.ToArray();
    }

    public static Transform Transform(string tag, params string[] extraTags)
    {
        Transform result = Transform(new string[] { tag });
        if (result == null) result = Transform(extraTags);
        return result;
    }

    public static Transform[] Transforms(string tag, params string[] extraTags)
    {
        List<Transform> list = new List<Transform>();
        list.AddRange(Transforms(new string[] { tag }));
        list.AddRange(Transforms(extraTags));
        return list.ToArray();
    }
    #endregion

    #region Component
    public static T Component<T>(params string[] tags) where T : Component
    {
        GameObject[] objs = GameObjects(tags);
        T component = null;
        if (objs != null)
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
        List<T> components = new List<T>();
        if (objs != null)
            foreach (GameObject obj in objs)
            {
                T component = obj.GetComponent<T>();
                if (component != null) components.Add(component);
            }
        return components.ToArray();
    }

    public static T Component<T>(params string[][] tags) where T : Component
    {
        T result = null;
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
            {
                result = Component<T>(tags[i]);
                if (result != null) break;
            }
        return result;
    }

    public static T[] Components<T>(params string[][] tags) where T : Component
    {
        List<T> list = new List<T>();
        if (tags != null)
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
        list.AddRange(Components<T>(extraTags));
        return list.ToArray();
    }
    #endregion

    #region OnlyEnabled
    public static T OnlyEnabled<T>(params string[] tags) where T : Behaviour
    {
        T[] objs = Components<T>(tags);
        T component = null;
        if (objs != null)
            foreach (T obj in objs)
            {
                if ((obj != null) && obj.enabled)
                {
                    component = obj;
                    break;
                }
            }
        return component;
    }

    public static T[] OnlyEnableds<T>(params string[] tags) where T : Behaviour
    {
        T[] objs = Components<T>(tags);
        List<T> components = new List<T>();
        if (objs != null)
            foreach (T obj in objs)
                if ((obj != null) && obj.enabled)
                    components.Add(obj);
        return components.ToArray();
    }

    public static T OnlyEnabled<T>(params string[][] tags) where T : Behaviour
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

    public static T[] OnlyEnableds<T>(params string[][] tags) where T : Behaviour
    {
        List<T> list = new List<T>();
        if (tags != null)
            for (int i = 0; i < tags.Length; i++)
                list.AddRange(OnlyEnableds<T>(tags[i]));
        return list.ToArray();
    }

    public static T OnlyEnabled<T>(string tag, params string[] extraTags) where T : Behaviour
    {
        T result = OnlyEnabled<T>(new string[] { tag });
        if (result == null) result = OnlyEnabled<T>(extraTags);
        return result;
    }

    public static T[] OnlyEnableds<T>(string tag, params string[] extraTags) where T : Behaviour
    {
        List<T> list = new List<T>();
        list.AddRange(OnlyEnableds<T>(new string[] { tag }));
        list.AddRange(OnlyEnableds<T>(extraTags));
        return list.ToArray();
    }
    #endregion
}

public enum ByTagUpdateMode { DontUpdate, UpdateWhenNull }
