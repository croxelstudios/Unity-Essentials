using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MaterialTextureMaterial : MonoBehaviour
{
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    ProceduralTexture proceduralTexture = new ProceduralTexture(null);
    [SerializeField]
    bool update = true;
    [SerializeField]
    MaterialProperty[] appliances = new MaterialProperty[] { new MaterialProperty(null) };

    struct MaterialProperty
    {
        public string name;
        public Material material;

        public MaterialProperty(Material material)
        {
            this.name = "_MainTex";
            this.material = material;
        }

        public MaterialProperty(string name, Material material)
        {
            this.name = name;
            this.material = material;
        }
    }

    void OnEnable()
    {
        UpdateRT();
        for (int i = 0; i < appliances.Length; i++)
            if ((appliances[i].material != null) && appliances[i].material.HasProperty(appliances[i].name))
                appliances[i].material.SetTexture(appliances[i].name, proceduralTexture.rt);
    }

    void OnDisable()
    {
        proceduralTexture.Release();
    }

    void Update()
    {
        if (update)
            UpdateRT();
    }

    void UpdateRT()
    {
        if (proceduralTexture.IsValid())
            proceduralTexture.Update();
    }
}
