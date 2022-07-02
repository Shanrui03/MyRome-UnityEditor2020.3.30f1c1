using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class DistractionsScriptable
    {
        [Header("General")]
        [Space(-5)]
        [Tooltip("Make this NPC prone to distractions")]
        public bool alwaysUse = true;
        [Tooltip("Will automatically to turn the NPC to face the distraction noise")]
        public bool autoTurn = true;
        [Tooltip("Speed of turning")]
        public float turnSpeed = 3f;
        [Tooltip("The amount of time (seconds) to pass before reacting to the distraction. It's always best to keep it a little above 0 since it adds more realism")]
        [Range(0, 1)]
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
        [Tooltip("Play animation when turning. Animation will play according to reaction time above")]
        public bool useTurnAnimations;

        [Header("Normal State")]
        [Tooltip("The animation state name that will be called for turning right in normal state")]
        public string rightTurnAnimationNameNormal;
        [Tooltip("Transition time from any state to the right turn animation in normal state")]
        public float rightTurnTransitionNormal = 0.25f;
        
        [Space(5)]
        [Tooltip("The animation state name that will be called for turning left in normal state")]
        public string leftTurnAnimationNameNormal;
        [Tooltip("Transition time from any state to the left turn animation in normal state")]
        public float leftTurnTransitionNormal = 0.25f;

        [Header("Alert State")]
        [Tooltip("The animation state name that will be called for turning right in alert state")]
        public string rightTurnAnimationNameAlert;
        [Tooltip("Transition time from any state to the right turn animation in alert state")]
        public float rightTurnTransitionAlert = 0.25f;
        
        [Space(5)]
        [Tooltip("The animation state name that will be called for turning left in alert state")]
        public string leftTurnAnimationNameAlert;
        [Tooltip("Transition time from any state to the left turn animation in alert state")]
        public float leftTurnTransitionAlert = 0.25f;

        [Space(5)]
        [Tooltip("When the NPC reaches distraction location to check, use animation triggering")]
        public bool distractionCheckAnimation;
        [Tooltip("The animation state name to play when NPC reaches distraction location")]
        public string distractionCheckAnimationName;
        [Tooltip("Transition time from any state to the distraction check animation")]
        public float distractionCheckTransition = 0.25f;

        [Header("Scripts Enabling")]
        [Tooltip("Enable a certain script (Monobehaviour) when the NPC gets distracted. This script disables when NPC fully goes back to patroling")]
        public bool enableScript;

        [Header("Audios")]
        [Tooltip("Play an audio source when distracted. An example would be having a voice saying 'What is this?' OR 'I heard something'")]
        public bool playAudios;

        [Tooltip("Play an audio source when the NPC reaches the distraction point. An example would be having a voice saying 'Must be a cat or something' OR 'Hmmmm am I imagining things?")]
        public bool playDistractionSearchAudio;
    }
}


