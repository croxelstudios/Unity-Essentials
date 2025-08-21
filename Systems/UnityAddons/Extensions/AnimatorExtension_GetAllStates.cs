#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

public static class AnimatorExtension_GetAllStates
{
    public static AnimatorState[] Editor_GetAllStates(this Animator anim)
    {
        AnimatorController ac = anim.runtimeAnimatorController as AnimatorController;
        AnimatorControllerLayer[] acLayers = ac.layers;
        List<AnimatorState> allStates = new List<AnimatorState>();
        foreach (AnimatorControllerLayer i in acLayers)
        {
            ChildAnimatorState[] animStates = i.stateMachine.states;
            foreach (ChildAnimatorState j in animStates)
                allStates.Add(j.state);
        }
        return allStates.ToArray();
    }
}
#endif
