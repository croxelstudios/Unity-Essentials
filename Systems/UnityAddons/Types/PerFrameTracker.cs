using UnityEngine;

public class PerFrameTracker
{
    bool occurring = false;
    int repetitions = 0;
    int waitForRepetitions = 0;
    int prevFrameCount = 0;

    public bool ShouldStart(out bool shouldEndFirst)
    {
        shouldEndFirst = false;

        if (Time.frameCount != prevFrameCount)
        {
            prevFrameCount = Time.frameCount;
            if (waitForRepetitions > 0)
            {
                shouldEndFirst = true;
                occurring = false;
            }
            waitForRepetitions = repetitions;
            repetitions = 0;
        }
        repetitions++;

        if (!occurring)
        {
            occurring = true;
            return true;
        }
        else return false;
    }

    public bool ShouldEnd()
    {
        if (occurring)
        {
            if (waitForRepetitions > 0)
                waitForRepetitions--;
            if (waitForRepetitions <= 0)
            {
                occurring = false;
                return true;
            }
        }
        return false;
    } 
}

public static class PerFrameTrackerExtensions
{
    public static PerFrameTracker CreateIfNull(this PerFrameTracker tracker)
    {
        if (tracker == null)
            tracker = new PerFrameTracker();
        return tracker;
    }
}
