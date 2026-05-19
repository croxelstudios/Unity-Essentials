using System.Collections.Generic;
using UnityEngine;

public class LayerSwitch : MonoBehaviour
{
    [SerializeField]
    Transform inObject = null;
    //TO DO: [LayerSelector]
    [SerializeField]
    string newLayer = "";

    Dictionary<Transform, int> oldLayers;

    void OnValidate()
    {
        if (inObject == null)
            inObject = transform;
    }

    public void SwitchLayer()
    {
        oldLayers = oldLayers.CreateIfNull();
        SwitchLayer(inObject, LayerMask.NameToLayer(newLayer));
    }

    public void SwitchLayer_ThisOnly()
    {
        oldLayers = oldLayers.CreateIfNull();
        SwitchLayer_ThisOnly(inObject, LayerMask.NameToLayer(newLayer));
    }

    public void SwitchLayer(string layer)
    {
        oldLayers = oldLayers.CreateIfNull();
        SwitchLayer(inObject, LayerMask.NameToLayer(layer));
    }

    public void SwitchLayer_ThisOnly(string layer)
    {
        oldLayers = oldLayers.CreateIfNull();
        SwitchLayer_ThisOnly(inObject, LayerMask.NameToLayer(layer));
    }

    public void RevertLayer()
    {
        SwitchLayer(inObject, ref oldLayers, -1);
    }

    void SwitchLayer(Transform tr, int layer)
    {
        SwitchLayer(tr, ref oldLayers, layer, IsNotEditor());
    }

    void SwitchLayer_ThisOnly(Transform tr, int layer)
    {
        SwitchLayer_ThisOnly(tr, ref oldLayers, layer, IsNotEditor());
    }

    void SwitchLayer(Transform tr, ref Dictionary<Transform, int> dic, int newLayer, bool registerDic = false)
    {
        for (int i = 0; i < tr.childCount; i++)
            SwitchLayer(tr.GetChild(i), ref dic, newLayer, registerDic);
        SwitchLayer_ThisOnly(tr, ref dic, newLayer, registerDic);
    }

    void SwitchLayer_ThisOnly(Transform tr, ref Dictionary<Transform, int> dic, int newLayer, bool registerDic = false)
    {
        if (registerDic)
        {
            int l = tr.gameObject.layer;
            if (dic.ContainsKey(tr)) { if (l != newLayer) dic[tr] = l; }
            else dic.Add(tr, l);

            tr.gameObject.layer = newLayer;
        }
        else
        {
            if ((dic != null) && dic.ContainsKey(tr))
                tr.gameObject.layer = dic[tr];
            else if (newLayer >= 0)
                tr.gameObject.layer = newLayer;
        }
    }

    public void DeactivateObject()
    {
        inObject.gameObject.SetActive(false);
    }

    public void ActivateObject()
    {
        inObject.gameObject.SetActive(true);
    }

    bool IsNotEditor()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return false;
        else
#endif
            return true;
    }
}
