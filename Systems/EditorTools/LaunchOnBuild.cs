using UnityEngine;

public class LaunchOnBuild : MonoBehaviour
{
    [SerializeField]
    DXEvent isBuild = null;

    private void OnEnable()
    {
#if !UNITY_EDITOR
        isBuild?.Invoke();
#endif
    }
}
