using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ReflectionProbe))]
public class ReflectionProbeTool : MonoBehaviour
{
    ReflectionProbe probe;
    [SerializeField]
    bool refreshOnAwake = true;

    void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
        if (refreshOnAwake) Refresh();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Refresh()
    {
        StartCoroutine(Refresher());
    }

    public void SwitchUpdateMode(bool turnTrue)
    {
        if (probe == null) probe = GetComponent<ReflectionProbe>();
        probe.refreshMode = turnTrue ? UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame :
        UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        StartCoroutine(Refresher());
    }

    IEnumerator Refresher()
    {
        yield return null;
        if (probe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake)
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        probe.RenderProbe();
    }
}
