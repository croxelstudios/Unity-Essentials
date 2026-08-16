using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeContext
{
    static Dictionary<int, float> scales;
    static TimeContext_Clock clock;
    static float fixedTimeRelation = -1;
    protected static float deltaTime { get { return UnityEngine.Time.deltaTime; } }
    protected static float fixedDeltaTime { get { return UnityEngine.Time.fixedDeltaTime; } }
    protected static float unscaledDeltaTime { get { return UnityEngine.Time.unscaledDeltaTime; } }
    protected static float time { get { return UnityEngine.Time.time; } }
    protected static float fixedTime { get { return UnityEngine.Time.fixedTime; } }
    protected static float unscaledTime { get { return UnityEngine.Time.unscaledTime; } }
    protected static float timeScale
    {
        get { return UnityEngine.Time.timeScale; }
        set { UnityEngine.Time.timeScale = value; }
    }

    static void EnsureClock()
    {
        if (clock == null)
        {
            GameObject clockObj = new GameObject("TimeContextClock");
            UnityEngine.Object.DontDestroyOnLoad(clockObj);
            clock = clockObj.AddComponent<TimeContext_Clock>();
        }
    }

    public static float GetTimeScale(int timeSpace)
    {
        if (timeSpace <= 0)
            return timeScale;
        if (!scales.SmartGetValue(timeSpace, out float scale))
            scale = 1f;
        return scale;
    }

    public static void SetTimeScale(int timeSpace, float scale)
    {
        if (timeSpace <= 0) timeScale = scale;
        else
        {
            scales = scales.CreateIfNull();
            scales.Set(timeSpace, scale);
        }
    }

    public static float DeltaTime(int timeSpace)
    {
        if (timeSpace <= 0) return deltaTime;
        else return unscaledDeltaTime * GetTimeScale(timeSpace);
    }

    public static float FixedDeltaTime(int timeSpace)
    {
        if (timeSpace <= 0) return fixedDeltaTime;
        else
        {
            if (fixedTimeRelation < 0)
                fixedTimeRelation = fixedDeltaTime / timeScale;
            return fixedTimeRelation * GetTimeScale(timeSpace);
        }
    }

    public static float Time(int timeSpace)
    {
        if (timeSpace <= 0) return time;
        else
        {
            EnsureClock();
            return clock.GetCustomTime(timeSpace);
        }
    }

    public static float FixedTime(int timeSpace)
    {
        if (timeSpace <= 0) return fixedTime;
        else
        {
            EnsureClock();
            return clock.GetCustomFixedTime(timeSpace);
        }
    }

    public static YieldInstruction WaitFor(int timeSpace)
    {
        if (timeSpace <= 0) return null;
        else
        {
            EnsureClock();
            return clock.StartWaitFor(timeSpace);
        }
    }

    public static YieldInstruction WaitForFixed(int timeSpace)
    {
        if (timeSpace <= 0) return new WaitForFixedUpdate();
        else
        {
            EnsureClock();
            return clock.StartWaitForFixed(timeSpace);
        }
    }
}

[HideLabel]
[InlineProperty]
[Serializable]
public class TimeContext<T> : TimeContext where T : struct, Enum
{
    public T timeMode;
    [Indent]
    [StringSelector("names", false)]
    [ShowIf("@EnabledTimeSpaceSetting()")]
    public int timeSpace;
    protected string[] names { get { return TimeSpaces.Names(); } }

    public TimeContext(T timeMode)
    {
        this.timeMode = timeMode;
        timeSpace = 0;
    }

    public TimeContext(T timeMode, int timeSpace)
    {
        this.timeMode = timeMode;
        this.timeSpace = timeSpace;
    }

    public TimeContext(int timeSpace)
    {
        timeMode = ToTimeMode(0);
        this.timeSpace = timeSpace;
    }

    protected T ToTimeMode(int i)
    {
        return (T)Enum.ToObject(typeof(T), i);
    }

    protected int ToInt(T timeMode)
    {
        return Convert.ToInt32(timeMode);
    }

    protected string ToString(T timeMode)
    {
        return Enum.GetName(typeof(T), timeMode);
    }

    protected TimeModeOrOnEnable ToEnum(T timeMode)
    {
        if (ToString(timeMode) == Enum.GetName(typeof(TimeModeOrOnEnable), TimeModeOrOnEnable.Update))
            return TimeModeOrOnEnable.Update;
        else if (ToString(timeMode) == Enum.GetName(typeof(TimeModeOrOnEnable), TimeModeOrOnEnable.FixedUpdate))
            return TimeModeOrOnEnable.FixedUpdate;
        else if (ToString(timeMode) == Enum.GetName(typeof(TimeModeOrOnEnable), TimeModeOrOnEnable.Unscaled))
            return TimeModeOrOnEnable.Unscaled;
        else if (ToString(timeMode) == Enum.GetName(typeof(TimeModeOrOnEnable), TimeModeOrOnEnable.OnEnable))
            return TimeModeOrOnEnable.OnEnable;
        else return TimeModeOrOnEnable.Update;
    }

    public float GetTimeScale()
    {
        return EnabledTimeSpaceSetting() ? GetTimeScale(timeSpace) : 1f;
    }

    protected virtual bool EnabledTimeSpaceSetting()
    {
        TimeModeOrOnEnable en = ToEnum(timeMode);
        return (en == TimeModeOrOnEnable.Update) || (en == TimeModeOrOnEnable.FixedUpdate);
    }

    public virtual float DeltaTime()
    {
        switch (ToEnum(timeMode))
        {
            case TimeModeOrOnEnable.FixedUpdate:
                return FixedDeltaTime(timeSpace);
            case TimeModeOrOnEnable.Unscaled:
                return unscaledDeltaTime;
            default:
                return DeltaTime(timeSpace);
        }
    }

    public virtual float Time()
    {
        switch (ToEnum(timeMode))
        {
            case TimeModeOrOnEnable.FixedUpdate:
                return FixedTime(timeSpace);
            case TimeModeOrOnEnable.Unscaled:
                return unscaledTime;
            default:
                return Time(timeSpace);
        }
    }

    public virtual YieldInstruction WaitFor()
    {
        switch (ToEnum(timeMode))
        {
            case TimeModeOrOnEnable.FixedUpdate:
                return WaitForFixed(timeSpace);
            case TimeModeOrOnEnable.Unscaled:
                return null;
            default:
                return WaitFor(timeSpace);
        }
    }

    public virtual bool IsFixed()
    {
        return ToEnum(timeMode).IsFixed();
    }

    public virtual bool IsSmooth()
    {
        return ToEnum(timeMode).IsSmooth();
    }
}
