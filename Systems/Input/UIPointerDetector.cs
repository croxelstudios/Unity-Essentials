using UnityEngine;

public class UIPointerDetector : MonoBehaviour
{
    [SerializeField]
    DXEvent pointerEnter = null;
    [SerializeField]
    DXEvent pointerExit = null;
    [SerializeField]
    CallMoment callMoment = CallMoment.Update;
    [SerializeField]
    DXVectorEvent pointerPosition = null;

    enum CallMoment { Update, OnEnable, WhenCalled }

    bool last;
    RectTransform rectTr;
    RectTransform parent;
    bool paused;

    void OnEnable()
    {
        RectTransform next = parent = rectTr = transform as RectTransform;
        do
        {
            parent = next;
            next = parent.parent as RectTransform;
        }
        while (next != null);

        if (callMoment == CallMoment.OnEnable)
            pointerPosition?.Invoke(LerpPosition());
    }

    void OnDisable()
    {
        if (last)
        {
            pointerExit?.Invoke();
            last = false;
        }
    }

    void Update()
    {
        if (!paused)
        {
            Vector2 pos = PointerPosition();
            if (callMoment == CallMoment.Update)
                pointerPosition?.Invoke(LerpPosition(pos));
            if (rectTr.rect.Contains(pos))
            {
                if (!last)
                {
                    pointerEnter?.Invoke();
                    last = true;
                }
            }
            else
            {
                if (last)
                {
                    pointerExit?.Invoke();
                    last = false;
                }
            }
        }
    }

    public void SendPosition()
    {
        pointerPosition?.Invoke(LerpPosition());
    }

    Vector2 PointerPosition()
    {
        return rectTr.InverseTransformPoint(
                parent.TransformPoint(parent.PointerPosition()));
    }

    Vector2 LerpPosition(Vector2 pos)
    {
        return (pos - rectTr.rect.center) / rectTr.rect.size;
    }

    Vector2 LerpPosition()
    {
        return (PointerPosition() - rectTr.rect.center) / rectTr.rect.size;
    }

    public void Pause()
    {
        paused = true;
    }

    public void Unpause()
    {
        paused = false;
    }
}
