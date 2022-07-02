using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class AlertStateScriptable
    {
        public bool useAlertStateOnStart = true;

        [Header("Movement")]
        [Tooltip("Speed of walking to waypoints")]
        public float moveSpeed = 2f;
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
        [Tooltip("Set to true to call animation states on idle and walking")]
        public bool useAnimations = false;
        
        [Space(5)]
        [Tooltip("Animation manager will play the animation state with this name")]
        public string idleAnimationName;
        [Tooltip("Transition time from any state to the idle animation")]
        public float idleAnimationTransition = 0.25f;

        [Space(5)]
        [Tooltip("Animation manager will play the animation state with this name")]
        public string moveAnimationName;
        [Tooltip("Transition time from any state to the move animation")]
        public float moveAnimationTransition = 0.1f;

        [Space(5)]
        [Tooltip("If enabled, when this NPC is idle in alert state it will play a random animation from the list below. For example: aiming his gun left and right or reloading his weapon")]
        public bool useRandomAnimationsOnIdle;
        [Tooltip("A list of all animation names you want to play randomly when idle. One will be randomly chosen and played. There is a 50/50 chance of random animations being played")]
        public string[] randomIdleAnimationNames;
        [Tooltip("Transition time from idle to one of the random animations played")]
        public float randomIdleAnimationTransition = 0.4f;

        [Header("Returning To Normal")]
        [Tooltip("When enabled, if this NPC is in alert state it will return back to normal state after a certain amount of time")]
        public bool returnToNormalState;
        [Tooltip("The amount of time in seconds to pass in alert state before returning to normal state")]
        public float timeBeforeReturningNormal = 100f;
        [Tooltip("The amount of time to pass in seconds in the returning to normal state before actually patroling waypoints. This is a headroom for playing the animations and sound")]
        public float returningDuration = 5f;

        [Space(5)]
        [Tooltip("Enabling this will play a special animation when the NPC goes back to normal state from an alert state")]
        public bool useAnimationOnReturn;
        [Tooltip("The name of the animation you want to play when the NPC goes back to normal state from an alert state")]
        public string animationNameOnReturn;
        [Tooltip("The time of transition for the animation on return")]
        public float animationOnReturnTransition;

        [Space(5)]
        [Tooltip("Enabling this will play a randomly chosen audio when returning to normal state; You can have an audio that says for example: 'he must've gone far, nothing to worry about'")]
        public bool playAudioOnReturn;

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
