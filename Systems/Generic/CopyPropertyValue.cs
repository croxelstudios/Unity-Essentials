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
            if (!Application.isPlaying)
                Undo.postprocessModifications += OnPostprocess;
#endif
        }
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        Undo.postprocessModifications -= OnPostprocess;
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

#if UNITY_EDITOR
    UndoPropertyModification[] OnPostprocess(UndoPropertyModification[] modifications)
    {
        foreach (UndoPropertyModification m in modifications)
        {
            PropertyModification pm = m.currentValue;
            if (((pm.target == sourceComponent) && PathContainsProp(pm.propertyPath, sourcePropertyName)) ||
                ((pm.target == targetComponent) && PathContainsProp(pm.propertyPath, targetPropertyName)) ||
                (pm.target == this))
                Copy();
        }

        return modifications;
    }

    bool PathContainsProp(string a, string b)
    {
        a = a.ToLower().Replace("m_", "");
        b = b.ToLower().Replace("m_", "");
        if (a.Contains(b)) return true;
        else return false;
    }
#endif
}
