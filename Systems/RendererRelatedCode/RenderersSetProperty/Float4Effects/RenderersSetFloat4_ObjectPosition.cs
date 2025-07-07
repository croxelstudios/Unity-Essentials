using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderersSetFloat4_ObjectPosition : RenderersSetFloat4
{
    protected override void UpdateBehaviour()
    {
        value = transform.position;
        base.UpdateBehaviour();
    }
}
