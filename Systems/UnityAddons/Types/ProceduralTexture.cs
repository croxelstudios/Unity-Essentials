using System;
using UnityEngine;

[Serializable]
public struct ProceduralTexture
{
    [SerializeField]
    Material textureMaterial;
    [SerializeField]
    [Min(1)]
    int textureResolution;
    [SerializeField]
    TextureWrapMode wrapMode;
    [SerializeField]
    FilterMode filterMode;

    [HideInInspector]
    public RenderTexture rt;

    public ProceduralTexture(Material textureMaterial)
    {
        this.textureMaterial = textureMaterial;
        textureResolution = 360;
        wrapMode = TextureWrapMode.Clamp;
        filterMode = FilterMode.Point;
        rt = null;
    }

    public ProceduralTexture(Material textureMaterial, int textureResolution, TextureWrapMode wrapMode, FilterMode filterMode)
    {
        this.textureMaterial = textureMaterial;
        this.textureResolution = textureResolution;
        this.wrapMode = wrapMode;
        this.filterMode = filterMode;
        rt = null;
    }

    public bool IsValid()
    {
        return textureMaterial != null;
    }

    public void Update(string name = "ProceduralTexture")
    {
        textureMaterial.Blit(ref rt, name, textureResolution);
        rt.wrapMode = wrapMode;
        rt.filterMode = filterMode;
    }

    public void Release()
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
    }
}
