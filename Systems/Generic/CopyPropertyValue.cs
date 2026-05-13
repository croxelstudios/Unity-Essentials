using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CopyPropertyValue : MonoBehaviour
{
    [SerializeField]
    Component sourceComponent = null;
    [SerializeField]
    string sourcePropertyName = "";
    [SerializeField]
    Component targetComponent = null;
    [SerializeField]
    string targetPropertyName = "";
    [SerializeField]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.OnEnable;

    void OnEnable()
    {
        if (SettingsValid())
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                if (timeMode == TimeModeOrOnEnable.OnEnable) Copy();
                else StartCoroutine(CopyPerFrame());
#if UNITY_EDITOR
            }
            else Copy();
#endif

#if UNITY_EDITOR
            this.OnEditorChange_In("sourceComponent", Copy, ".sourcePropertyName");
            this.OnEditorChange_In("targetComponent", Copy, ".targetPropertyName");
            this.OnEditorChange_In(Copy);
#endif
        }
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        this.OnEditorChange_Out("sourceComponent", Copy, ".sourcePropertyName");
        this.OnEditorChange_Out("targetComponent", Copy, ".targetPropertyName");
        this.OnEditorChange_Out(Copy);
#endif
        StopAllCoroutines();
    }

    IEnumerator CopyPerFrame()
    {
        while (true)
        {
            Copy();
            yield return timeMode.WaitFor();
        }
    }

    void Copy()
    {
        if (SettingsValid())
            ReflectionTools.SetFieldValue(targetComponent, targetPropertyName,
                ReflectionTools.GetFieldValue(sourceComponent, sourcePropertyName,
                timeMode != TimeModeOrOnEnable.OnEnable),
                timeMode != TimeModeOrOnEnable.OnEnable);
    }

    bool SettingsValid()
    {
        return (sourceComponent != null) && (targetComponent != null) &&
            (!sourcePropertyName.IsNullOrEmpty()) && (!targetPropertyName.IsNullOrEmpty());
    }
}
