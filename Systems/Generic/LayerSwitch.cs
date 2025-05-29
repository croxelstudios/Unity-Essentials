using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSwitch : MonoBehaviour
{
    [SerializeField]
    string newLayer = "";

    Dictionary<Transform, int> oldLayers;

    public void SwitchLayer()
    {
        if (oldLayers == null) oldLayers = new Dictionary<Transform, int>();
        SwitchLayer(transform, LayerMask.NameToLayer(newLayer));
    }

    public void RevertLayer()
    {
        SwitchLayer(transform, ref oldLayers, 0);
    }

    void SwitchLayer(Transform tr, int layer)
    {
        SwitchLayer(tr, ref oldLayers, layer, true);
    }

    void SwitchLayer(Transform tr, ref Dictionary<Transform, int> dic, int defaultLayer, bool registerDic = false)
    {
        for (int i = 0; i < tr.childCount; i++)
            SwitchLayer(tr.GetChild(i), ref dic, defaultLayer, registerDic);
        if (registerDic)
        {
            int l = tr.gameObject.layer;
            if (dic.ContainsKey(tr)) { if (l != defaultLayer) dic[tr] = l; }
            else dic.Add(tr, l);

            tr.gameObject.layer = defaultLayer;
        }
        else
        {
            if ((dic != null) && dic.ContainsKey(tr))
                tr.gameObject.layer = dic[tr];
            else tr.gameObject.layer = defaultLayer;
        }
    }
}
