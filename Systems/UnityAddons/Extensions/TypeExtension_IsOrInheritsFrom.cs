using System;

public static class TypeExtension_IsOrInheritsFrom
{
    public static bool IsOrInheritsFrom(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type);
    }
}
