using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class NormalStateScriptable
    {
        public bool useNormalStateOnStart = true;

        [Header("Movement")]
        [Tooltip("Speed of walking to waypoints")]
        public float moveSpeed = 4f;
        [Tooltip("Speed of rotating")]
        public float rotationSpeed = 2.5f;

        [Header("Timing")]
        [Tooltip("The amount of time to wait before moving to the next waypoint")]
        public float waitTime = 5f;
        [Tooltip("Randomize the wait time between two values at every waypoint other than having a constant fixed value")]
        public bool randomizeWaitTime = false;
        [Tooltip("Randomize the wait time between two values (only works if the above [Randomize Wait Time] is set to true)")]
        public Vector2 randomizeWaitTimeBetween = new Vector2(0f, 0f);

        [Header("Animations")]
        [Tooltip("Set to true to call animation states for idle and walking")]
        public bool useAnimations = false;
        
        [Space(5)]
        [Tooltip("Animation manager will play the animation state with this name when idle")]
        public string idleAnimationName;
        [Tooltip("Transition time from any state to the idle animation")]
        public float idleAnimationTransition = 0.25f;
        
        [Space(5)]
        [Tooltip("Animation manager will play the animation state with this name when moving")]
        public string moveAnimationName;
        [Tooltip("Transition time from any state to the move animation")]
        public float moveAnimationTransition = 0.25f;

        [Space(5)]
        [Tooltip("If enabled, when this NPC is idle in normal state will play a random animation from the list below. For example: checking his gun or cleaning his boots")]
        public bool useRandomAnimationsOnIdle;
        [Tooltip("A list of all animation names you want to play randomly when idle. One will be randomly chosen and played. There is a 50/50 chance of random animations being played")]
        public string[] randomIdleAnimationNames;
        [Tooltip("Transition time from idle to one of the random animations played")]
        public float randomIdleAnimationTransition = 0.4f;

        [Header("Script Enabling")]
        [Tooltip("Set to true to enable/disable your custom scripts when idle (waypoint reached) and walking")]
        public bool enableScripts = false;

        [Header("Audios")]
        [Tooltip("Choose and play a random audio during patrol. Good for immersion, you can have a guard saying 'I should've stayed home, this is boring'")]
        public bool playAudiosOnPatrol = false;
        [Min(0), Tooltip("The amount of time in seconds to play a patrol audio. The value will be randomized between the two inputs. For a constant time, set the two inputs to the same value.")]
        public Vector2 playAudioEvery = new Vector2(5f, 30f);
    }
}

