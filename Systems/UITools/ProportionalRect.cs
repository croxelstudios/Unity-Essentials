using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(RectTransform))]
public class ProportionalRect : MonoBehaviour
{
    [SerializeField]
    bool size = true;
    [Indent]
    [ShowIf("size")]
    [SerializeField]
    Vector2 proportion = Vector2.one;
    [SerializeField]
    bool offset = false;
    [Indent]
    [ShowIf("offset")]
    [SerializeField]
    Vector2 relativeOffset = Vector2.zero;

    RectTransform rect;
    RectTransform parent;
    Vector2 lastSize;

    private void OnEnable()
    {
        if (rect == null) rect = transform as RectTransform;
        if (parent == null) parent = transform.parent as RectTransform;
        Canvas.preWillRenderCanvases += OnWillRenderCanvas;
        Apply(false);
    }

    private void OnDisable()
    {
        Canvas.preWillRenderCanvases -= OnWillRenderCanvas;
    }

    void OnValidate()
    {
        Apply(false);
    }

    void OnRectTransformDimensionsChange()
    {
        Apply(true);
    }

    void OnWillRenderCanvas()
    {
        Apply(true);
    }

    void Apply(bool checkSize)
    {
        if ((rect == null) || (parent == null)) return;

        Vector2 pSize = parent.rect.size;
        if ((!checkSize) || (lastSize != pSize))
        {
            if (size) rect.sizeDelta = proportion * pSize;
            if (offset) rect.anchoredPosition = relativeOffset * pSize;
            lastSize = pSize;
        }
    }
}
