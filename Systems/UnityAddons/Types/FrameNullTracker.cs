using UnityEngine;

public struct FrameNullTracker
{
    public object obj;
    PerFrameTracker tracker;
    bool last;

    public FrameNullTracker(object obj)
    {
        this.obj = obj;
        tracker = new PerFrameTracker();
        last = obj == null;
    }

    public bool IsNull()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return obj is null;
#endif

        if (tracker.Simple())
            last = obj is null;
        return last;
    }
}
