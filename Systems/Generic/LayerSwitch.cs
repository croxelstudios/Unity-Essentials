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
    public static event LayerSwitchDelegate SwitchedParentLayer = null;
    public static event LayerSwitchDelegate SwitchedLayer = null;
    public delegate void LayerSwitchDelegate(GameObject go, int oldLayer, int newLayer);

    void OnValidate()
    {
        if (inObject == null)
            inObject = transform;
    }

    public void SwitchLayer()
    {
        SwitchLayer(newLayer);
    }

    public void SwitchLayer_ThisOnly()
    {
        SwitchLayer_ThisOnly(newLayer);
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
        GameObject go = inObject.gameObject;
        int l = go.layer;

        SwitchLayer_Recursive(inObject, ref oldLayers, -1);

        int newLayer = -1;
        if ((oldLayers != null) && oldLayers.ContainsKey(inObject))
            newLayer = oldLayers[inObject];

        SwitchedParentLayer?.Invoke(go, l, newLayer);
    }

    void SwitchLayer(Transform tr, int layer)
    {
        GameObject go = tr.gameObject;
        int l = go.layer;

        SwitchLayer_Recursive(tr, ref oldLayers, layer, IsNotEditor());

        SwitchedParentLayer?.Invoke(go, l, layer);
    }

    void SwitchLayer_ThisOnly(Transform tr, int layer)
    {
        GameObject go = tr.gameObject;
        int l = go.layer;

        SwitchLayer_ThisOnly(tr, ref oldLayers, layer, IsNotEditor());

        SwitchedParentLayer?.Invoke(go, l, layer);
    }

    void SwitchLayer_Recursive(Transform tr, ref Dictionary<Transform, int> dic, int newLayer, bool registerDic = false)
    {
        for (int i = 0; i < tr.childCount; i++)
            SwitchLayer_Recursive(tr.GetChild(i), ref dic, newLayer, registerDic);
        SwitchLayer_ThisOnly(tr, ref dic, newLayer, registerDic);
    }

    void SwitchLayer_ThisOnly(Transform tr, ref Dictionary<Transform, int> dic, int newLayer, bool registerDic = false)
    {
        GameObject go = tr.gameObject;
        int l = go.layer;
        if (registerDic)
        {
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
        SwitchedLayer?.Invoke(go, l, newLayer);
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
