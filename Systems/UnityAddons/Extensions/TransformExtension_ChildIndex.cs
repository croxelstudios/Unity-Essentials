using UnityEngine;

public static class TransformExtension_ChildIndex
{
    public static int ChildIndex(this Transform tr)
    {
        if (tr == null || tr.parent == null)
            return -1;

        for (int i = 0; i < tr.parent.childCount; i++)
            if (tr.parent.GetChild(i) == tr)
                return i;

        return -1;
    }
}
