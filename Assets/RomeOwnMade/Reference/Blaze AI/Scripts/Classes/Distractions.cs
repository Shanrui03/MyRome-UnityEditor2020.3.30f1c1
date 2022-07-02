using UnityEngine;

namespace BlazeAISpace
{
    //properties for the distracted functionality
    [System.Serializable]
    public class Distractions
    {
        [Header("General")]
        [Space(-5)]
        [Tooltip("Make this NPC prone to distractions")]
        public bool alwaysUse = true;
        [Tooltip("Will automatically turn the NPC to face the distraction noise")]
        public bool autoTurn = true;
        [Tooltip("Speed of turning")]
        public float turnSpeed = 3f;
        [Tooltip("The amount of time (seconds) to pass before reacting to the distraction. It's always best to keep it a little above 0 since it adds more realism")]
        [Min(0)]
        public float turnReactionTime = 0.3f;
        [Tooltip("If enabled, when this NPC gets distracted it'll be in alert state and also alert others")]
        public bool turnAlertOnDistraction;

        [Header("Movement")]
        [Tooltip("If set to true the NPC will move to the distraction location, if false the NPC will only look towards the distraction location")]
        public bool moveToDistractionLocation = true;
        [Tooltip("When a group of enemies are distracted only one will be sent to check out the location. The enemy with the highest priority level in the group will be sent. If two enemies or more have the same priority level. The one closest will be sent.")]
        [Range(0f, 99f)]
        public float checkDistractionPriorityLevel = 1f;
        [Tooltip("The reaction time (seconds) to moving to the distraction location or continuing patrol if MoveToDistractionLocation is set to false")]
        [Range(0, 60)]
        public float moveToDistractionReactTime = 1.5f;
        [Tooltip("The amount of time to spend in the distraction location checking/guarding")]
        public float checkingTime = 3f;

        [Header("Animations")]
        [Tooltip("Play turn animations found in waypoints class.")]
        public bool useTurnAnimations;

        [Space(5)]
        [Tooltip("When the NPC reaches distraction location to check, use animation triggering")]
        public bool distractionCheckAnimation;
        [Tooltip("The animation state name to play when NPC reaches distraction location")]
        public string distractionCheckAnimationName;
        [Tooltip("Transition time from any state to the distraction check animation")]
        public float distractionCheckTransition = 0.25f;

        [Header("Scripts")]
        [Tooltip("Enable a certain script (Monobehaviour) when the NPC gets distracted. This script disables when NPC fully goes back to patroling")]
        public bool enableScript;
        [Tooltip("Script to enable when the NPC gets distracted")]
        public MonoBehaviour distractionScript;

        [Header("Audios")]
        [Tooltip("Play an audio source when distracted. An example would be having a voice saying 'What is this?' OR 'I heard something'")]
        public bool playAudios;
        [Tooltip("GameObject with several audio sources, will choose a random audio source to play when distracted. If only one is added that only one will be played. If empty, audio play will be ignored")]
        public GameObject distractedAudiosObject;

        [Tooltip("Play an audio source when the NPC reaches the distraction point. An example would be having a voice saying 'Must be a cat or something' OR 'Hmmmm am I imagining things?")]
        public bool playDistractionSearchAudio;
        [Tooltip("GameObject with several audio sources, will choose a random audio source to play when reaches distraction location. If only one is added that only one will be played. If empty, audio play will be ignored")]
        public GameObject distractionSearchAudiosObject;


        // the current audio being played within the distraction sub-system
        AudioSource currentAudio;
        AudioSource[] distractedAudios;
        AudioSource[] distractionSearchAudios;

        public bool inAttack { get; set; }

        bool _audioPlayed;
        public bool audioPlayed {
            get { return _audioPlayed; }
            set {
                _audioPlayed = value;
            }
        }

        //method responsible for playing distracted audios
        public void PlayDistractedAudios()
        {
            //if play distracted audio set to true, will choose a random audio source if length is more than 1
            if (playAudios && !audioPlayed && distractedAudiosObject != null && !inAttack) {
                
                distractedAudios = distractedAudiosObject.GetComponents<AudioSource>();
                if (distractedAudios.Length > 1) {
                    currentAudio = distractedAudios[Random.Range(0, distractedAudios.Length)];
                    currentAudio.Play();
                }else{
                    //play first one only
                    if (distractedAudios.Length == 1) {
                        currentAudio = distractedAudios[0];
                        currentAudio.Play();
                    }
                }

                audioPlayed = true;
            }
        }

        //used to stop the current audio within the distractions sub-system
        public void StopCurrentAudio()
        {
            if (currentAudio != null && currentAudio.isPlaying) currentAudio.Stop();
        }
        
        //to avoid having distraction audios playing on every single distraction
        //leave a time gap  to allow audios to be played again
        public System.Collections.IEnumerator EnableAudiosToBePlayedAgain()
        {
            yield return new WaitForSeconds(0.5f);
            audioPlayed = false;
        }

        //method responsible for playing audios when reaching distraction location
        public void TriggerDistractionSearchAudio()
        {
            if (distractionSearchAudiosObject == null) return;

            AudioSource[] distractionSearchAudios = distractionSearchAudiosObject.GetComponents<AudioSource>();
            
            //if play var is set to true, will choose a random audio source if length is more than 1
            if(distractionSearchAudios.Length > 1){
                currentAudio = distractionSearchAudios[Random.Range(0, distractionSearchAudios.Length)];
                currentAudio.Play();
            }else{
                //play first one only
                if(distractionSearchAudios.Length == 1){
                   currentAudio = distractionSearchAudios[0];
                   currentAudio.Play();
                }
            }
        }

        //get distraction direction (left/right)
        public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) 
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);
            
            if (dir > 0f) {
                return 1f;
            } else if (dir < 0f) {
                return -1f;
            } else {
                return 0f;
            }
        }

        //enable the script of distraction
        public void EnableScript()
        {
            if (enableScript) {
                if (distractionScript != null) distractionScript.enabled = true;
            }
        }

        //disable the script of distraction
        public void DisableScript() 
        {
            if (distractionScript != null) distractionScript.enabled = false;
        }
    }
}
