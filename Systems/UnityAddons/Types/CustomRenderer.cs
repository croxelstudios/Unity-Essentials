using UnityEngine;
using UnityEngine.Events;

public class CustomRenderer : MonoBehaviour
{
    [SerializeField]
    //[MaterialEditor]
    protected Material[] materials = null;
    public Material[] sharedMaterials
    {
        get { return materials; }
        set
        {
            materials = materials.Resize(value.Length);
            for (int i = 0; i < value.Length; i++)
                materials[i] = value[i];
            instancedMaterials = false;
        }
    }
    bool instancedMaterials;
    public Material[] instMaterials
    {
        get
        {
            if (!instancedMaterials)
            {
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = Instantiate(materials[i]);
                instancedMaterials = true;
            }
            return materials;
        }
        set
        {
            materials = materials.Resize(value.Length);
            for (int i = 0; i < value.Length; i++)
                materials[i] = Instantiate(value[i]);
            instancedMaterials = true;
        }
    }
    protected UnityEvent startRendering;
    protected UnityEvent finishedRendering;
    protected MaterialPropertyBlock[] propertyBlocks;

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

    public bool IsVisible(float maxFar)
    {
        return IsVisible(-1, maxFar);
    }

    public bool IsVisible(bool excludeShadowCasters)
    {
        return IsVisible(-1, excludeShadowCasters);
    }

    public virtual bool IsVisible(int id, float maxFar)
    {
        return enabled;
    }

    public virtual bool IsVisible(int id, bool excludeShadowCasters = false)
    {
        return enabled;
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

    public void GetPropertyBlock(ref MaterialPropertyBlock block, int material)
    {
        propertyBlocks = propertyBlocks.Resize(Mathf.Max(materials.Length, material));
        if (propertyBlocks[material] == null)
            propertyBlocks[material] = new MaterialPropertyBlock();
        block = propertyBlocks[material];
    }

    public void SetPropertyBlock(MaterialPropertyBlock block, int material)
    {
        propertyBlocks = propertyBlocks.Resize(Mathf.Max(materials.Length, material));
        propertyBlocks[material] = block;
    }
}
