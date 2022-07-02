using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class WaypointsScriptable
    {
        [Tooltip("On game start the NPC will instantly move onto the first waypoint instead of being idle first (works on both manual waypoints and randomize)")]
        public bool instantMoveAtStart = true;
        [Tooltip("Setting this to true will loop the waypoints when patrolling, setting it to false will stop at the waypoint.")]
        public bool loop = false;
        [Tooltip("Enabling randomize will instead generate randomized waypoints within the navmesh in a continuous fashion.")]
        public bool randomize = true;

        [Space(5)]
        [Tooltip("The animation state name that will be called for turning right in normal state. If empty no animation will be played.")]
        public string rightTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning left in normal state. If empty no animation will be played.")]
        public string leftTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning right in alert state. If empty no animation will be played.")]
        public string rightTurnAnimAlert;
        [Tooltip("The animation state name that will be called for turning left in alert state. If empty no animation will be played.")]
        public string leftTurnAnimAlert;
        [Tooltip("Transition time from any state to the turning animation.")]
        public float turningAnimT = 0.25f;

        [Space(5), Tooltip("Movement turning will make the AI when in normal-alert states turn to the correct direction before moving and always turn to face the correct path. The turn speed is the property found above.")]
        public bool useMovementTurning = false;
        [Range(-1f, 1f), Tooltip("Movement turning will be used if the dot product between path corner and current position is equals to or less than this value. Best to keep it between 0.5 - 0.7.")]
        public float movementTurningSensitivity = 0.6f;
        

        //save inspector states
        bool inspectorLoop;
        bool inspectorRandomize;

        public void Validate() 
        {
            if (randomize && loop) {
                randomize = !inspectorRandomize;
                loop = !inspectorLoop;
            }

            inspectorLoop = loop;
            inspectorRandomize = randomize;
        }
    }
}

