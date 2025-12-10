using UnityEngine;

public class SetRenderingLayer : MonoBehaviour
{
    [SerializeField]
    RenderingLayerMask renderingLayers = RenderingLayerMask.defaultRenderingLayerMask;

    Renderer[] renderers;

    void OnEnable()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].renderingLayerMask = renderingLayers;
    }

    //int CalculateBitmask(int currentBitmask, System.Array enumValues)
    //{
    //    foreach (LightLayerEnum current in enumValues)
    //    {
    //        if (current == LightLayerEnum.Everything) continue;

    //        int layerBitVal = (int)current;

    //        bool set = current == renderingLayer;
    //        currentBitmask = SetBitmask(currentBitmask, layerBitVal, set);
    //    }

    //    return currentBitmask;
    //}

    //int SetBitmask(int bitmask, int bitVal, bool set)
    //{
    //    if (set)
    //        bitmask |= bitVal;
    //    else
    //        bitmask &= ~bitVal;

    //    return bitmask;
    //}
}
