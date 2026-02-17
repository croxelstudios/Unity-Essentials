using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Renderer))]
public class RenderersSetTexture_RampTextureGradient : RenderersSetTexture
{
    [SerializeField]
    [OnValueChanged("@CreateTexture()")]
    Gradient gradient = null;

    protected override void Init()
    {
        //TO DO: Don't execute this code when on build. Texture should have been serialized in editor before build.
        CreateTexture();
        base.Init();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif

    void Reset()
    {
        propertyName = "_RampTexture";
        CreateTexture();
    }

    void CreateTexture()
    {
        Texture2D texture2D = new Texture2D(100, 1, TextureFormat.ARGB32, false);
        for (int i = 0; i < 100; i++)
            texture2D.SetPixel(i, 0, gradient.Evaluate(i * 0.01f));
        texture2D.Apply();
        if (texture != null)
        {
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
        }
        texture = texture2D;
    }
}
