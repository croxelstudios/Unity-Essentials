using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class GameObjectExtension_AddComponentCopy
{
    public static T AddComponentCopy<T>(this GameObject go, T toAdd, string[] exclusions, bool copyBaseValues = true) where T : Component
    {
        Type type = toAdd.GetType();
        Component comp = go.AddComponent(type);
        return comp.GetCopyOf(toAdd, exclusions, copyBaseValues);
    }

    public static T AddComponentCopy<T>(this GameObject go, T toAdd, bool copyBaseValues = true) where T : Component
    {
        return go.AddComponentCopy(toAdd, null, copyBaseValues);
    }

    public static T GetCopyOf<T>(this Component comp, T other,
        string[] exclusions, bool copyBaseValues = true) where T : Component
    {
        //try
        //{
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match

        Action<T, T> copier =
            MemberCopier<T>.GetCopier(
                new MemberCopier<T>.CopyOptions(type, exclusions, copyBaseValues));
        copier((T)comp, other);

        return comp as T;
        //}
        //catch
        //{
        //    return comp.GetSingleCopyOf(other, exclusions, copyBaseValues);
        //}
    }

    public static T GetCopyOf<T>(this Component comp, T other,
        bool copyBaseValues = true) where T : Component
    {
        return comp.GetCopyOf(other, null, copyBaseValues);
    }

    public static T GetSingleCopyOf<T>(this Component comp, T other,
        string[] exclusions, bool copyBaseValues = true) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match

        MemberInfo[] infos = type.GetPropFields(copyBaseValues);

        Dictionary<string, List<int>> listItems = exclusions.ProcessExclusions(
            out bool checkExclusions, out bool checkArrays);

        Type iListType = null;
        if (checkArrays)
            iListType = typeof(IList);

        foreach (MemberInfo info in infos)
            if (info.CanWrite() && !CreatesInstance(type, info) &&
                !(checkExclusions && exclusions.Contains(info.Name)))
            {
                if (checkArrays && listItems.ContainsKey(info.Name))
                {
                    if (!info.PropFieldType().IsOrInheritsFrom(iListType))
                        Debug.LogError(
                            "Property or field " + info.Name + " is not an array or list.");
                    else
                    {
                        IList trg = info.GetPropFieldValue(comp) as IList;
                        IList org = info.GetPropFieldValue(other) as IList;
                        for (int i = 0; i < listItems[info.Name].Count; i++)
                            trg[i] = org[i];
                    }
                }
                else info.CopyPropFieldValue(comp, other);
            }

        return comp as T;
    }

    public static class MemberCopier<T> where T : Component
    {
        static readonly Dictionary<CopyOptions, Action<T, T>> _cache =
            new Dictionary<CopyOptions, Action<T, T>>();

        public static Action<T, T> GetCopier(CopyOptions copyOptions)
        {
            if (!_cache.TryGetValue(copyOptions, out Action<T, T> copier))
            {
                copier = Build(copyOptions);
                _cache.Add(copyOptions, copier);
            }
            return copier;
        }

        static Action<T, T> Build(CopyOptions copyOptions)
        {
            Type type = typeof(T);

            ParameterExpression targetParam = Expression.Parameter(type, "target");
            ParameterExpression sourceParam = Expression.Parameter(type, "source");
            List<Expression> exprs = new List<Expression>();
            type = copyOptions.type;
            ParameterExpression target = Expression.Variable(type, "target");
            ParameterExpression source = Expression.Variable(type, "source");
            exprs.Add(Expression.Assign(target, Expression.Convert(targetParam, type)));
            exprs.Add(Expression.Assign(source, Expression.Convert(sourceParam, type)));

            MemberInfo[] infos = type.GetPropFields(copyOptions.copyBaseValues);

            string[] exclusions = copyOptions.exclusions;
            Dictionary<string, List<int>> listItems = exclusions.ProcessExclusions(
                out bool checkExclusions, out bool checkArrays);

            Type iListType = null;
            MethodInfo getItem = null;
            MethodInfo setItem = null;

            if (checkArrays)
            {
                iListType = typeof(IList);
                getItem = iListType.GetProperty("Item").GetGetMethod();
                setItem = iListType.GetProperty("Item").GetSetMethod();
            }

            foreach (MemberInfo info in infos)
                if (info.CanWrite() && info.CanRead() && !CreatesInstance(type, info) &&
                    !(checkExclusions && exclusions.Contains(info.Name)))
                {
                    Expression trg = target.PropField(info);
                    Expression src = source.PropField(info);
                    if (checkArrays && listItems.ContainsKey(info.Name))
                    {
                        if (!info.PropFieldType().IsOrInheritsFrom(iListType))
                            Debug.LogError(
                                "Property or field " + info.Name + " is not an array or list.");
                        else
                            for (int i = 0; i < listItems[info.Name].Count; i++)
                            {
                                Expression index = Expression.Constant(listItems[info.Name][i]);
                                Expression setGetIndex =
                                    Expression.Call(target.PropField(info),
                                    setItem,
                                    index,
                                    Expression.Call(
                                        source.PropField(info), getItem, index)
                                    );
                                exprs.Add(setGetIndex);
                            }
                    }
                    else exprs.Add(Expression.Assign(trg, src));
                }

            Expression body = (exprs.Count == 0) ? Expression.Empty() :
                Expression.Block(new[] { target, source }, exprs);
            return Expression.Lambda<Action<T, T>>(body, targetParam, sourceParam).Compile();
        }

        public struct CopyOptions : IEquatable<CopyOptions>
        {
            public Type type;
            public readonly string[] exclusions;
            public readonly bool copyBaseValues;

            public CopyOptions(Type type, string[] exclusions, bool copyBaseValues = true)
            {
                this.type = type;
                this.exclusions = exclusions;
                this.copyBaseValues = copyBaseValues;
            }

            public override bool Equals(object other)
            {
                if (!(other is CopyOptions)) return false;
                return Equals((CopyOptions)other);
            }

            public bool Equals(CopyOptions other)
            {
                if (type != other.type)
                    return false;
                if (copyBaseValues != other.copyBaseValues)
                    return false;
                if (exclusions != null)
                {
                    if (exclusions.Length != other.exclusions.Length)
                        return false;
                    for (int i = 0; i < exclusions.Length; i++)
                        if (exclusions[i] != other.exclusions[i])
                            return false;
                }
                else if (other.exclusions != null)
                    return false;
                return true;
            }

            public override int GetHashCode()
            {
                int hash = type.GetHashCode();
                if (exclusions != null)
                    for (int i = 0; i < exclusions.Length; i++)
                        hash = hash * 31 + exclusions[i].GetHashCode();
                hash = hash * 31 + copyBaseValues.GetHashCode();
                return hash;
            }

            public static bool operator ==(CopyOptions o1, CopyOptions o2)
            {
                return o1.Equals(o2);
            }

            public static bool operator !=(CopyOptions o1, CopyOptions o2)
            {
                return !o1.Equals(o2);
            }
        }
    }

    static bool CreatesInstance(Type type, MemberInfo info)
    {
        if (info is PropertyInfo pInfo)
            return CreatesInstance(type, pInfo);
        return false;
    }

    static bool CreatesInstance(Type type, PropertyInfo info)
    {
        return (type.IsOrInheritsFrom(typeof(MeshFilter)) && (info.Name == "mesh")) ||
            (type.IsOrInheritsFrom(typeof(Renderer)) &&
            ((info.Name == "material") || (info.Name == "materials")));
    }

    static Dictionary<string, List<int>> ProcessExclusions(
        this string[] exclusions, out bool checkExclusions, out bool checkArrays)
    {
        checkExclusions = !exclusions.IsNullOrEmpty();
        checkArrays = false;
        Dictionary<string, List<int>> listItems = null;
        if (checkExclusions)
        {
            listItems = new Dictionary<string, List<int>>();
            for (int i = 0; i < exclusions.Length; i++)
                if (exclusions[i].Contains("["))
                {
                    int ibrack = exclusions[i].IndexOf("[");
                    int ibrack2 = exclusions[i].IndexOf("]");
                    string arrayName = exclusions[i].Substring(0, ibrack);
                    listItems.CreateAdd(arrayName,
                        int.Parse(exclusions[i].Substring(ibrack + 1, ibrack2 - ibrack - 1)));
                    checkArrays = true;
                }
        }
        return listItems;
    }

    public static string GetDebugView(this Expression expr)
    {
        PropertyInfo prop = typeof(Expression)
            .GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
        return prop.GetValue(expr) as string;
    }
}

