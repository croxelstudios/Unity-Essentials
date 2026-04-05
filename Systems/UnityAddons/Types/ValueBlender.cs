using UnityEngine;
using System;
using System.Collections.Generic;

public class ValueBlender<T>
{
    public virtual int Count()
    {
        return 0;
    }

    public virtual T GetBlend()
    {
        return default;
    }

    public virtual void Dispose()
    {
    }
}

public class ValueBlender<C, K, T> : ValueBlender<T> where K : IEquatable<K>
{
    static Dictionary<K, List<ValueBlender<C, K, T>>> stackDictionary;

    K key;
    T value;
    BlendMode blendMode;

    public ValueBlender()
    {
    }

    public ValueBlender(K key, T value, BlendMode blendMode)
    {
        Register(key, value, blendMode);
    }

    public void Register(K key, T value, BlendMode blendMode)
    {
        Dispose();
        this.key = key;
        this.value = value;
        this.blendMode = blendMode;
        Register();
    }

    public override void Dispose()
    {
        stackDictionary.SmartRemoveClear(key, this);
    }

    public override int Count()
    {
        if (stackDictionary.TryGetValue(key, out List<ValueBlender<C, K, T>> list))
            return list.Count;
        else return 0;
    }

    public override T GetBlend()
    {
        T current = NeutralAdd();
        if (stackDictionary.TryGetValue(key, out List<ValueBlender<C, K, T>> list))
        {
            int count = list.Count;
            bool first = true;
            foreach (ValueBlender<C, K, T> setter in list)
            {
                if (first)
                {
                    setter.SetNeutral(ref current);
                    first = false;
                }
                setter.CombineValueIn(ref current, count);
            }
            return current;
        }
        else return current;
    }

    void Register()
    {
        stackDictionary = stackDictionary.CreateAdd(key, this);
    }

    void SetNeutral(ref T current)
    {
        switch (blendMode)
        {
            case BlendMode.Average:
                current = NeutralAdd();
                break;
            case BlendMode.Add:
                current = NeutralAdd();
                break;
            case BlendMode.Subtract:
                current = NeutralMult();
                break;
            default:
                current = NeutralMult();
                break;
        }
    }

    void CombineValueIn(ref T current, int count)
    {
        CombineValueIn(ref current, value, count);
    }

    void CombineValueIn(ref T current, T next, int count)
    {
        switch (blendMode)
        {
            case BlendMode.Average:
                current = Combine_Average(current, next, count);
                break;
            case BlendMode.Add:
                current = Combine_Add(current, next);
                break;
            case BlendMode.Subtract:
                current = Combine_Subtract(current, next);
                break;
            default:
                current = Combine_Multiply(current, next);
                break;
        }
    }

    protected virtual T NeutralAdd()
    {
        return Default<T>.Value;
    }

    protected virtual T NeutralMult()
    {
        return Neutral<T>.Value;
    }

    protected virtual T Combine_Average(T current, T next, int count)
    {
        return Generics.Add(current, Generics.Scale(next, 1f / count));
    }

    protected virtual T Combine_Multiply(T current, T next)
    {
        return Generics.Multiply(current, next);
    }

    protected virtual T Combine_Add(T current, T next)
    {
        return Generics.Add(current, next);
    }

    protected virtual T Combine_Subtract(T current, T next)
    {
        return Generics.Subtract(current, next);
    }
}

public class ValueBlender_Color<C, K> : ValueBlender<C, K, Color> where K : IEquatable<K>
{
    protected override Color Combine_Average(Color current, Color next, int count)
    {
        float alpha = current.a;
        current += (next / count);
        current.a = alpha * next.a;
        return current;
    }

    protected override Color Combine_Add(Color current, Color next)
    {
        float alpha = current.a;
        current += next;
        current.a = alpha * next.a;
        return current;
    }

    protected override Color Combine_Subtract(Color current, Color next)
    {
        float alpha = current.a;
        current -= next;
        current.a = alpha * next.a;
        return current;
    }
}

public static class ValueBlender_Extension
{
    public static ValueBlender<T> Set<C, K, T>(this ValueBlender<T> blender,
        C source, K key, T value, BlendMode blendMode) where K : IEquatable<K>
    {
        if (value is Color color)
            return (ValueBlender<T>)(object)(blender as ValueBlender<Color>).
                Set(source, key, color, blendMode);

        ValueBlender<C, K, T> bl;
        if (blender == null)
            blender = bl = new ValueBlender<C, K, T>();
        else bl = blender as ValueBlender<C, K, T>;
        bl.Register(key, value, blendMode);
        return blender;
    }

    static ValueBlender<Color> Set<C, K>(this ValueBlender<Color> blender,
        C source, K key, Color value, BlendMode blendMode) where K : IEquatable<K>
    {
        ValueBlender_Color<C, K> bl;
        if (blender == null)
            blender = bl = new ValueBlender_Color<C, K>();
        else bl = blender as ValueBlender_Color<C, K>;
        bl.Register(key, value, blendMode);
        return blender;
    }

    //WARNING: Doesn't work?
    //public static void TryDispose<T>(this ValueBlender<T> blender)
    //{
    //    if (blender != null)
    //        blender.Dispose();
    //}
}

public enum BlendMode { Multiply, Average, Add, Subtract }
