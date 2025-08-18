using UnityEngine;
using UnityEngine.Animations;

[ExecuteAlways]
[RequireComponent(typeof(IConstraint))]
public class Constraint_ByTag : BByTag<Transform>
{
    [SerializeField]
    int sourceIndex = 0;

    IConstraint pcons;

    void Reset()
    {
        targetTag = "Player";
        extraTags = null;
        updateMode = ByTagUpdateMode.DontUpdate;
    }

    protected override void InitIfNull()
    {
        if (pcons == null)
            pcons = GetComponent<IConstraint>();
    }

    protected override void SetSource(Transform target)
    {
        ConstraintSource source = pcons.GetSource(sourceIndex);
        source.sourceTransform = target;
        pcons.SetSource(sourceIndex, source);
    }
}
