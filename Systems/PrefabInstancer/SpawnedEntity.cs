using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SpawnedEntity : MonoBehaviour
{
    public delegate void EntityDelegate(SpawnedEntity entity);
    public event EntityDelegate EntityDestroyed;
    [HideInInspector]
    public PrefabInstancer instancer = null;
    [HideInInspector]
    public PrefabInstancer_Launcher instancerLauncher = null;

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            EntityDestroyed?.Invoke(this);
    }

    #region Instancer-filtered signals
    public void CallSignalOnInstancer(EventSignal signal)
    {
        signal.CallSignal(new Transform[] { instancer.GetTransform() });
    }
    #endregion

    #region Launcher-filtered signals
    public void CallSignalOnLauncher(EventSignal signal)
    {
        if (instancerLauncher != null)
            signal.CallSignal(new Transform[] { instancerLauncher.GetTransform() });
        else CallSignalOnInstancer(signal);
    }
    #endregion
}
