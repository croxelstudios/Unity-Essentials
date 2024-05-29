#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public static class Texture2DExtension_GetSpritesArray
{
    public static Sprite[] GetSpritesArray(this Texture2D texture, bool alphabetical = true)
    {
        IEnumerable<Sprite> sprites =
            AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>();

        if (alphabetical)
            sprites = sprites.OrderBy(sprite => sprite.name);

        return sprites.ToArray();
    }
}
#endif
