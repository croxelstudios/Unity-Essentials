using UnityEngine;

public class ShaderUnscaledTimeSetter : MonoBehaviour
{
    void Update()
    {
        float t = Time.unscaledTime;
        Shader.SetGlobalVector("_UnscaledTime", new Vector4(t / 20, t, t * 2, t * 3));
        Shader.SetGlobalVector("_UnscaledSinTime", new Vector4(Mathf.Sin(t / 8), Mathf.Sin(t / 4), Mathf.Sin(t / 2), Mathf.Sin(t)));
        Shader.SetGlobalVector("_UnscaledCosTime", new Vector4(Mathf.Cos(t / 8), Mathf.Cos(t / 4), Mathf.Cos(t / 2), Mathf.Cos(t)));
        t = Time.unscaledDeltaTime;
        Shader.SetGlobalVector("unity_UnscaledDeltaTime", new Vector4(t, 1 / t, t, 1 / t));
    }
}
