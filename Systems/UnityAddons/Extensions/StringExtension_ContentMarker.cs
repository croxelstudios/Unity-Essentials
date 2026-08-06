using UnityEngine;

public static class StringExtension_ContentMarker
{
    public static string ContentMarker(this string original, params object[] source)
    {
        if (source.IsNullOrEmpty())
            return original;
        else
        {
            bool hasContent = false;
            foreach (object obj in source)
            {
                switch (obj)
                {
                    case Wrapper w:
                        if (w != null) hasContent = true;
                        break;
                    default:
                        if (obj != null) hasContent = true;
                        break;
                }
                if (hasContent) break;
            }

            return hasContent ? original + " ●" : original;
        }
    }
}
