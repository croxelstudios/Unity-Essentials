using System.Collections.Generic;
using UnityEngine;

public static class SkinnedMeshRendererExtension_PopulateBones
{
    public static void PopulateBoneArray(this SkinnedMeshRenderer skinnedMesh,
        Transform otherRoot, Transform[] otherBones)
    {
        if (skinnedMesh.rootBone == null)
        {
            throw new System.Exception(
                "Missing root bone; please ensure that the root bone is set before attempting"
                + " to populate the bone array for the skinned mesh."
            );
        }
        else
        {
            List<Transform> boneArray = new List<Transform>();
            for (int i = 0; i < otherBones.Length; i++)
            {
                List<int> hierarchy = new List<int>();
                Transform parent = otherBones[i];
                while (parent != otherRoot)
                {
                    hierarchy.Add(parent.GetSiblingIndex());
                    parent = parent.parent;
                }

                Transform bone = skinnedMesh.rootBone;
                for (int j = hierarchy.Count - 1; j >= 0; j--)
                    bone = bone.GetChild(hierarchy[j]);
                boneArray.Add(bone);
            }

            skinnedMesh.bones = boneArray.ToArray();
        }

    }
}
