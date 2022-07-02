using UnityEngine;

namespace BlazeAISpace
{   
    [System.Serializable]
    public class SurprisedScriptable
    {
        [Tooltip("Using surprised will play special animation and sounds when the NPC is in NORMAL STATE and sees a hostile for the first time")]
        public bool useSurprised;

        [Tooltip("The time to pass in seconds before actually going into attack state")]
        public float surprisedDuration;

        [Tooltip("If checked the agent while in surprised emotion will always rotate to face the enemy.")]
        public bool alwaysRotate = false;

        [Header("Animations")]
        [Tooltip("Set whether you want to use animations or not")]
        public bool useAnimations;
        [Tooltip("Name of the animation state to play when the enemy is surprised")]
        public string surprisedAnimationName;
        [Tooltip("Transition time from the current animation to surprised animation")]
        public float surprisedAnimationTransition = 0.25f;
        
        [Header("Audios")]
        [Tooltip("Play special audio when seeing a hostile for the first time")]
        public bool useAudio;
    }
}

