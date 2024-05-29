public static class FloatExtension_IsBetween
{
    public static bool IsBetween(this float source, float min, float max,
        bool includeMin = false, bool includeMax = false)
    {
        bool minCheck = includeMin ? source >= min : source > min;
        bool maxCheck = includeMax ? source <= max : source < max;
        return minCheck && maxCheck;
    }
}
