using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLog : MonoBehaviour
{
#if UNITY_EDITOR
    public bool note = false;
    [TextArea]
    [ShowIf("@note")]
    public string NOTE = "";

    public void Log(Color value)
    {
        DLog(value);
    }

    public void Log(Vector4 value)
    {
        DLog(value);
    }

    public void Log(Collider value)
    {
        DLog(value);
    }

    public void Log(char value)
    {
        DLog(value);
    }

    public void Log(int value)
    {
        DLog(value);
    }

    public void Log(Transform value)
    {
        DLog(value);
    }

    public void Log(GameObject value)
    {
        DLog(value);
    }

    public void Log(Vector3 value)
    {
        DLog(value);
    }

    public void Log(Vector2 value)
    {
        DLog(value);
    }

    public void Log(float value)
    {
        DLog(value);
    }

    public void Log(string value)
    {
        DLog(value);
    }

    void DLog<T>(T value)
    {
        Debug.Log(value.ToString());
    }
#endif
}
