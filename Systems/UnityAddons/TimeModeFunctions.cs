using UnityEngine;

public enum TimeMode { Update, FixedUpdate, Unscaled }

public enum ScaledTimeMode { Update, FixedUpdate }

public enum TimeModeOrOnEnable { Update, FixedUpdate, Unscaled, OnEnable }

public enum RenderingTimeMode { Update, Unscaled }

public enum RenderingTimeModeOrOnEnable { Update, Unscaled, OnEnable }

public static class TimeModeFunctions
{
    //Delta Time
    public static float DeltaTime(this TimeMode timeMode)
    {
        switch (timeMode)
        {
            case TimeMode.FixedUpdate:
                return UnityEngine.Time.fixedDeltaTime;
            case TimeMode.Unscaled:
                return UnityEngine.Time.unscaledDeltaTime;
            default:
                return UnityEngine.Time.deltaTime;
        }
    }

    public static float DeltaTime(this ScaledTimeMode timeMode)
    {
        return DeltaTime((TimeMode)timeMode);
    }

    public static float DeltaTime(this TimeModeOrOnEnable timeMode)
    {
        return DeltaTime((TimeMode)timeMode);
    }

    public static float DeltaTime(this RenderingTimeMode timeMode)
    {
        TimeMode tm = (TimeMode)((timeMode == 0) ? 0 : 2);
        return DeltaTime(tm);
    }

    public static float DeltaTime(this RenderingTimeModeOrOnEnable timeMode)
    {
        TimeMode tm = (TimeMode)((timeMode == RenderingTimeModeOrOnEnable.Unscaled) ? 2 : 0);
        return DeltaTime(tm);
    }

    //Time
    public static float Time(this TimeMode timeMode)
    {
        switch (timeMode)
        {
            case TimeMode.FixedUpdate:
                return UnityEngine.Time.fixedTime;
            case TimeMode.Unscaled:
                return UnityEngine.Time.unscaledTime;
            default:
                return UnityEngine.Time.time;
        }
    }

    public static float Time(this ScaledTimeMode timeMode)
    {
        return Time((TimeMode)timeMode);
    }

    public static float Time(this TimeModeOrOnEnable timeMode)
    {
        return Time((TimeMode)timeMode);
    }

    public static float Time(this RenderingTimeMode timeMode)
    {
        TimeMode tm = (TimeMode)((timeMode == 0) ? 0 : 2);
        return Time(tm);
    }

    public static float Time(this RenderingTimeModeOrOnEnable timeMode)
    {
        TimeMode tm = (TimeMode)((timeMode == RenderingTimeModeOrOnEnable.Unscaled) ? 2 : 0);
        return Time(tm);
    }

    //IsFixed
    public static bool IsFixed(this TimeMode timeMode)
    {
        return timeMode == TimeMode.FixedUpdate;
    }

    public static bool IsFixed(this ScaledTimeMode timeMode)
    {
        return ((TimeMode)timeMode).IsFixed();
    }

    public static bool IsFixed(this TimeModeOrOnEnable timeMode)
    {
        return ((TimeMode)timeMode).IsFixed();
    }

    //IsSmooth
    public static bool IsSmooth(this TimeMode timeMode)
    {
        return timeMode != TimeMode.FixedUpdate;
    }

    public static bool IsSmooth(this ScaledTimeMode timeMode)
    {
        return ((TimeMode)timeMode).IsSmooth();
    }

    public static bool IsSmooth(this TimeModeOrOnEnable timeMode)
    {
        return ((TimeMode)timeMode).IsSmooth() && (timeMode != TimeModeOrOnEnable.OnEnable);
    }

    public static bool IsSmooth(this RenderingTimeModeOrOnEnable timeMode)
    {
        return timeMode != RenderingTimeModeOrOnEnable.OnEnable;
    }

    //WaitFor
    public static YieldInstruction WaitFor(this TimeMode timeMode)
    {
        if (timeMode.IsFixed()) return new WaitForFixedUpdate();
        else return null;
    }

    public static YieldInstruction WaitFor(this ScaledTimeMode timeMode)
    {
        return ((TimeMode)timeMode).WaitFor();
    }

    public static YieldInstruction WaitFor(this TimeModeOrOnEnable timeMode)
    {
        return ((TimeMode)timeMode).WaitFor();
    }

    public static YieldInstruction WaitFor(this RenderingTimeMode timeMode)
    {
        return ((TimeMode)timeMode).WaitFor();
    }

    public static YieldInstruction WaitFor(this RenderingTimeModeOrOnEnable timeMode)
    {
        return ((TimeMode)timeMode).WaitFor();
    }

    //Other
    public static bool OnEnable(this TimeModeOrOnEnable timeMode)
    {
        return timeMode == TimeModeOrOnEnable.OnEnable;
    }

    public static bool OnEnable(this RenderingTimeModeOrOnEnable timeMode)
    {
        return timeMode == RenderingTimeModeOrOnEnable.OnEnable;
    }
}