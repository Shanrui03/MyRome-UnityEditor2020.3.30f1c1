using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class Death
    {
        [Header("Animations")]
        [Tooltip("Do you want to play an animation when Death is triggered?")]
        public bool useAnimation;
        [Tooltip("The name of the animation state to play when dead.")]
        public string animationName;
        [Tooltip("The transition time from a current animation to this animation.")]
        public float animationTransition = 0.25f;

        [Header("Audios")]
        [Tooltip("If enabled an audio will be played when dead.")]
        public bool useAudio;
        [Tooltip("Set to a gameobject with multiple audio source components, one will be chosen and played at random. If only one is set then that only one will be played. If is NULL, audio will be ignored.")]
        public GameObject audioObject;

        [Header("Scripts Enabling")]
        [Tooltip("Do you want to enable a script when dead.")]
        public bool enableScript;
        [Tooltip("Script to enable when dead.")]
        public MonoBehaviour scriptToEnable;


        //play random death audio
        public void PlayAudio()
        {
            if (audioObject == null) return;

            AudioSource[] audios = audioObject.GetComponents<AudioSource>();

            if(audios.Length > 1) {
                audios[Random.Range(0, audios.Length)].Play();
            }else{
                if (audios.Length == 1){
                    audios[0].Play();
                }
            }
        }

        //trigger death script
        public void TriggerScript()
        {
            if (enableScript) {
                if (scriptToEnable != null) scriptToEnable.enabled = true;
            }
        }

        //disable death script
        public void DisableScript()
        {
            if (scriptToEnable != null) scriptToEnable.enabled = false;
        }
    }
}

