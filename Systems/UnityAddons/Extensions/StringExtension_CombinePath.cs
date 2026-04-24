public static class StringExtension_CombinePath
{
    public static string CombinePath(this string original, params string[] subpaths)
    {
        for (int i = 0; i < subpaths.Length; i++)
            original = CombinePath_Internal(original, subpaths[i]);
        return original;
    }

    static string CombinePath_Internal(string left, string right)
    {
        left = (left ?? string.Empty).Replace("\\", "/").TrimEnd('/');
        right = (right ?? string.Empty).Replace("\\", "/").TrimStart('/');

        if (string.IsNullOrEmpty(left))
            return right;

        if (string.IsNullOrEmpty(right))
            return left;

        return $"{left}/{right}";
    }
}
