using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class RenderersSetFloat_Group : MonoBehaviour
{
    [SerializeField]
    float value = 0f;
    [SerializeField]
    bool update = true;

    [SerializeField]
    [HideInInspector]
    List<RenderersSetFloat> rsfs;

#if UNITY_EDITOR
    void OnValidate()
    {
        RenderersSetFloat[] array = GetComponentsInChildren<RenderersSetFloat>();
        rsfs = rsfs.CreateAddRange(array, true);

        for (int i = rsfs.Count; i-- > 0;)
        {
            if (rsfs[i] == null) rsfs.RemoveAt(i);
            else
            {
                rsfs[i].value = value;
                EditorUtility.SetDirty(rsfs[i]);
            }
        }
    }
#endif

    void OnEnable()
    {
        if (update)
        {
            if (RenderersSetFloat.init == null)
                RenderersSetFloat.init = new RenderersSetFloat.CustomEvent();
            RenderersSetFloat.init.AddListener(SetValue);

            RenderersSetFloat[] array = GetComponentsInChildren<RenderersSetFloat>();
            rsfs = rsfs.CreateAddRange(array, true);

            for (int i = rsfs.Count; i-- > 0;)
            {
                if (rsfs[i] == null) rsfs.RemoveAt(i);
                else rsfs[i].value = value;
            }
        }
    }

    void SetValue(RenderersSetFloat rsf)
    {
        if (rsf.transform.IsChildOf(transform))
        {
            rsf.value = value;
            rsfs.Add(rsf);
        }
    }

    //public void UpdateRenderers() //TO DO: Doesn´t work with non dynamic rsfs for some reason
    //{
    //    for (int i = rsfs.Count; i-- <= 0;)
    //    {
    //        if (rsfs[i] == null) rsfs.RemoveAt(i);
    //        else rsfs[i].UpdateRenderers();
    //    }
    //}
}
