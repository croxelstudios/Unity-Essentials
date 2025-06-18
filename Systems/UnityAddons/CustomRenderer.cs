using UnityEngine;

public class CustomRenderer : MonoBehaviour
{
    [SerializeField]
    //[MaterialEditor]
    protected Material[] materials = null;

    public virtual void CreateComputables(string name, int vCount, int tCount)
    {
    }

    public virtual ComputableMesh[] GetComputables()
    {
        return null;
    }

    public virtual void StopUsingComputables()
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
