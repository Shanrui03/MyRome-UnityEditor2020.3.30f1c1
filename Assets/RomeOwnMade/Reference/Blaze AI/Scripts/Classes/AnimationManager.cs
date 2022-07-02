using UnityEngine;

namespace BlazeAISpace
{
    public class AnimationManager
    {
        string currentState;
        Animator anim;

        // constructor
        public AnimationManager (Animator animator)
        {
            anim = animator;
        }

        // actual animation playing function
        public void PlayAnimationState(string state, float time = 0.25f, bool shouldUse = true, bool overplay = false)
        {
            if (state == currentState || state.Length == 0 || !shouldUse) return;
            
            anim.CrossFadeInFixedTime(state, time, 0);

            if (overplay) currentState = "";
            else currentState = state;
        }
    }
}
