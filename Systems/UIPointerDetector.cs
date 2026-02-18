using UnityEngine;

public class UIPointerDetector : MonoBehaviour
{
    [SerializeField]
    DXEvent pointerEnter = null;
    [SerializeField]
    DXEvent pointerExit = null;

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
    }

    void OnDisable()
    {
        if (last)
        {
            pointerExit.Invoke();
            last = false;
        }
    }

    void Update()
    {
        if (!paused)
        {
            Vector2 pos = rectTr.InverseTransformPoint(parent.PointerPosition());
            if (rectTr.rect.Contains(pos))
            {
                if (!last)
                {
                    pointerEnter.Invoke();
                    last = true;
                }
            }
            else
            {
                if (last)
                {
                    pointerExit.Invoke();
                    last = false;
                }
            }
        }
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
