using UnityEngine;

public class Animator_SetFloatParameter : MonoBehaviour
{
    [SerializeField]
    Animator animator = null;
    [SerializeField]
    string parameter = "Speed";
    [SerializeField]
    bool clampAnimationTime = true;

    public void SetFloat(float input)
    {
        if ((animator != null) && animator.isActiveAndEnabled)
        {
            if (clampAnimationTime)
                for (int i = 0; i < animator.layerCount; i++)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
                    animator.Play(stateInfo.shortNameHash, i, Mathf.Clamp01(stateInfo.normalizedTime));
                }

            animator.SetFloat(parameter, input);
        }
    }

    void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
}
