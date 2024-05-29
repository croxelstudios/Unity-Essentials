using UnityEngine;
using System.Collections;

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

    Coroutine co;

    void OnEnable()
    {
        if (timeMode == TimeModeOrOnEnable.OnEnable) Copy();
        else co = StartCoroutine(CopyPerFrame());
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
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
        ReflectionTools.SetFieldValue(targetComponent, targetPropertyName,
            ReflectionTools.GetFieldValue(sourceComponent, sourcePropertyName));
    }
}
