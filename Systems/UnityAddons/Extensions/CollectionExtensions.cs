using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

public static class CollectionExtensions
{
    #region Create Add (1 layer)
    public static L CreateAdd<L, T>(this L list, T element, bool testContains = true)
        where L : ICollection<T>, new()
    {
        list = list.CreateIfNull();
        if ((!testContains) || (!list.Contains(element)))
            list.Add(element);
        return list;
    }

    public static D CreateAdd<D, T, Y>(this D dict, T key, Y value) where D : IDictionary<T, Y>, new()
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
        return CreateAdd<Dictionary<T, List<Y>>, List<Y>, T, Y>(dict, key, value);
    }

    public static SortedDictionary<T, List<Y>> CreateAdd<T, Y>(this SortedDictionary<T, List<Y>> dict,
        T key, Y value)
    {
        return CreateAdd<SortedDictionary<T, List<Y>>, List<Y>, T, Y>(dict, key, value);
    }

    public static SortedList<T, List<Y>> CreateAdd<T, Y>(this SortedList<T, List<Y>> dict,
        T key, Y value)
    {
        return CreateAdd<SortedList<T, List<Y>>, List<Y>, T, Y>(dict, key, value);
    }

    public static D CreateAdd<D, L, T, Y>(this D dict,
        T key, Y value) where D : IDictionary<T, L>, new() where L : ICollection<Y>, new()
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(value);
        else dict.Add(key, new L().CreateAdd(value));
        return dict;
    }
    #endregion

    #region Create Add (3 layers)
    public static Dictionary<T, Dictionary<Y, U>> CreateAdd<T, Y, U>(
        this Dictionary<T, Dictionary<Y, U>> dict, T key, Y key2, U value)
    {
        return CreateAdd<Dictionary<T, Dictionary<Y, U>>, Dictionary<Y, U>, T, Y, U>
            (dict, key, key2, value);
    }

    public static Dictionary<T, SortedDictionary<Y, U>> CreateAdd<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, U>> dict, T key, Y key2, U value)
    {
        return CreateAdd<Dictionary<T, SortedDictionary<Y, U>>, SortedDictionary<Y, U>, T, Y, U>
            (dict, key, key2, value);
    }

    public static SortedDictionary<T, Dictionary<Y, U>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, U>> dict, T key, Y key2, U value)
    {
        return CreateAdd<SortedDictionary<T, Dictionary<Y, U>>, Dictionary<Y, U>, T, Y, U>
            (dict, key, key2, value);
    }

    public static SortedDictionary<T, SortedDictionary<Y, U>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, U>> dict, T key, Y key2, U value)
    {
        return CreateAdd<SortedDictionary<T, SortedDictionary<Y, U>>, SortedDictionary<Y, U>, T, Y, U>
            (dict, key, key2, value);
    }

    public static D CreateAdd<D, F, T, Y, U>(this D dict,
        T key, Y key2, U value) where D : IDictionary<T, F>, new() where F : IDictionary<Y, U>, new()
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd(key2, value);
        else dict.Add(key, new F().CreateAdd(key2, value));
        return dict;
    }
    #endregion

    #region Create Add (4 layers)
    public static Dictionary<T, Dictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this Dictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        return CreateAdd<Dictionary<T, Dictionary<Y, List<U>>>,
            Dictionary<Y, List<U>>, List<U>, T, Y, U>(dict, key, key2, value);
    }

    public static Dictionary<T, SortedDictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        return CreateAdd<Dictionary<T, SortedDictionary<Y, List<U>>>,
            SortedDictionary<Y, List<U>>, List<U>, T, Y, U>(dict, key, key2, value);
    }

    public static SortedDictionary<T, Dictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        return CreateAdd<SortedDictionary<T, Dictionary<Y, List<U>>>,
            Dictionary<Y, List<U>>, List<U>, T, Y, U>(dict, key, key2, value);
    }

    public static SortedDictionary<T, SortedDictionary<Y, List<U>>> CreateAdd<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, U value)
    {
        return CreateAdd<SortedDictionary<T, SortedDictionary<Y, List<U>>>,
            SortedDictionary<Y, List<U>>, List<U>, T, Y, U>(dict, key, key2, value);
    }

    public static D CreateAdd<D, F, L, T, Y, U>(this D dict,
        T key, Y key2, U value) where D : IDictionary<T, F>, new() where F : IDictionary<Y, L>, new()
        where L : ICollection<U>, new()
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAdd<F, L, Y, U>(key2, value);
        else dict.Add(key, new F().CreateAdd<F, L, Y, U>(key2, value));
        return dict;
    }
    #endregion

    #region Create Add Range
    //public static List<T> CreateAddRange<T>(this List<T> list,
    //    IEnumerable<T> elements, bool testContains = false)
    //{
    //    return CreateAddRange<List<T>, T>(list, elements, testContains);
    //}

    //public static L CreateAddRange<L, T>(this L list,
    //    IEnumerable<T> elements, bool testContains = false) where L : IList, new()
    //{
    //    list = list.CreateIfNull();

    //    if (elements.IsNullOrEmpty())
    //        return list;

    //    if (list is ICollection<T> collection)
    //    {
    //        if (testContains)
    //        {
    //            foreach (T element in elements)
    //                if (!collection.Contains(element))
    //                    collection.Add(element);
    //        }
    //        else collection.AddRange(elements);
    //    }
    //    else throw new ArgumentException("List doesn't have defined type.");
    //    return list;
    //}

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

    public static D CreateAddRange<D, T, Y>(this D dict,
        T key, IEnumerable<Y> values, bool testContains = false)
        where D : IDictionary<T, List<Y>>, new()
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) CreateAddRange(dict[key], values, testContains);
        else dict.Add(key, CreateAddRange(new List<Y>(), values, testContains));
        return dict;
    }

    public static Dictionary<T, Dictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this Dictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        return CreateAddRange<Dictionary<T, Dictionary<Y, List<U>>>, Dictionary<Y, List<U>>, T, Y, U>
            (dict, key, key2, values, testContains);
    }

    public static Dictionary<T, SortedDictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this Dictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        return CreateAddRange<Dictionary<T, SortedDictionary<Y, List<U>>>, SortedDictionary<Y, List<U>>, T, Y, U>
            (dict, key, key2, values, testContains);
    }

    public static SortedDictionary<T, SortedDictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        return CreateAddRange<SortedDictionary<T, SortedDictionary<Y, List<U>>>, SortedDictionary<Y, List<U>>, T, Y, U>
            (dict, key, key2, values, testContains);
    }

    public static SortedDictionary<T, Dictionary<Y, List<U>>> CreateAddRange<T, Y, U>(
        this SortedDictionary<T, Dictionary<Y, List<U>>> dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
    {
        return CreateAddRange<SortedDictionary<T, Dictionary<Y, List<U>>>, Dictionary<Y, List<U>>, T, Y, U>
            (dict, key, key2, values, testContains);
    }

    public static D CreateAddRange<D, F, T, Y, U>(
        this D dict, T key, Y key2, IEnumerable<U> values,
        bool testContains = false)
        where D : IDictionary<T, F>, new() where F : IDictionary<Y, List<U>>, new()
    {
        dict = dict.CreateIfNull();
        if (dict.ContainsKey(key)) dict[key].CreateAddRange(key2, values, testContains);
        else dict.Add(key, new F().CreateAddRange(key2, values, testContains));
        return dict;
    }
    #endregion

    #region Smart Remove
    public static void SmartRemove<T>(this ICollection<T> list, T element)
    {
        if (list != null) list.Remove(element);
    }

    public static void SmartRemove<T, Y>(this IDictionary<T, Y> dict, T key)
    {
        if (dict != null) dict.Remove(key);
    }

    public static void SmartRemove<L, T, Y>(this IDictionary<T, L> dict, T key, Y element) where L : IList<Y>
    {
        if (dict.SmartGetValue(key, out L list))
            list.Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this IDictionary<T, Dictionary<Y, U>> dict, T key, Y key2)
    {
        SmartRemove<Dictionary<Y, U>, T, Y, U>(dict, key, key2);
    }

    public static void SmartRemove<T, Y, U>(this IDictionary<T, SortedDictionary<Y, U>> dict, T key, Y key2)
    {
        SmartRemove<SortedDictionary<Y, U>, T, Y, U>(dict, key, key2);
    }

    public static void SmartRemove<T, Y, U>(this IDictionary<T, SortedList<Y, U>> dict, T key, Y key2)
    {
        SmartRemove<SortedList<Y, U>, T, Y, U>(dict, key, key2);
    }

    public static void SmartRemove<D, T, Y, U>(this IDictionary<T, D> dict, T key, Y key2)
        where D : IDictionary<Y, U>
    {
        if (dict.SmartGetValue(key, out D d))
            d.SmartRemove(key2);
    }

    public static void SmartRemove<D, L, T, Y, U>(this IDictionary<T, D> dict,
        T key, Y key2, U element) where D : IDictionary<Y, L> where L : IList<U>
    {
        if (dict.SmartGetValue(key, out D d))
            d.SmartRemove(key2, element);
    }
    #endregion

    #region Smart GetValue
    public static bool SmartGetValue<T>(this IList<T> list, int i, out T element)
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

    public static bool SmartGetValue<T, Y>(this IDictionary<T, Y> dict, T key, out Y value)
    {
        if (dict != null)
            return dict.TryGetValue(key, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<L, T, Y>(this IDictionary<T, L> dict, T key, int i, out Y value)
        where L : IList<Y>
    {
        if (dict.SmartGetValue(key, out L list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<D, T, Y, U>(this IDictionary<T, D> dict,
        T key, Y key2, out U value) where D : IDictionary<Y, U>
    {
        if (dict.SmartGetValue(key, out D d))
            return d.SmartGetValue(key2, out value);
        else
        {
            value = default;
            return false;
        }
    }

    public static bool SmartGetValue<D, L, T, Y, U>(this IDictionary<T, D> dict,
        T key, Y key2, int i, out U value) where D : IDictionary<Y, L> where L : IList<U>
    {
        if (dict.SmartGetValue(key, key2, out L list))
            return list.SmartGetValue(i, out value);
        else
        {
            value = default;
            return false;
        }
    }
    #endregion

    #region Not Null Contains
    public static bool NotNullContains<T>(this T[] array, T element)
    {
        if (array == null) return false;
        return array.Contains(element);
    }

    public static bool NotNullContains<T>(this ICollection<T> list, T element)
    {
        if (list == null) return false;
        return list.Contains(element);
    }

    public static bool NotNullContainsKey<T, Y>(this IDictionary<T, Y> dict, T key)
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

    public static void ClearNulls<T>(this IList<T> list)
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
    /// WARNING: Causes overhead on big dictionaries and GC when the dictionary actually has null values.
    /// Use with caution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static Dictionary<T, Y> ClearNulls<T, Y>(this Dictionary<T, Y> dict)
    {
        return ClearNulls<Dictionary<T, Y>, T, Y>(dict);
    }

    /// <summary>
    /// Clears the dictionary of null key values.
    /// WARNING: Causes overhead on big dictionaries and GC when the dictionary actually has null values.
    /// Use with caution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static SortedDictionary<T, Y> ClearNulls<T, Y>(this SortedDictionary<T, Y> dict)
    {
        return ClearNulls<SortedDictionary<T, Y>, T, Y>(dict);
    }

    /// <summary>
    /// Clears the dictionary of null key values.
    /// WARNING: Causes overhead on big dictionaries and GC when the dictionary actually has null values.
    /// Use with caution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static SortedList<T, Y> ClearNulls<T, Y>(this SortedList<T, Y> dict)
    {
        return ClearNulls<SortedList<T, Y>, T, Y>(dict);
    }

    /// <summary>
    /// Clears the dictionary of null key values.
    /// WARNING: Causes overhead on big dictionaries and GC when the dictionary actually has null values.
    /// Use with caution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static D ClearNulls<D, T, Y>(this D dict) where D : class, IDictionary<T, Y>, new()
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
        {
            //return (from kv in dict
            //    where kv.Key != null
            //    select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            D newDict = new D();
            foreach (KeyValuePair<T, Y> pair in dict)
                if (pair.Key != null)
                    newDict.Add(pair.Key, pair.Value);
            return newDict;
        }
        else return dict;
    }
    #endregion

    #region ClearOrCreate
    public static List<T> ClearOrCreate<T>(this List<T> list)
    {
        return ClearOrCreate<List<T>, T>(list);
    }

    public static Dictionary<T, Y> ClearOrCreate<T, Y>(this Dictionary<T, Y> list)
    {
        return ClearOrCreate<Dictionary<T, Y>, KeyValuePair<T, Y>>(list);
    }

    public static SortedDictionary<T, Y> ClearOrCreate<T, Y>(this SortedDictionary<T, Y> list)
    {
        return ClearOrCreate<SortedDictionary<T, Y>, KeyValuePair<T, Y>>(list);
    }

    public static SortedList<T, Y> ClearOrCreate<T, Y>(this SortedList<T, Y> list)
    {
        return ClearOrCreate<SortedList<T, Y>, KeyValuePair<T, Y>>(list);
    }

    public static L ClearOrCreate<L, T>(this L list) where L : ICollection<T>, new()
    {
        if (list != null) list.Clear();
        else list = new L();
        return list;
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
    public static L TryAdd<L, T>(this L list, T element, bool testContains = true) where L : ICollection<T>
    {
        if ((element != null) && ((!testContains) || (!list.Contains(element))))
            list.Add(element);
        return list;
    }

    public static L TryAddRange<L, T>(this L list,
        IEnumerable<T> elements, bool testContains = false) where L : ICollection<T>
    {
        if (elements.IsNullOrEmpty())
            return list;

        foreach (T element in elements)
            list.TryAdd(element, testContains);

        return list;
    }
    #endregion

    public static T CreateIfNull<T>(this T list) where T : new()
    {
        if (list == null) list = new T();
        return list;
    }

    public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> items)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (items == null) throw new ArgumentNullException(nameof(items));

        if (target is List<T> list)
        {
            list.AddRange(items);
            return;
        }

        foreach (var it in items)
            target.Add(it);
    }

    public static void SmartClear<T>(this ICollection<T> list)
    {
        if (list != null) list.Clear();
    }

    public static void Set<T, Y>(this IDictionary<T, Y> dict, T key, Y value)
    {
        if (!dict.ContainsKey(key))
            dict.Add(key, value);
        else dict[key] = value;
    }
}
