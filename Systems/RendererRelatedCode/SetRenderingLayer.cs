using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SetRenderingLayer : MonoBehaviour
{
    [SerializeField]
    LightLayerEnum renderingLayer = LightLayerEnum.LightLayerDefault;

    Renderer[] renderers;

    void OnEnable()
    {
        System.Array enumValues = System.Enum.GetValues(typeof(LightLayerEnum));
        renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            int bitmask = (int)renderingLayer;
            bitmask = CalculateBitmask(bitmask, enumValues);
            renderers[i].renderingLayerMask = (uint)bitmask;
        }
    }

    int CalculateBitmask(int currentBitmask, System.Array enumValues)
    {
        foreach (LightLayerEnum current in enumValues)
        {
            if (current == LightLayerEnum.Everything) continue;

            int layerBitVal = (int)current;

            bool set = current == renderingLayer;
            currentBitmask = SetBitmask(currentBitmask, layerBitVal, set);
        }

        return currentBitmask;
    }

    int SetBitmask(int bitmask, int bitVal, bool set)
    {
        if (set)
            bitmask |= bitVal;
        else
            bitmask &= ~bitVal;

        return bitmask;
    }
}
