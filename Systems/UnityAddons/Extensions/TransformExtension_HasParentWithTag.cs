using UnityEngine;

public static class TransformExtension_HasParentWithTag
{
    public static bool HasParentWithTag(this Transform tr, string tag)
    {
        Transform parent = tr.parent;
        if (parent != null)
        {
            if (parent.tag == tag) return true;
            else return HasParentWithTag(parent, tag);
        }
        else return false;
    }
}
