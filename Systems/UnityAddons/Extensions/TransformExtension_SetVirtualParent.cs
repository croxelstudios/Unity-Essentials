using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public static class TransformExtension_SetVirtualParent
{
    static Dictionary<Transform, PCon> parentConstraints;

    struct PCon
    {
        public ParentConstraint constraint;
        public int offset;

        public PCon(ParentConstraint constraint, int offset)
        {
            this.constraint = constraint;
            this.offset = offset;
        }
    }

    public static void SetVirtualParent(this Transform origin, Transform parent)
    {
        PCon pCon;
        parentConstraints = parentConstraints.CreateIfNull();
        if (!parentConstraints.TryGetValue(origin, out pCon))
        {
            pCon = new PCon();
            pCon.constraint = origin.GetComponent<ParentConstraint>();
            if (pCon.constraint == null)
            {
                pCon.constraint = origin.gameObject.AddComponent<ParentConstraint>();
                pCon.offset = 0;

                pCon.constraint.locked = true;
                pCon.constraint.rotationAxis = Axis.None;
            }
            else pCon.offset = pCon.constraint.sourceCount;
            parentConstraints = parentConstraints.CreateAdd(origin, pCon);

            pCon.constraint.AddSource(new ConstraintSource());
        }
        bool prevState = pCon.constraint.constraintActive;
        pCon.constraint.constraintActive = false;
        if (parent != null)
        {
            pCon.constraint.SetSource(pCon.offset,
                new ConstraintSource() { sourceTransform = parent, weight = 1f });
            pCon.constraint.translationAtRest = origin.localPosition;
            pCon.constraint.rotationAtRest = origin.localEulerAngles;
            pCon.constraint.constraintActive = true;
        }
        else if (pCon.offset > 0)
        {
            pCon.constraint.SetSource(pCon.offset,
                new ConstraintSource() { sourceTransform = parent, weight = 0f });
            pCon.constraint.constraintActive = prevState;
        }
    }
}
