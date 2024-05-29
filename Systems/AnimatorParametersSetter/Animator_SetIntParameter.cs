using UnityEngine;

public class Animator_SetIntParameter : MonoBehaviour
{
    [SerializeField]
    Animator animator = null;
    [SerializeField]
    string parameter = "State";

    public void SetInteger(int input)
    {
        if ((animator != null) && animator.isActiveAndEnabled)
            animator.SetInteger(parameter, input);
    }

    void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
}
