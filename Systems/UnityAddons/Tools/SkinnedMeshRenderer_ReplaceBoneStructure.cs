using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SkinnedMeshRenderer_ReplaceBoneStructure : MonoBehaviour
{
    [SerializeField]
    Transform newRootBone;
    [SerializeField]
    Mode mode = Mode.ByHierarchy;

    SkinnedMeshRenderer[] smr;

    enum Mode { ByHierarchy, ByName };

    [Button]
    public void ReplaceBones()
    {
        GetMeshRenderer();

        for (int id = 0; id < smr.Length; id++)
        {
            Transform[] newBones = new Transform[smr[id].bones.Length];

            for (int i = 0; i < smr[id].bones.Length; i++)
            {
                newBones[i] = newRootBone;
                if (smr[id].bones[i] != smr[id].rootBone)
                {
                    Dictionary<Transform, int> si = new Dictionary<Transform, int>();
                    switch (mode)
                    {
                        case Mode.ByName:
                            do
                            {
                                if (si.ContainsKey(newBones[i]))
                                    si[newBones[i]]++;
                                else si.Add(newBones[i], 0);
                                if (si[newBones[i]] >= newBones[i].childCount)
                                    newBones[i] = newBones[i].parent;
                                else newBones[i] = newBones[i].GetChild(si[newBones[i]]);
                            }
                            while (newBones[i].name != smr[id].bones[i].name);
                            break;
                        default:
                            si = new Dictionary<Transform, int>();
                            int parentCount = 0;
                            Transform currentParent = smr[id].bones[i];
                            do
                            {
                                Transform parent = currentParent.parent;
                                si.Add(parent, currentParent.GetSiblingIndex());
                                parentCount++;
                                currentParent = parent;
                            }
                            while (currentParent != smr[id].rootBone);

                            for (int j = 0; j < parentCount; j++)
                            {
                                newBones[i] = newBones[i].GetChild(si[currentParent]);
                                currentParent = currentParent.GetChild(si[currentParent]);
                            }
                            break;
                    }
                }
            }
            smr[id].bones = newBones;
            smr[id].rootBone = newRootBone;
        }
    }

    void GetMeshRenderer()
    {
        //if (smr == null)
        smr = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
}
