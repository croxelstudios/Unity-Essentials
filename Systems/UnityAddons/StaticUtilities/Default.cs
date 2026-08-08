using UnityEngine;
using System.Collections.Generic;
using System;

public static class Default<T>
{
    static readonly Dictionary<Type, object> overrides = new Dictionary<Type, object>
    {
        { typeof(Quaternion), Quaternion.identity }
    };

    public static readonly T Value;

    static Default()
    {
        if (overrides.TryGetValue(typeof(T), out var val))
            Value = (T)val;
        else
            Value = default;
    }
}

public static class Neutral<T>
{
    static readonly Dictionary<Type, object> overrides = new Dictionary<Type, object>
    {
        { typeof(float), 1f },
        { typeof(Vector2), Vector2.one },
        { typeof(Vector3), Vector3.one },
        { typeof(Vector4), Vector4.one },
        { typeof(Color), Color.white },
        { typeof(Quaternion), new Quaternion(1f, 1f, 1f, 1f) }
    };

    public static readonly T Value;

    static Neutral()
    {
        if (overrides.TryGetValue(typeof(T), out var val))
            Value = (T)val;
        else
            Value = default;
    }
}
