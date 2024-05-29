using System.Collections.Generic;
using System.ComponentModel;

public static class StringExtension_ToList
{
    public static string ToUsefulString<T>(this List<T> list)
    {
        string result = "";
        if (list.Count > 0)
        {
            result += list[0].ToString();
            for (int i = 1; i < list.Count; i++)
                result += "," + list[i].ToString();
        }
        return result;
    }

    public static string ToUsefulString<T>(this T[] array)
    {
        string result = "";
        if (array.Length > 0)
        {
            result += array[0].ToString();
            for (int i = 1; i < array.Length; i++)
                result += "," + array[i].ToString();
        }
        return result;
    }

    public static List<T> ToList<T>(this string source)
    {
        List<T> result = new List<T>();
        string[] split = source.Split(",");
        for (int i = 0; i < split.Length; i++)
        {
            try { result.Add((T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(split[i])); }
            catch { }
        }
        return result;
    }

    public static T[] ToArray<T>(this string source)
    {
        return source.ToList<T>().ToArray();
    }
}
