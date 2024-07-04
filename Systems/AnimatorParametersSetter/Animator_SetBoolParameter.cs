using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Windows;

public class Animator_SetBoolParameter : MonoBehaviour
{
    [SerializeField]
    bool useAvailableChildAnimator = false;
    [SerializeField]
    [EnableIf("@useAvailableChildAnimator == false")]
    Animator animator = null;
    [SerializeField]
    string parameter = "Bool";

    public void SwitchBool()
    {
        UpdateNullAnimator();
        if ((animator != null) && animator.isActiveAndEnabled)
        {
            bool input = !animator.GetBool(parameter);
            animator.SetBool(parameter, input);
        }
    }

    public void SetBool(bool input)
    {
        UpdateNullAnimator();
        if ((animator != null) && animator.isActiveAndEnabled)
            animator.SetBool(parameter, input);
    }

    void UpdateNullAnimator()
    {
        if (useAvailableChildAnimator && ((animator == null) || animator.isActiveAndEnabled == false))
            animator = GetComponentInChildren<Animator>();
    }

    void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
}
