using UnityEngine;
using System;

public struct RendMatProp : IEquatable<RendMatProp>
{
    RendMat rendMat;
    public Renderer rend { get { return rendMat.rend; } set { rendMat.rend = value; } }
    public int mat { get { return rendMat.mat; } set { rendMat.mat = value; } }
    public string property;
    public Material sharedMaterial { get { return rendMat.sharedMaterial; } }
    public Material material { get { return rendMat.material; } }

    public static implicit operator RendMat(RendMatProp obj) => obj.rendMat;

    public RendMatProp(Renderer rend, int mat, string property)
    {
        rendMat = new RendMat(rend, mat);
        this.property = property;
    }

    public RendMatProp(RendMat rendMat, string property)
    {
        this.rendMat = rendMat;
        this.property = property;
    }

    public override bool Equals(object other)
    {
        if (!(other is RendMatProp)) return false;
        return Equals((RendMatProp)other);
    }

    public bool Equals(RendMatProp other)
    {
        return (rendMat == other.rendMat)
            && (property == other.property);
    }

    public override int GetHashCode()
    {
        return rendMat.GetHashCode() * 31 + property.GetHashCode();
    }

    public static bool operator ==(RendMatProp o1, RendMatProp o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(RendMatProp o1, RendMatProp o2)
    {
        return !o1.Equals(o2);
    }
}

public struct RendMat : IEquatable<RendMat>
{
    public Renderer rend;
    public int mat;
    public Material sharedMaterial { get { return rend.sharedMaterials[mat]; } }
    public Material material { get { return rend.materials[mat]; } }

    public RendMat(Renderer rend, int mat)
    {
        this.rend = rend;
        this.mat = mat;
    }

    public void GetPropertyBlock(MaterialPropertyBlock block)
    {
        rend.GetPropertyBlock(block, mat);
    }

    public void SetPropertyBlock(MaterialPropertyBlock block)
    {
        rend.SetPropertyBlock(block, mat);
    }

    public bool IsNull()
    {
        return (rend == null) || (!mat.IsBetween(0, rend.sharedMaterials.Length)) || (sharedMaterial == null);
    }

    public override bool Equals(object other)
    {
        if (!(other is RendMat)) return false;
        return Equals((RendMat)other);
    }

    public bool Equals(RendMat other)
    {
        return (rend == other.rend)
            && (mat == other.mat);
    }

    public override int GetHashCode()
    {
        return rend.GetHashCode() * 31 + mat.GetHashCode();
    }

    public static bool operator ==(RendMat o1, RendMat o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(RendMat o1, RendMat o2)
    {
        return !o1.Equals(o2);
    }
}