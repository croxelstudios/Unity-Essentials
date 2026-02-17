using UnityEngine;

[ExecuteAlways]
public class RenderersSetFloat4_ObjectPosition : RenderersSetFloat4
{
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    protected override void Init()
    {
        value = transform.position;
        base.Init();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
        {
            value = transform.position;
            TryUpdate();
        }
    }
}
