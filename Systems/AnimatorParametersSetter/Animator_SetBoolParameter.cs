using UnityEngine;

public class Animator_SetBoolParameter : MonoBehaviour
{
    [SerializeField] //TO DO: search on parent by default
    Animator animator = null;
    [SerializeField]
    string parameter = "Bool";

    public void SetBool(bool input)
    {
        if ((animator != null) && animator.isActiveAndEnabled)
            animator.SetBool(parameter, input);
    }

    void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
}
