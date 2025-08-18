using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasCamera_ByTag : BByTag<Camera>
{
    [SerializeField]
    [TagSelector]
    string cameraTag = "MainCamera";

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
        canv.worldCamera = target;
    }
}
