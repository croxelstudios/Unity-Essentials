using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;

public static class CollectionExtensions
{
    #region Create If Null
    public static List<T> CreateIfNull<T>(this List<T> list)
    {
        if (list == null) list = new List<T>();
        return list;
    }

    public static Dictionary<T, Y> CreateIfNull<T, Y>(this Dictionary<T, Y> dict)
    {
        if (dict == null) dict = new Dictionary<T, Y>();
        return dict;
    }

    public static SortedDictionary<T, Y> CreateIfNull<T, Y>(this SortedDictionary<T, Y> dict)
    {
        if (dict == null) dict = new SortedDictionary<T, Y>();
        return dict;
    }
    #endregion

    #region Create Add (1 layer)
    public static List<T> CreateAdd<T>(this List<T> list, T element, bool testContains = true)
    {
        list = list.CreateIfNull();
        if ((!testContains) || (!list.Contains(element)))
            list.Add(element);
        return list;
    }

    public static Dictionary<T, Y> CreateAdd<T, Y>(this Dictionary<T, Y> dict, T key, Y value)
    {
        dict = dict.CreateIfNull();
        dict.TryAdd(key, value);
        return dict;
    }

    public static SortedDictionary<T, Y> CreateAdd<T, Y>(this SortedDictionary<T, Y> dict, T key, Y value)
    {
        dict = dict.CreateIfNull();
        dict.TryAdd(key, value);
        return dict;
    }
    #endregion

    #region Create Add (2 layers)
    public static Dictionary<T, List<Y>> CreateAdd<T, Y>(this Dictionary<T, List<Y>> dict,
        T key, Y value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(value);
        else dict.Add(key, new List<Y>().CreateAdd(value));
        return dict;
    }

    public static SortedDictionary<T, List<Y>> CreateAdd<T, Y>(this SortedDictionary<T, List<Y>> dict,
        T key, Y value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(value);
        else dict.Add(key, new List<Y>().CreateAdd(value));
        return dict;
    }
    #endregion

    #region Create Add (3 layers)
    public static Dictionary<T, Dictionary<Y, U>> CreateAdd<T, Y, U>(
        this Dictionary<T, Dictionary<Y, U>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new Dictionary<Y, U>().CreateAdd(key2, value));
        return dict;
    }

    public static Dictionary<T, SortedDictionary<Y, U>> CreateAdd<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, U>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new SortedDictionary<Y, U>().CreateAdd(key2, value));
        return dict;
    }

    public static SortedDictionary<T, Dictionary<Y, U>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, U>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new Dictionary<Y, U>().CreateAdd(key2, value));
        return dict;
    }

    public static SortedDictionary<T, SortedDictionary<Y, U>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, U>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new SortedDictionary<Y, U>().CreateAdd(key2, value));
        return dict;
    }
    #endregion

    #region Create Add (4 layers)
    public static Dictionary<T, Dictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this Dictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new Dictionary<Y, List<U>>().CreateAdd(key2, value));
        return dict;
    }

    public static Dictionary<T, SortedDictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new SortedDictionary<Y, List<U>>().CreateAdd(key2, value));
        return dict;
    }

    public static SortedDictionary<T, Dictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new Dictionary<Y, List<U>>().CreateAdd(key2, value));
        return dict;
    }

    public static SortedDictionary<T, SortedDictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new SortedDictionary<Y, List<U>>().CreateAdd(key2, value));
        return dict;
    }
    #endregion

    #region Create Add Range
    public static List<T> CreateAddRange<T>(this List<T> list,
        IEnumerable<T> elements, bool testContains = false)
    {
        list = list.CreateIfNull();

        if (elements.IsNullOrEmpty())
            return list;

        if (testContains)
        {
            foreach (T element in elements)
                if (!list.Contains(element))
                    list.Add(element);
        }
        else list.AddRange(elements);
        return list;
    }

    public static Dictionary<T, List<Y>> CreateAddRange<T, Y>(this Dictionary<T, List<Y>> dict,
        T key, IEnumerable<Y> values, bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(values, testContains);
        else dict.Add(key, new List<Y>().CreateAddRange(values, testContains));
        return dict;
    }

    public static SortedDictionary<T, List<Y>> CreateAddRange<T, Y>(
        this SortedDictionary<T, List<Y>> dict, T key, IEnumerable<Y> values, bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(values, testContains);
        else dict.Add(key, new List<Y>().CreateAddRange(values, testContains));
        return dict;
    }

    public static Dictionary<T, Dictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this Dictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(key2, values, testContains);
        else dict.Add(key, new Dictionary<Y, List<U>>().CreateAddRange(key2, values, testContains));
        return dict;
    }

    public static Dictionary<T, SortedDictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(key2, values, testContains);
        else dict.Add(key, new SortedDictionary<Y, List<U>>().CreateAddRange(key2, values, testContains));
        return dict;
    }

    public static SortedDictionary<T, Dictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(key2, values, testContains);
        else dict.Add(key, new Dictionary<Y, List<U>>().CreateAddRange(key2, values, testContains));
        return dict;
    }

    public static SortedDictionary<T, SortedDictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(key2, values, testContains);
        else dict.Add(key, new SortedDictionary<Y, List<U>>().CreateAddRange(key2, values, testContains));
        return dict;
    }
    #endregion

    #region Smart Remove
    public static void SmartRemove<T>(this List<T> list, T element)
    {
        if (list != null) list.Remove(element);
    }

    public static void SmartRemove<T, Y>(this Dictionary<T, Y> dict, T key)
    {
        if (dict != null) dict.Remove(key);
    }

    public static void SmartRemove<T, Y>(this Dictionary<T, List<Y>> dict, T key, Y element)
    {
        if (dict.SmartGetValue(key, out List<Y> list))
            list.Remove(element);
    }

    public static void SmartRemove<T, Y>(this SortedDictionary<T, Y> dict, T key)
    {
        if (dict != null) dict.Remove(key);
    }

    public static void SmartRemove<T, Y>(this SortedDictionary<T, List<Y>> dict, T key, Y element)
    {
        if (dict.SmartGetValue(key, out List<Y> list))
            list.Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, Dictionary<Y, U>> dict, T key, Y key2)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, U> d))
            d.SmartRemove(key2);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, U> d))
            d.SmartRemove(key2);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, Dictionary<Y, U>> dict,
        T key, Y key2)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, U> d))
            d.SmartRemove(key2);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, U> d))
            d.SmartRemove(key2);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, List<U>> d))
            d.SmartRemove(key2, element);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, List<U>> d))
            d.SmartRemove(key2, element);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, List<U>> d))
            d.SmartRemove(key2, element);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, List<U>> d))
            d.SmartRemove(key2, element);
    }
    #endregion

    #region Smart Clear
    public static void SmartClear<T>(this List<T> list)
    {
        if (list != null) list.Clear();
    }

    public static void SmartClear<T, Y>(this Dictionary<T, Y> dict)
    {
        if (dict != null) dict.Clear();
    }

    public static void SmartClear<T, Y>(this SortedDictionary<T, Y> dict)
    {
        if (dict != null) dict.Clear();
    }
    #endregion

    #region Smart GetValue
    public static bool SmartGetValue<T>(this List<T> list, int i, out T element)
    {
        if ((list != null) && i.IsBetween(0, list.Count))
        {
            element = list[i];
            return true;
        }
        else
        {
            element = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y>(this Dictionary<T, Y> dict, T key, out Y value)
    {
        if (dict != null)
            return dict.TryGetValue(key, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y>(this Dictionary<T, List<Y>> dict, T key, int i, out Y value)
    {
        if (dict.SmartGetValue(key, out List<Y> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y>(this SortedDictionary<T, Y> dict, T key, out Y value)
    {
        if (dict != null)
            return dict.TryGetValue(key, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y>(this SortedDictionary<T, List<Y>> dict,
        T key, int i, out Y value)
    {
        if (dict.SmartGetValue(key, out List<Y> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this Dictionary<T, Dictionary<Y, U>> dict,
        T key, Y key2, out U value)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, U> d))
            return d.SmartGetValue(key2, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this Dictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2, out U value)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, U> d))
            return d.SmartGetValue(key2, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this SortedDictionary<T, Dictionary<Y, U>> dict,
        T key, Y key2, out U value)
    {
        if (dict.SmartGetValue(key, out Dictionary<Y, U> d))
            return d.SmartGetValue(key2, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2, out U value)
    {
        if (dict.SmartGetValue(key, out SortedDictionary<Y, U> d))
            return d.SmartGetValue(key2, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this Dictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, int i, out U value)
    {
        if (dict.SmartGetValue(key, key2, out List<U> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this Dictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, int i, out U value)
    {
        if (dict.SmartGetValue(key, key2, out List<U> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this SortedDictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, int i, out U value)
    {
        if (dict.SmartGetValue(key, key2, out List<U> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, int i, out U value)
    {
        if (dict.SmartGetValue(key, key2, out List<U> list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }
    #endregion

    #region Not Null Contains
    public static bool NotNullContains<T>(this List<T> list, T element)
    {
        if (list == null) return false;
        return list.Contains(element);
    }

    public static bool NotNullContainsKey<T, Y>(this Dictionary<T, Y> dict, T key)
    {
        if (dict == null) return false;
        return dict.ContainsKey(key);
    }

    public static bool NotNullContainsKey<T, Y>(this SortedDictionary<T, Y> dict, T key)
    {
        if (dict == null) return false;
        return dict.ContainsKey(key);
    }
    #endregion

    #region ClearNulls
    public static T[] ClearNulls<T>(this T[] array) where T : Object
    {
        if (array == null)
            return null;

        List<T> newData = new List<T>(array);
        for (int i = newData.Count - 1; i >= 0; i--)
        {
            T item = newData[i];
            bool isNull;

            if (item is Object unityObj)
                isNull = unityObj == null;
            else isNull = item == null;

            if (isNull)
                newData.RemoveAt(i);
        }
        return newData.ToArray();
    }

    public static void ClearNulls<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            T item = list[i];
            bool isNull;

            if (item is Object unityObj)
                isNull = unityObj == null;
            else isNull = item == null;

            if (isNull)
                list.RemoveAt(i);
        }
    }

    /// <summary>
    /// Clears the dictionary of null key values.
    /// WARNING: Causes overhead on big dictionaries and GC when the dictionary actually has null values. Use with caution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static Dictionary<T, Y> ClearNulls<T, Y>(this Dictionary<T, Y> dict)
    {
        if (dict == null) return null;

        bool hasNulls = false;
        foreach (KeyValuePair<T, Y> pair in dict)
        {
            if (pair.Key == null)
            {
                hasNulls = true;
                break;
            }
        }

        if (hasNulls)
            return (from kv in dict
                where kv.Key != null
                select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
        else return dict;
    }
    #endregion

    #region ClearOrCreate
    public static List<T> ClearOrCreate<T>(this List<T> list)
    {
        if (list != null) list.Clear();
        else list = new List<T>();
        return list;
    }

    public static Dictionary<T, Y> ClearOrCreate<T, Y>(this Dictionary<T, Y> dict)
    {
        if (dict != null) dict.Clear();
        else dict = new Dictionary<T, Y>();
        return dict;
    }

    public static SortedDictionary<T, Y> ClearOrCreate<T, Y>(this SortedDictionary<T, Y> dict)
    {
        if (dict != null) dict.Clear();
        else dict = new SortedDictionary<T, Y>();
        return dict;
    }
    #endregion

    #region Array IndexOf
    public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, int, bool> match)
    {
        var offset = 0;
        foreach (var item in collection)
        {
            if (match(item, offset))
                return offset;

            offset++;
        }

        return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> match) =>
        collection.IndexOf((item, offset) => match(item));

    public static int IndexOf<T>(this IEnumerable<T> collection, T value) =>
        collection.IndexOf((item, offset) => item.Is(value));

    public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> collection, Func<T, int, bool> match)
    {
        var offset = 0;
        foreach (var item in collection)
        {
            if (match(item, offset))
                yield return offset;

            offset++;
        }
    }

    public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> match) =>
        collection.IndicesOf((item, offset) => match(item));

    public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> collection, T value) =>
        collection.IndicesOf((item, offset) => item.Is(value));

    public static bool Equals<T>(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);
    public static bool Is<T>(this T o, T x) => Equals(o, x);
    #endregion

    #region TryAdd
    public static List<T> TryAdd<T>(this List<T> list, T element, bool testContains = true)
    {
        if ((element != null) && ((!testContains) || (!list.Contains(element))))
            list.Add(element);
        return list;
    }

    public static List<T> TryAddRange<T>(this List<T> list,
        IEnumerable<T> elements, bool testContains = false)
    {
        if (elements.IsNullOrEmpty())
            return list;

        foreach (T element in elements)
            list.TryAdd(element, testContains);

        return list;
    }
    #endregion
}
