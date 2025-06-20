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
