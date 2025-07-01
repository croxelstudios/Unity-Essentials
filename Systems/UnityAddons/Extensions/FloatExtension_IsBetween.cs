public static class FloatExtension_IsBetween
{
    public static bool IsBetween(this float source, float min, float max,
        bool includeMin = false, bool includeMax = false)
    {
        bool minCheck = includeMin ? source >= min : source > min;
        bool maxCheck = includeMax ? source <= max : source < max;
        return minCheck && maxCheck;
    }

    public static bool IsBetween(this int source, float min, float max,
        bool includeMin = false, bool includeMax = false)
    {
        bool minCheck = includeMin ? source >= min : source > min;
        bool maxCheck = includeMax ? source <= max : source < max;
        return minCheck && maxCheck;
    }

    public static bool IsBetween(this int source, int min, int max,
        bool includeMin = true, bool includeMax = false)
    {
        bool minCheck = includeMin ? source >= min : source > min;
        bool maxCheck = includeMax ? source <= max : source < max;
        return minCheck && maxCheck;
    }
}
