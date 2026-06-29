using UnityEngine;

public struct FrameNullTracker
{
    object obj;
    int frame;
    bool last;

    public FrameNullTracker(object obj)
    {
        this.obj = obj;
        frame = Time.frameCount;
        last = obj == null;
    }

    public bool IsNull()
    {
        if (Time.frameCount != frame)
        {
            frame = Time.frameCount;
            last = obj == null;
        }
        return last;
    }
}
