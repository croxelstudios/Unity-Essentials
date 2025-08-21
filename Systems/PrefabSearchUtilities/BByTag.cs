using UnityEngine;
using Sirenix.OdinInspector;

public class BByTag<T> : MonoBehaviour where T : Component
{
    [SerializeField]
    [TagSelector]
    [OnValueChanged("EditorUpdate")]
    protected string targetTag = "MainCamera";
    [SerializeField]
    [TagSelector]
    [OnValueChanged("EditorUpdate", true)]
    protected string[] extraTags = null;
    [SerializeField]
    protected ByTagUpdateMode updateMode = ByTagUpdateMode.DontUpdate;

    T target;

    protected virtual void InitIfNull()
    {

    }

    protected virtual void OnEnable()
    {
        InitIfNull();
        ResetSource();
    }

    protected virtual void Update()
    {
        if ((updateMode != ByTagUpdateMode.DontUpdate) &&
            ((target == null) ||
            ((updateMode == ByTagUpdateMode.UpdateWhenNullOrInactive) &&
            (!(target.gameObject.activeInHierarchy && target.IsEnabled())))))
            ResetSource();
    }

    public void ResetSource()
    {
        target = FindWithTag.OnlyEnabled<T>(targetTag, extraTags);
        if (target != null) SetSource(target);
    }

    protected virtual void SetSource(T target)
    {
        InitIfNull();
    }

    protected virtual void EditorUpdate()
    {
        InitIfNull();
        ResetSource();
    }

    public void UpdateTag(string tag)
    {
        targetTag = tag;
        ResetSource();
    }
}
