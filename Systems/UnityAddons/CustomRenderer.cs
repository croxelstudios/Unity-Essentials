using UnityEngine;
using UnityEngine.Events;

public class CustomRenderer : MonoBehaviour
{
    [SerializeField]
    //[MaterialEditor]
    protected Material[] materials = null;
    [HideInInspector]
    public UnityEvent startRendering;
    [HideInInspector]
    public UnityEvent finishedRendering;

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
}
