using UnityEngine;
using UnityEngine.Animations;

[ExecuteAlways]
[RequireComponent(typeof(ParentConstraint))]
public class SetParentConstraintByTag : MonoBehaviour
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
    UpdateMode updateMode = UpdateMode.DontUpdate;

    Transform target;

    enum UpdateMode { DontUpdate, Update, UpdateWhenNull }

    void OnEnable()
    {
        pcons = GetComponent<ParentConstraint>();

        if (updateMode != UpdateMode.Update) ResetSource();

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            ResetSource();
    }

    void Update()
    {
        if ((updateMode == UpdateMode.Update) ||
            ((updateMode == UpdateMode.UpdateWhenNull) &&
            (target == null)))
            ResetSource();
    }

    public void ResetSource()
    {
        target = GameObject.FindGameObjectWithTag(targetTag)?.transform;
        if (target != null)
            SetSource(target);
        else
            for (int i = 0; i < extraTags.Length; i++)
            {
                target = GameObject.FindGameObjectWithTag(extraTags[i])?.transform;
                if (target)
                {
                    SetSource(target);
                    break;
                }
            }
    }

    void SetSource(Transform target)
    {
        ConstraintSource source = pcons.GetSource(sourceIndex);
        source.sourceTransform = target;
        pcons.SetSource(sourceIndex, source);
    }
}
