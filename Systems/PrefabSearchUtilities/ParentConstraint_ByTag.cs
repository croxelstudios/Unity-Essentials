using UnityEngine;
using UnityEngine.Animations;

[ExecuteAlways]
[RequireComponent(typeof(ParentConstraint))]
public class ParentConstraint_ByTag : MonoBehaviour
{
    ParentConstraint pcons;

    [SerializeField]
    [TagSelector]
    string targetTag = "Player";
    [SerializeField]
    [TagSelector]
    string[] extraTags = null;
    [SerializeField]
    int sourceIndex = 0;
    [SerializeField]
    ByTagUpdateMode updateMode = ByTagUpdateMode.DontUpdate;

    Transform target;

    void OnEnable()
    {
        pcons = GetComponent<ParentConstraint>();
        ResetSource();
    }

    void Update()
    {
        if ((updateMode == ByTagUpdateMode.UpdateWhenNull) && (target == null))
            ResetSource();
    }

    public void ResetSource()
    {
        target = FindWithTag.Transform(targetTag, extraTags);
        SetSource(target);
    }

    void SetSource(Transform target)
    {
        ConstraintSource source = pcons.GetSource(sourceIndex);
        source.sourceTransform = target;
        pcons.SetSource(sourceIndex, source);
    }
}
