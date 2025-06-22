using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
using QFSW.QC.Actions;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/Flag")] //Change type here
public class Flag : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    bool checkMultipleConditions = false;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    bool startValue = false; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("SetFlagOnCurrentTagAndValue")]
    public bool currentValue = false; //Change type here

    [FoldoutGroup("Before Calls")]
    // TO DO: Display name should not be this one (should not show the "_").
    //LabelText attribute does not work because the property inspector of DXEvents is a custom inspector
    public DXEvent whenTrue_ = null;
    [FoldoutGroup("Before Calls")]
    public DXEvent whenFalse_ = null;

    [FoldoutGroup("After Calls", true)]
    public DXEvent whenTrue = null;
    [FoldoutGroup("After Calls", true)]
    public DXEvent whenFalse = null;

    int currentConditionCount = 0;

    public void SetFlag(bool value, IEnumerable<GameObject> objects) //Change type here
    {
        SetFlag(value, "", objects);
    }

    public void SetFlag(bool value, IEnumerable<Transform> transforms) //Change type here
    {
        SetFlag(value, "", transforms);
    }

    [TagSelector]
    public void SetFlag(bool value, string tag) //Change type here
    {
        SetFlag<Transform>(value, tag, null);
    }

    public void SetFlag(bool value) //Change type here
    {
        SetFlag(value, "");
    }

    void SetFlag<O>(bool value, string tag, IEnumerable<O> objects = null) where O : Object //Change type here
    {
        bool canExecute = false;
        if (!checkMultipleConditions)
            canExecute = true;
        else
        {
            if (value ^ startValue)
            {
                if (currentConditionCount == 0)
                    canExecute = true;
                currentConditionCount++;
            }
            else
            {
                if (currentConditionCount > 0)
                {
                    if (currentConditionCount == 1)
                        canExecute = true;
                    currentConditionCount--;
                }
            }
        }

        if ((value != currentValue) && canExecute)
        {
            beforeCall?.Invoke();

            if (value) whenTrue_?.Invoke();
            else whenFalse_?.Invoke();

            currentValue = value;
            if (objects == null)
                Launch(Calculate(), tag);
            else
                Launch(Calculate(), objects);

            called?.Invoke();

            if (value) whenTrue?.Invoke();
            else whenFalse?.Invoke();
        }
    }

    [Command("set-flag")]
    public static void SetFlag(Flag signal, bool value) //Change type here
    {
        signal.SetFlag(value);
    }

    public void SwitchValue()
    {
        SetFlag(!currentValue);
    }

    bool Calculate() //Change type here
    {
        return currentValue;
    }

#if UNITY_EDITOR
    public bool CanShowStartValue()
    {
        return resetValueOnStart && !Application.isPlaying;
    }

    public void SetFlagOnCurrentTagAndValue()
    {
        if (Application.isPlaying)
            SetFlag(currentValue, currentTag);
    }
#endif

    protected override void OnLoad()
    {
        base.OnLoad();
        Reset();
    }

    protected override void Reset()
    {
        if (resetValueOnStart)
            DoReset();
    }

    public void DoReset()
    {
        currentValue = startValue;
        currentConditionCount = 0;
    }

    public void LaunchOnTrue()
    {
        LaunchOnTrue("");
    }

    public void LaunchOnTrue(string tag)
    {
        if (currentValue)
            Launch(true, tag);
    }

    public void LaunchOnFalse()
    {
        LaunchOnFalse("");
    }

    public void LaunchOnFalse(string tag)
    {
        if (!currentValue)
            Launch(false, tag);
    }

    void Launch(bool value, string tag)
    {
        if (dynamicSearch) DynamicSearch<FlagChecker>(); //Change type here

        if (listeners != null)
            for (int i = (listeners.Count - 1); i >= 0; i--)
                if ((tag == "") || (tag == listeners[i].receiver.tag))
                    ((FlagChecker)listeners[i].receiver). //Change type here
                        LaunchActions(listeners[i].index, value);

        if (dynamicSearch) listeners = null;
    }

    void Launch<O>(bool value, IEnumerable<O> objects) where O : Object
    {
        if (dynamicSearch) DynamicSearch<FlagChecker>(); //Change type here

        if (listeners != null)
            for (int i = (listeners.Count - 1); i >= 0; i--)
            {
                bool isChild = false;
                foreach (O obj in objects)
                {
                    Transform tr = obj.GetTransform();
                    if (listeners[i].receiver.transform.IsChildOf(tr))
                    {
                        isChild = true;
                        break;
                    }
                }
                if (isChild)
                    ((FlagChecker)listeners[i].receiver). //Change type here
                        LaunchActions(listeners[i].index, value);
            }
        if (dynamicSearch) listeners = null;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Flag))] //Change type here
public class FlagPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
