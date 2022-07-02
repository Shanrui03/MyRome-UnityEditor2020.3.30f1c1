using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class DeathScriptable
    {
        [Header("Animations")]
        [Tooltip("Do you want to use an animation when dead?")]
        public bool useAnimation;
        [Tooltip("The name of the animation state to play when dead")]
        public string animationName;
        [Tooltip("The transition time from a current animation to this animation")]
        public float animationTransition = 0.25f;

        [Header("Audios")]
        [Tooltip("If enabled an audio will be played when dead")]
        public bool useAudio;

        [Header("Scripts Enabling")]
        [Tooltip("Do you want to enable a script when dead")]
        public bool enableScript;        
    }
}

