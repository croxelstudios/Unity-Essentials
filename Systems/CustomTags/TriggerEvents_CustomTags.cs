using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TriggerEvents_CustomTags : TriggerEvents
{
    [SerializeField]
    CustomTagItems customTags = new CustomTagItems();

    protected override bool CheckCollision(GameObject other)
    {
        if (base.CheckCollision(other) && CheckCollisionCustom(other))
            return true;
        else return false;
    }

    bool CheckCollisionCustom(GameObject other)
    {
        //TO DO: This should work with this thing,
        //but unfortunately it causes issues when the object is deactivated in the same physics step
        //return customTag.Check(other);
        return customTags.Check_Dirty(other);
    }

    public void SetFirstCustomTag(int id)
    {
        customTags.SetFirstCustomTag(id);
#if UNITY_EDITOR
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
    }
}
