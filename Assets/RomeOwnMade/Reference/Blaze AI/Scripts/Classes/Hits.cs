using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class Hits 
    {
        [Tooltip("The duration of being in the hit state (seconds).")]
        public float hitDuration = 0.8f;
        [Tooltip("If agent is going for an attack and got hit midway the attack will be cancelled and will return to idle-attack position. If false, after the hit the agent will continue on it's attack.")]
        public bool cancelAttackIfHit = true;

        [Header("Animations")]
        [Tooltip("Use animations on getting hit or not.")]
        public bool useAnimation = true;
        [Tooltip("The animation name to play when getting hit. You can access this property dynamically and set the animation name depending on where the NPC gets hit. So for example: if the player hits the npc on the head you can access this property and set the animation name to the 'head hit' animation.")]
        public string animationName;
        [Tooltip("The transition time to play this animation (seconds).")]
        public float animationTransition = 0.25f;
        [Min(0), Tooltip("Hit animation won't play again until this value has passed in seconds since the previous hit. Used to not make the agent look freezing by playing the same animation every frame with something repitive such as a machine gun. So this adds a gap, where the hit logic is applied but animation isn't played at the gap. This value can't be bigger than the hit duration property. Suggested to set to half the value of hit duration.")]
        public float animPlayGap = 0.4f;

        [Header("Audios")]
        [Tooltip("Do you want to play audios when getting hit?")]
        public bool useAudios;
        [Tooltip("Set this to a game object with several audio source components and one will be chosen and played at random. If it's empty, audio play will be ignored. If only one audio source component is added then that only one will be played.")]
        public GameObject AudiosObject;

        AudioSource currentAudio;
        AudioSource[] audios;

        [HideInInspector] public bool canPlayAnim = true;
        [HideInInspector] public float gapTimer = 0f;

        // play hit audio
        public void PlayAudio()
        {
            if (!useAudios || AudiosObject == null || (currentAudio != null && currentAudio.isPlaying)) return;

            // get audios once and cache
            if (audios == null) audios = AudiosObject.GetComponents<AudioSource>();

            if (audios.Length > 1) {
                currentAudio = audios[Random.Range(0, audios.Length)];
                currentAudio.Play();
            }else{
                if (audios.Length == 1) {
                    currentAudio = audios[0];
                    currentAudio.Play();
                }
            }
        }

        // stop the current audio playing
        public void StopAudio()
        {
            if (currentAudio != null && currentAudio.isPlaying) currentAudio.Stop();
        }

        public void Validate()
        {
            if (animPlayGap > hitDuration) animPlayGap = hitDuration - 0.2f;
        }
    }
}

