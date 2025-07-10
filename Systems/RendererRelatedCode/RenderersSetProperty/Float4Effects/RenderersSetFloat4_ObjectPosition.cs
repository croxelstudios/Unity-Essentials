using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderersSetFloat4_ObjectPosition : RenderersSetFloat4
{
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
    }

    protected override void UpdateBehaviour()
    {
        value = transform.position;
        base.UpdateBehaviour();
    }
}
