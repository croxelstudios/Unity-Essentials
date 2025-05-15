using System.Collections;
using System.Collections.Generic;

public static class ListExtension_CreateAdd
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
        if (!dict.ContainsKey(key))
            dict.Add(key, value);
        return dict;
    }

    public static SortedDictionary<T, Y> CreateAdd<T, Y>(this SortedDictionary<T, Y> dict, T key, Y value)
    {
        dict = dict.CreateIfNull();
        if (!dict.ContainsKey(key))
            dict.Add(key, value);
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
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(element);
    }

    public static void SmartRemove<T, Y>(this SortedDictionary<T, Y> dict, T key)
    {
        if (dict != null) dict.Remove(key);
    }

    public static void SmartRemove<T, Y>(this SortedDictionary<T, List<Y>> dict, T key, Y element)
    {
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, Dictionary<Y, U>> dict, T key, Y key2)
    {
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(key2);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2)
    {
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(key2);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, Dictionary<Y, U>> dict,
        T key, Y key2)
    {
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(key2);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, U>> dict,
        T key, Y key2)
    {
        if ((dict != null) && dict.ContainsKey(key))
            dict[key].Remove(key2);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if ((dict != null) && dict.ContainsKey(key) && dict[key].ContainsKey(key2))
            dict[key][key2].Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this Dictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if ((dict != null) && dict.ContainsKey(key) && dict[key].ContainsKey(key2))
            dict[key][key2].Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, Dictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if ((dict != null) && dict.ContainsKey(key) && dict[key].ContainsKey(key2))
            dict[key][key2].Remove(element);
    }

    public static void SmartRemove<T, Y, U>(this SortedDictionary<T, SortedDictionary<Y, List<U>>> dict,
        T key, Y key2, U element)
    {
        if ((dict != null) && dict.ContainsKey(key) && dict[key].ContainsKey(key2))
            dict[key][key2].Remove(element);
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
}
