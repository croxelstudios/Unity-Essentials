using Sirenix.OdinInspector;
using UnityEngine;

public class Animator_SetTriggerParameter : MonoBehaviour
{
    [SerializeField]
    bool useAvailableChildAnimator = false;
    [SerializeField]
    [EnableIf("@useAvailableChildAnimator == false")]
    Animator animator = null;
    [SerializeField]
    string trigger = "Die";
    
    public void SetTrigger()
    {
        if (useAvailableChildAnimator && ((animator == null) || animator.isActiveAndEnabled == false))
            animator = GetComponentInChildren<Animator>();
        if ((animator != null) && animator.isActiveAndEnabled)
        {
            animator.ResetTrigger(trigger);
            animator.SetTrigger(trigger);
        }
    }

    void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
}
