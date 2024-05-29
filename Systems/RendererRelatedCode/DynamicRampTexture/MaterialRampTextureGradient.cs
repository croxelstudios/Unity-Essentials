using UnityEngine;
using Sirenix.OdinInspector;

public class MaterialRampTextureGradient : MonoBehaviour
{
    [SerializeField]
    Material material = null;
    [SerializeField]
    string propertyName = "_RampTexture";
    [SerializeField]
    [OnValueChanged("@CreateTexture()")]
    Gradient gradient = null;

    [SerializeField]
    [ReadOnly]
    Texture texture = null;

    void OnEnable()
    {
        CreateTexture();
    }

    void Reset()
    {
        CreateTexture();
    }

    void CreateTexture()
    {
        Texture2D texture2D = new Texture2D(100, 1, TextureFormat.ARGB32, false);
        texture2D.filterMode = FilterMode.Point;
        for (int i = 0; i < 100; i++)
            texture2D.SetPixel(i, 0, gradient.Evaluate(i * 0.01f));
        texture2D.Apply();
        if (texture != null)
        {
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
        }
        texture = texture2D;
        if ((material != null) && material.HasProperty(propertyName))
            material.SetTexture(propertyName, texture);
    }
}
