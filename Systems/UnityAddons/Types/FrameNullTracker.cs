using UnityEngine;

public struct FrameNullTracker
{
    public object obj;
    PerFrameTracker tracker;
    bool last;
    bool realNullCheck;

    public FrameNullTracker(object obj, bool realNullCheck = false)
    {
        this.obj = obj;
        tracker = new PerFrameTracker();
        last = obj == null;
        this.realNullCheck = realNullCheck;
    }

    void Initialize()
    {
        tracker = new PerFrameTracker();
        last = obj == null;
    }

    public bool IsNull()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return realNullCheck ? (obj is null) : (obj == null);
#endif

        if (tracker == null)
            Initialize();

        if (tracker.Simple())
            last = realNullCheck ? (obj is null) : (obj == null);
        return last;
    }
}
