using UnityEngine;
using System.Collections.Generic;

namespace BlazeAISpace
{
    [System.Serializable]
    public class Waypoints
    {
        [Tooltip("On game start the NPC will instantly move onto the first waypoint instead of being idle first (works on both manual waypoints and randomize)")]
        public bool instantMoveAtStart = true;
        [Tooltip("Locations of the waypoints in world space. Will appear as green boxes at agent's location to tweak their locations visually but the [Randomize] property must be set to off.")]
        public Vector3[] waypoints;
        
        [Space(5)]
        [Tooltip("Set the idle rotation for each waypoint. Set the turning animations below. The rotation direction is shown in the scene view as red squares along the waypoints. If both the x and y are 0 then no rotation will occur and no red squares will appear.")]
        public Vector2[] waypointsRotation;
        [Tooltip("The amount of time in seconds to pass before turning to waypoint rotation.")]
        public float timeBeforeTurning = 0.2f;
        [Tooltip("Turning speed of waypoints rotations.")]
        public float turnSpeed = 2f;

        [Space(5)]
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

        // save inspector states
        bool inspectorLoop;
        bool inspectorRandomize;


        // GUI validation for the waypoint system
        public void WaypointsSystemValidation() 
        {
            if (randomize && loop) {
                randomize = !inspectorRandomize;
                loop = !inspectorLoop;
            }
            
            inspectorLoop = loop;
            inspectorRandomize = randomize;

            if (waypointsRotation != null) {
                Vector2[] arrCopy;
                arrCopy = new Vector2[waypointsRotation.Length];

                waypointsRotation.CopyTo(arrCopy, 0);
                waypointsRotation = new Vector2[waypoints.Length];

                for (var i=0; i<waypointsRotation.Length; i+=1) {
                    if (i <= arrCopy.Length-1) {
                        waypointsRotation[i] = arrCopy[i];
                        if (waypointsRotation[i].x > 0.5f) waypointsRotation[i].x = 0.5f;
                        if (waypointsRotation[i].y > 0.5f) waypointsRotation[i].y = 0.5f;
                        if (waypointsRotation[i].x < -0.5f) waypointsRotation[i].x = -0.5f;
                        if (waypointsRotation[i].y < -0.5f) waypointsRotation[i].y = -0.5f;
                    }
                }
            }
        }

        // Mark the waypoints in editor-view
        public void ShowWayPoints()
        {
            if (randomize) return;
            
            for (int i = 0; i < waypoints.Length; i++){
                
                if (i == 0) {
                    Gizmos.color = new Color(0, 0.4f, 0);
                }else{
                    Gizmos.color = new Color(0.6f, 1, 0.6f);
                }
                
                Gizmos.DrawCube(waypoints[i], new Vector3(0.5f, 0.5f, 0.5f));
                
                // Draws the waypoint rotation cubes
                if (waypointsRotation[i].x != 0 || waypointsRotation[i].y != 0) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(waypoints[i].x + waypointsRotation[i].x, waypoints[i].y, waypoints[i].z + waypointsRotation[i].y), new Vector3(0.3f, 0.3f, 0.3f));
                }

                if (waypoints.Length > 1)
                {
                    Gizmos.color = Color.blue;
                    if (i == 0)
                    {
                        Gizmos.DrawLine(waypoints[0], waypoints[1]);

                    }
                    else if (i == waypoints.Length - 1)
                    {
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                        Gizmos.color = Color.grey;
                        Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                    } 
                }
            }
        }
    }
}
