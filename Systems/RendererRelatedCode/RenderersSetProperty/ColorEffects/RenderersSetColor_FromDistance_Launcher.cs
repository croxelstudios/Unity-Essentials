using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderersSetColor_FromDistance_Launcher : BRemoteLauncher
{
    RenderersSetColor_FromDistance[] blocks;

    void Awake()
    {
        FillArrayAwake(ref blocks);
    }

    public void UpdateTaggedObjects()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref blocks);
            foreach (RenderersSetColor_FromDistance block in blocks)
            {
                block.UpdateTaggedObjects();
            }
        }
    }

    public void ChangeTag(string tag)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref blocks);
            foreach (RenderersSetColor_FromDistance block in blocks)
            {
                block.ChangeTag(tag);
            }
        }
    }
}
