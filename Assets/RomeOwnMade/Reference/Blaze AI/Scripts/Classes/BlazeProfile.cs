using UnityEngine;
using UnityEditor;

namespace BlazeAISpace
{
    [CreateAssetMenu(fileName="Blaze Profile", menuName="Blaze AI/Blaze Profile")]
    public class BlazeProfile : ScriptableObject
    {
        [Space(20), Header("Set scripts & audio objects from Blaze component of the gameobject itself.")]

        public LayerMask groundLayers;
        public float pathRecalculationRate = 0.3f;
        public bool pathSmoothing = true;
        public float pathSmoothingFactor = 1f;
        public float proxyOffset = 0.7f;
        public bool enableGravity = true;
        public float gravityStrength = 10f;
        public bool useRootMotion = false;
        public Vector3 centerPosition;
        public bool showCenterPosition;
        
        [Space(10)]
        public bool avoidFacingObstacles = true;
        public float obstacleRayDistance = 3f;
        public Vector3 obstacleRayOffset;
        public LayerMask obstacleLayers;

        [Space(10)]
        public LayerMask layersToAvoid;
        
        [Space(10)]
        public WaypointsScriptable waypoints;
        
        [Space(10)]
        public VisionScriptable vision;

        [Space(10)]
        public NormalStateScriptable normalState;

        [Space(10)]
        public AlertStateScriptable alertState;

        [Space(10)]
        public AttackStateScriptable attackState;

        [Space(10)]
        public DistractionsScriptable distractions;

        [Space(10)]
        public HitsScriptable hits;

        [Space(10)]
        public DeathScriptable death;
        
        void OnValidate() 
        {
            if (vision.maxSightLevel < 0f) vision.maxSightLevel = 0f;
            if (vision.sightLevel < 0f) vision.sightLevel = 0f;

            if (attackState.coverShooterOptions.coverShooter) attackState.onAttackRotate = true;
            if (attackState.coverShooterOptions.coverShooter) vision.visionDuringAttackState.sightRange = attackState.distanceFromEnemy + attackState.coverShooterOptions.searchDistance;

            if (!alertState.useAlertStateOnStart && !normalState.useNormalStateOnStart) {
                normalState.useNormalStateOnStart = true;
            }

            waypoints.Validate();
            attackState.Validate();
        }
    }
}

