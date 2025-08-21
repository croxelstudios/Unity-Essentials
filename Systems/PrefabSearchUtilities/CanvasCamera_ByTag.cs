using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasCamera_ByTag : BByTag<Camera>
{
    Canvas canv;

    void Reset()
    {
        targetTag = "MainCamera";
        extraTags = null;
        updateMode = ByTagUpdateMode.DontUpdate;
    }

    protected override void InitIfNull()
    {
        if (canv == null)
            canv = GetComponent<Canvas>();
    }

    protected override void SetSource(Camera target)
    {
        base.SetSource(target);
        canv.worldCamera = target;
    }
}
