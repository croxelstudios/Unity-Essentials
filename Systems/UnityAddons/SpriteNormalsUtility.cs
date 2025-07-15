#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEditor;

public static class SpriteNormalsUtility
{
    [MenuItem("Tools/Correct Selected Sprite Normals")]
    public static void SetSelectedSpriteNormalsBackwards()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i] is Sprite sprite)
            {
                CorrectSpriteNormals(sprite);
                EditorUtility.SetDirty(sprite);
                AssetDatabase.SaveAssets();
                Debug.Log("Sprite normals set to Vector3.back and saved: " + sprite.name);
            }
            else Debug.LogWarning("Please select a Sprite asset in the Project view.");
        }
    }

    static void CorrectSpriteNormals(Sprite sprite)
    {
        int vertexCount = sprite.GetVertexCount();

        if (vertexCount == 0)
            return;

        NativeArray<Vector3> newNormals =
            new NativeArray<Vector3>(vertexCount, Allocator.Temp,
            NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < vertexCount; i++)
            newNormals[i] = Vector3.back;
        SpriteDataAccessExtensions.SetVertexAttribute(sprite, VertexAttribute.Normal, newNormals);
    }
}
#endif
