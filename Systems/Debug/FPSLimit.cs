using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [SerializeField]
    int limit = 60;

    int oldVSync;

    void OnEnable()
    {
        oldVSync = QualitySettings.vSyncCount;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = limit;
    }

    void OnDisable()
    {
        QualitySettings.vSyncCount = oldVSync;
        Application.targetFrameRate = -1;
    }
}
