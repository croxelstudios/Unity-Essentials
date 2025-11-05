using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CustomRenderer : MonoBehaviour
{
    [SerializeField]
    //[MaterialEditor]
    protected Material[] materials = null;
    protected UnityEvent startRendering;
    protected UnityEvent finishedRendering;

    public virtual void CreateComputables(string name, int vCount, int tCount)
    {
    }

    public virtual ComputableMesh[] GetComputables(Component comp)
    {
        return null;
    }

    public virtual void StopUseByComponent(Component comp)
    {
    }

    public virtual Matrix4x4 LocalToWorldMatrix(int id)
    {
        return transform.localToWorldMatrix;
    }

    public virtual Matrix4x4 WorldToLocalMatrix(int id)
    {
        return transform.worldToLocalMatrix;
    }

    public void AddStartAction(UnityAction action)
    {
        if (startRendering == null)
            startRendering = new UnityEvent();
        startRendering.AddListener(action);
    }

    public void AddFinishAction(UnityAction action)
    {
        if (finishedRendering == null)
            finishedRendering = new UnityEvent();
        finishedRendering.AddListener(action);
    }

    public void RemoveStartAction(UnityAction action)
    {
        startRendering?.RemoveListener(action);
    }

    public void RemoveFinishAction(UnityAction action)
    {
        finishedRendering?.RemoveListener(action);
    }
}
