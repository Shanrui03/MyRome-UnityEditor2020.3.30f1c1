using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlazeAISpace;
using UnityEditor;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshObstacle))]

public class BlazeAI : MonoBehaviour {
    
    [Header("GENERAL")]
    [Tooltip("The navmesh layers that are walkable by the agent.")]
    public LayerMask groundLayers;
    
    [Tooltip("Path recalculation every seconds. The lower the number the more frequent the recalculation giving better quality of obstacle avoidance but more CPU intensive. It all depends on your game and target hardware.")]
    [Range(0.3f, 5f)]
    public float pathRecalculationRate = 0.45f;
    
    [Space(5)]
    [Tooltip("Set whether path smoothing should be enabled or disabled. Path smoothing makes turns through corners more realistic as a smooth curved motion.")]
    public bool pathSmoothing = true;
    [Min(1f), Tooltip("The strength of smoothing paths. TAKE NOTE: Too much smoothing can cause incorrect paths and weird behaviour. Recommended from 1f-2f.")]
    public float pathSmoothingFactor = 1f;

    [Space(5)]
    [Tooltip("The gameobject that will do the pathfinding and navigation. This object must be outside the boundries of the navmesh obstacle.")]
    public Transform pathFindingProxy;
    [Min(0f), Tooltip("The PathFindingProxy is the object responsible for pathfinding. Set the offset position to be in front and outside the boundries of the navmesh obstacle. The same offset will be used in negative value (behind) when moving backwards so take care of this. Use the edit screen to debug and see this as a green sphere.")]
    public float proxyOffset = 0.8f;

    [Space(5)]
    [Tooltip("Have a gravity effect to keep the character controller grounded.")]
    public bool enableGravity = true;
    [Min(0), Tooltip("The strength of the gravity.")]
    public float gravityStrength = 0.5f;

    [Space(5)]
    [Tooltip("Enabling this will make the agent use root motion, this gives more accurate and realistic movement related to the animation. If you want the basic speed of the root motion, set the movement speeds to 0.")]
    public bool useRootMotion = false;
    [Tooltip("The center position where the vision ray will cast from. Best to position at the pelvis.")]
    public Vector3 centerPosition;
    [Tooltip("Will show the center position as a red sphere in the scene view.")]
    public bool showCenterPosition = true;

    [Header("OBSTACLES")]
    [Tooltip("Setting this to true will prevent the AI facing too close to an obstacle at a specified distance from the end point. Good for realism, most of the times you don't want your NPC to stand facing an obstacle. The ray is shown in the editor view during game play.")]
    public bool avoidFacingObstacles = true;

    [Tooltip("The distance of the raycast to avoid facing an obstacles. When the ray hits an obstacle at the way point. It'll idle the agent.")]
    public float obstacleRayDistance = 3f;

    [Tooltip("Position the ray with an offset from the center that will detect the obstacles in front.")]
    public Vector3 obstacleRayOffset = Vector3.zero;

    [Tooltip("The layers of obstacles you want to avoid facing too closely.")]
    public LayerMask obstacleLayers;

    [Header("AVOIDANCE")]
    [Tooltip("Detect other Blaze AI agents and objects. When two Blaze AI agents touch one another during patrol one will wait to give room while the other walks by. Used in strafing too to avoid touching other agents.")]
    public LayerMask layersToAvoid;

    [Header("PATROL ROUTES")]
    public BlazeAISpace.Waypoints waypoints;

    [Header("CONE OF SIGHT AND RANGE")]
    public BlazeAISpace.Vision vision;

    [Header("NORMAL STATE")]
    [Tooltip("The agent has not seen any of the hostile tags in the Vision class.")]
    public BlazeAISpace.NormalState normalState;

    [Header("ALERT STATE")]
    [Tooltip("The agent has seen any of the alert tags or hostile tags in the Vision class but no longer thus becoming alert.")]
    public BlazeAISpace.AlertState alertState;

    [Header("ATTACK STATE")]
    [Tooltip("Attack state is when this agent finds an enemy (hostile tag) within it's cone of vision.")]
    public BlazeAISpace.AttackState attackState;

    [Header("DISTRACTIONS")]
    public BlazeAISpace.Distractions distractions;
    
    [Header("HITS & DAMAGE")]
    public BlazeAISpace.Hits hits;

    [Header("DEATH"), Tooltip("Trigger death by using the public method Death(). To get the agent back to 'alive state' use the public method Undeath().")]
    public BlazeAISpace.Death death;

    [Header("ADD PROFILE"), Tooltip("Add a Blaze profile with pre-set properties and options. To create a Blaze profile, right click in project window -> Create -> Blaze AI -> Blaze Profile.")]
    public BlazeProfile blazeProfile;
    [Tooltip("Keep this inspector synced with the profile. Changes can be done from the profile itself not the inspector.")]
    public bool profileSync = true;
    
    public BlazeProfile lastProfile { get; set; }


    #region System Variables

    public enum State 
    {
        normal,
        alert,
        attack,
        hit
    }

    public State state { get; set; }
    public int waypointIndex { get; set; }
    public bool reachedEnd { get; set; }
    public bool distracted { get; set; }
    public GameObject enemyToAttack { get; set; }
    public Vector3 checkEnemyPosition { get; set; }
    public float captureEnemyTimeStamp { get; set; }
    public bool alertedByOther { get; set; }
    
    public GameObject enemyScheduled { get; set; }
    public float stopPriority { get; set; }
    public bool stop { get; set; }
    public bool attackBackUp { get; set; }
    public bool crowdedAttack { get; set; }
    public Vector3 lastCheckedEnemyPosition { get; set; }
    public bool idleAttack { get; set; }
    public Vector3 reachedEndEnemyPosition { get; set; }
    public bool isAttacking { get; set; }
    public bool startCoverTimer { get; set; }
    public Transform currentCover { get; set; }
    public BlazeAI backedUpBy { get; set; }
    public Transform enemyCover { get; set; }
    public Vector3 enemyCoverHitPoint { get; set; }
    
    public BlazeAISpace.AnimationManager animationManager { get; set; }
    NavMeshObstacle agentObstacle;
    NavMeshAgent navmeshAgent;
    CharacterController controller;
    Animator anim;
    NavMeshPath path;
    CapsuleCollider capsuleCollider;
    Transform visionT;
    
    Queue<Vector3> cornerQueue = new Queue<Vector3>();
    Vector3 endPoint;
    bool normalStateActive;
    bool alertStateActive;
    bool attackStateActive;
    bool hasPath = false;
    Vector3 pathCorner = Vector3.zero;
    float pathFramesElapsed;
    bool wpIdleTriggered = false;
    bool wpRandomMode = false;
    bool activateRay = false;
    Vector3 distractionPosition;
    bool distractionTurn = false;
    bool passDistractionCheck;
    bool goingToDistractionPoint = false;
    bool waypointInterrupted;
    int visionFramesElapsed = 0;
    bool waitFrameRan = false;
    Vector3 enemyPosition = Vector3.zero;
    float returnNormalTimer = 0f; 
    float callOthersAttackTimer = 0f;
    bool isSeenVisionAlertTags;
    string seenVisionAlertTag;
    bool attackPreparationsLaunched = false;
    bool shouldAttack = false;
    float attackTimer = 0f;
    bool startAttackTimer;
    bool isHit = false;
    float stopTimer;
    int detectOthersFrames = 0;
    float controllerRadius;
    bool startIntervalTimer;
    float intervalTimer = 0f;
    float defaultDistanceFromEnemy;
    float defaultMoveBackwardsDist;
    float emptyEnemyTimer = 0f;
    Vector3 rootMotionDirection;
    bool forceTurn;
    float sphereDetectionFrame;
    bool goingToVisionAlertTag;
    bool startWaypointRotation;
    float waypointRotationAnimationTimer = 0f;
    int waypointTurnDir;
    int hideFrames = 0;
    float coverTimer = 0f;
    float actualCoverTime = 0f;
    bool coverLocationSet = false;
    bool findCoverFired = false;
    float coverHeight;
    Collider currentCoverColl;
    Vector3 coverNormal;
    bool tookCover = false;
    float attackDelayTimer = 0f;
    bool getEnemyCover = false;
    Vector3 enemyPositionOnAttack = Vector3.zero;
    float verticalVelocity = 0f;
    bool isAgent;
    bool agentChange = false;
    float agentChangeTimer = 0f;
    bool ranEnableAgent = false;
    float ranEnableAgentTimer = 0f;
    bool ranDisableAgent = false;
    float ranDisableAgentTimer = 0f;
    BlazeAI lastEnemyScript;
    Transform lastEnemy;
    bool turningToCorner = false;
    Vector3 enemyColPoint = Vector3.zero;
    bool turningToTargetInAttack;
    BlazeAIEnemyManager targetManager;
    bool strafing;
    float strafeTimer = 0f;
    float strafeTime;
    bool strafeWait;
    float strafeWaitTime;
    float strafeWaitTimer = 0f;
    bool strafeRan;
    bool abortStrafe;
    string currentStrafingDirection;
    bool currentStrafeDirBlocked;
    bool justEndedAttackState;
    float idleAttackTimer = 1f;
    Vector3 endPointBeforeDistraction;
    bool forceEndAttackState = false;
    bool noCoversFound = false;
    float attackMovementFrames = 0f;
    bool enemyPathNotReachable;
    bool canAttackEnemyUnreachable;
    Collider[] coverColls;
    Collider[] callOthersColl = new Collider[10];
    Collider[] visionColl = new Collider[20];
    List<Collider> enemiesToAttack = new List<Collider>();
    Collider[] callOthersToVisionColl = new Collider[15];
    Collider[] enemyCoverColl = new Collider[5];
    Collider[] sphereDetectionColl = new Collider[5];
    List<BlazeAI> sphereDetectedScripts = new List<BlazeAI>();
    Collider[] checkCoverOccupiedColl = new Collider[1];
    int checkCoverOccFrames = 0;
    int getEnemyCoverFrames = 0;
    int coverShooterAttackingFrames = 0;
    Collider enemyCol;
    bool calledAgentToLocationWhileDisabled;
    struct CallAgentToLocationInfo {
        public Vector3 location;
        public float time;
        public string animationName;
        public string stateToTurn;
    }
    CallAgentToLocationInfo CATLObject;
    float timerToTakeCover = 0f;
    
    #endregion
    
    #region Engine Methods

    // Start is called before the first frame update
    void Start()
    {
        agentObstacle = GetComponent<NavMeshObstacle>();
        navmeshAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        anim = GetComponent<Animator>();
        animationManager = new AnimationManager(anim);
        path = new NavMeshPath();
        
        AdjustComponentsStatesOnStart();
        ResetAllFlags();
        SetAgentObstacle();
        vision.buildDictionaryOfAlert();

        reachedEnd = false;
        idleAttackTimer = 1f;
        waypointIndex = 0;
        stopPriority = Random.Range(0f, 100f);
        endPoint = Vector3.zero;

        DisableAllSystemScripts();
        attackStateActive = false;

        defaultDistanceFromEnemy = attackState.distanceFromEnemy;
        defaultMoveBackwardsDist = attackState.moveBackwardsDist;
        
        attackState.RandomizeAttackIntervals();
        attackState.Validate();
        ValidateProperties();

        if (calledAgentToLocationWhileDisabled) {
            calledAgentToLocationWhileDisabled = false;
            CallAgentToLocation(CATLObject.location, CATLObject.time, CATLObject.animationName, CATLObject.stateToTurn);
        }else{
            // normal state if to be used on game start
            if (normalState.useNormalStateOnStart) {
                state = State.normal;
                if (waypoints.instantMoveAtStart) normalState.InstantMoveChangeVals();
                
                // setting the waypoints loop to true forces the agent to move
                // if waypoints length is only 1
                bool temp = waypoints.loop;
                if (waypoints.waypoints.Length == 1) {
                    waypoints.loop = true;
                }

                StartCoroutine(NormalStateIdle());
                waypoints.loop = temp;
            }else{
                // if not normal then alert state
                state = State.alert;
                if (waypoints.instantMoveAtStart) alertState.InstantMoveChangeVals();
                
                bool temp = waypoints.loop;
                if (waypoints.waypoints.Length == 1) {
                    waypoints.loop = true;
                }
                
                StartCoroutine(AlertStateIdle());
                waypoints.loop = temp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (vision.head) visionT = vision.head;
        else visionT = transform;

        // trigger vision checking
        VisionCheck();

        // check for functionalities
        FlaggedFunctions();

        if (!forceTurn) {
            // states and movement
            if (state == State.attack) AttackStateMovement();
            if (state == State.normal) NormalStateMovementTrigger();
            if (state == State.alert) AlertStateMovementTrigger();
        }
        
        AgentSpeeds();
        MainToClassesUpdate();

        isAttacking = startAttackTimer;

        // avoid facing an obstacle too closely when patrolling way points
        if (state != State.attack) {
            if (avoidFacingObstacles && activateRay && !reachedEnd) {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + obstacleRayOffset, transform.TransformDirection(Vector3.forward), out hit, obstacleRayDistance, obstacleLayers)) {
                    activateRay = false;
                    StopAgent();
                }

                // for debugging (seeing) the obstacle prevention ray - can delete
                Debug.DrawRay(transform.position + obstacleRayOffset, transform.TransformDirection(Vector3.forward) * obstacleRayDistance, Color.yellow);
            }
        }else{
            activateRay = false;
        }
        
        if (state != State.attack && state != State.hit && calledAgentToLocationWhileDisabled) {
            calledAgentToLocationWhileDisabled = false;
            CallAgentToLocation(CATLObject.location, CATLObject.time, CATLObject.animationName, CATLObject.stateToTurn);
        }
    }

    // validate GUI properties
    void OnValidate()
    {
        if (state == State.attack) return;

        GetComponent<NavMeshAgent>().enabled = false;

        if (path != null) {
            waypointInterrupted = true;
            StateWalk();
        }
        
        if (waypoints != null) {
            waypoints.WaypointsSystemValidation();
            if (waypoints.waypoints.Length > 0) {
                for (int i=0; i<waypoints.waypoints.Length; i++) {
                    if (waypoints.waypoints[i] == Vector3.zero) waypoints.waypoints[i] = transform.position;
                }
            }
        }

        StatesInspectorValidation();
        
        if (attackState != null) {
            attackState.Validate();
            if (attackState.coverShooterOptions.coverShooter) {
                vision.visionDuringAttackState.sightRange = attackState.distanceFromEnemy + attackState.coverShooterOptions.searchDistance;
                attackState.onAttackRotate = true;
                attackState.strafe = false;
            }
        }

        if (pathFindingProxy) pathFindingProxy.transform.localPosition = new Vector3(0f, 0f, proxyOffset);

        if (useRootMotion) GetComponent<Animator>().updateMode = AnimatorUpdateMode.AnimatePhysics;
        else GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;

        if (vision != null) {
            if (vision.visionDuringAttackState.sightRange <= attackState.distanceFromEnemy) {
                vision.visionDuringAttackState.sightRange = attackState.distanceFromEnemy + 1f;
            }
        }

        if (hits != null) hits.Validate();
        if (lastProfile != blazeProfile) LoadProfile(blazeProfile);
    }

    // use Gizmos to help debugging in editor view
    void OnDrawGizmosSelected()
    {
        if (vision.head) visionT = vision.head;
        else visionT = transform;

        //show the vision spheres
        vision.ShowVisionSpheres(visionT);

        //debug the ray cast
        if (!Application.isPlaying && avoidFacingObstacles) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + obstacleRayOffset, transform.position + obstacleRayOffset + transform.TransformDirection(Vector3.forward) * obstacleRayDistance);
        }

        //show the waypoints
        waypoints.ShowWayPoints();

        if (pathFindingProxy != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pathFindingProxy.transform.position, 0.2f);
        }

        if (GetComponent<CharacterController>() != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0f, GetComponent<CharacterController>().height/2f, 0f), attackState.callRadius);
        }

        attackState.CoverShooterGizmos(transform);

        if (showCenterPosition) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + centerPosition, 0.2f);
        }

        // draw a red ray to target
        if (state == State.attack && enemyToAttack) {
            Debug.DrawRay(transform.position + centerPosition, (enemyColPoint - (transform.position + centerPosition)), Color.red, 0.1f);
        }
    }

    // animator motion
    void OnAnimatorMove()
    {
        if (useRootMotion) {
            rootMotionDirection = anim.deltaPosition / Time.deltaTime;

            // set agent velocity to root motion direction
            navmeshAgent.velocity = rootMotionDirection;
            if (!navmeshAgent.enabled && !shouldAttack) transform.position = new Vector3(anim.rootPosition.x, transform.position.y, anim.rootPosition.z);
        }
    }

    #endregion
    
    #region Normal State
    
    // waypoint reached and now should go idle
    // enabling/disabling scripts/animations and waypoints
    IEnumerator NormalStateIdle(bool overrideStop = false)
    {
        if (!overrideStop) DisableAgent();
        
        // if checking distraction point, and property is checked, 
        // trigger special animation when reached else play normal animations
        if (goingToDistractionPoint) {
            if (!overrideStop) {
                //if went to distraction location and no check distraction animation
                //although set to true then use the idle animation of waypoint
                if (distractions.distractionCheckAnimation) {
                    if (distractions.distractionCheckAnimationName.Length > 0)
                        animationManager.PlayAnimationState(distractions.distractionCheckAnimationName, distractions.distractionCheckTransition);
                    else
                        animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition);
                }else{
                    animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
                }
            }else{
                animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
            }
        }else{
            // trigger random idle animation if set to do so
            if (normalState.useRandomAnimationsOnIdle && (normalState.randomIdleAnimationNames.Length > 0) && !distracted && !overrideStop && !normalState.instantMoveChange) {
                
                // generate a chance
                int num = Random.Range(0, 10);

                // play random animation if random number is bigger than 5
                if(num >= 5){
                    string randomAnimation = normalState.randomIdleAnimationNames[Random.Range(0, normalState.randomIdleAnimationNames.Length)];
                    animationManager.PlayAnimationState(randomAnimation, normalState.randomIdleAnimationTransition);
                }else{
                    animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
                }

            }else{
                // trigger idle animation
                animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
            }
        }

        alertState.DisableScripts();
        normalState.TriggerScripts("idle");

        state = State.normal;
        wpIdleTriggered = true;

        startWaypointRotation = false;
        waypointRotationAnimationTimer = 0f;

        if (overrideStop) yield break;

        // get the wait time for later use depending on checked properties
        float waitTime;
        
        if (normalState.randomizeWaitTime) {
            waitTime = Random.Range(normalState.randomizeWaitTimeBetween.x, normalState.randomizeWaitTimeBetween.y); 
        } else {
            waitTime = normalState.waitTime;
        }

        if (goingToDistractionPoint) waitTime = distractions.checkingTime;
        yield return new WaitForSeconds(waitTime);

        FromIdleToWalk();
    }

    // triggers the movement and idle of the normal state
    void NormalStateMovementTrigger()
    {
        if (forceTurn || turningToCorner) return;

        callOthersAttackTimer = 0f;
        attackState.distanceFromEnemy = defaultDistanceFromEnemy;

        if (normalStateActive && !distracted) {
            if (reachedEnd) {
                if (WaypointRotationCheck() && !waypointInterrupted) TriggerWaypointRotation();
                else {
                    if (!wpIdleTriggered) {
                        StopAllCoroutines();
                        StartCoroutine(NormalStateIdle());
                    }
                }
            }else{
                wpIdleTriggered = false;
                animationManager.PlayAnimationState(normalState.moveAnimationName, normalState.moveAnimationTransition, normalState.useAnimations);  
                MoveToDestination(endPoint);
            }
        }
        
        if (normalState.playAudiosOnPatrol && (normalState.currentAudio != null && !normalState.currentAudio.isPlaying)) {
            normalState.audioPlayTimer += Time.deltaTime;
            if (normalState.audioPlayTimer >= normalState.audioRandomTime && !distracted) {        
                StopSystemsAudios("normal");
                normalState.PlayRandomPatrolAudio();
            }
        }
    }

    #endregion

    #region Alert State
    
    // alert state idle
    public IEnumerator AlertStateIdle(bool overrideStop = false)
    {
        if (!overrideStop) DisableAgent();

        if (overrideStop) {
            StopMovement();
        }
        
        // if checking distraction point, and property is checked, 
        // trigger special animation when reached else play normal animations
        if (goingToDistractionPoint) {
            
            if (!overrideStop) {
                //if went to distraction location and no check distraction animation
                //although set to true then use the idle animation of waypoint
                if (distractions.distractionCheckAnimation) {

                    if (distractions.distractionCheckAnimationName.Length > 0)
                        animationManager.PlayAnimationState(distractions.distractionCheckAnimationName, distractions.distractionCheckTransition, distractions.distractionCheckAnimation);
                    else
                        animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
                
                }else{
                    animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
                }

            }else{
                animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
            }

        }else{
            // trigger random idle animation if set to do so
            if (alertState.useRandomAnimationsOnIdle && (alertState.randomIdleAnimationNames.Length > 0) && !isSeenVisionAlertTags && !overrideStop && !alertState.instantMoveChange) {
                
                // generate a chance
                int num = Random.Range(0, 10);
                
                // play random animation if random number is bigger than 5
                if (num >= 5) {
                    string randomAnimation = alertState.randomIdleAnimationNames[Random.Range(0, alertState.randomIdleAnimationNames.Length)];
                    animationManager.PlayAnimationState(randomAnimation, alertState.randomIdleAnimationTransition);
                }else{
                    animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
                }

            }else{
                // trigger idle animation
                if (!isSeenVisionAlertTags) animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
            }

            // if got alert by vision alert tags and need to play an animation at the location
            if (isSeenVisionAlertTags) {
                if (vision.alertTagsDict[seenVisionAlertTag].animationName.Length > 0)
                    animationManager.PlayAnimationState(vision.alertTagsDict[seenVisionAlertTag].animationName, alertState.moveAnimationTransition);
                else
                    animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
            }
        }

        normalState.DisableScripts();
        alertState.TriggerScripts("idle");
        
        state = State.alert;
        wpIdleTriggered = true;

        startWaypointRotation = false;
        waypointRotationAnimationTimer = 0f;

        // get the wait time for later use depending on checked properties
        float waitTime;

        if (alertState.randomizeWaitTime) waitTime = Random.Range(alertState.randomizeWaitTimeBetween.x, alertState.randomizeWaitTimeBetween.y); 
        else waitTime = alertState.waitTime;

        // override previous wait times if seen vision alert tags
        if (isSeenVisionAlertTags) waitTime = vision.alertTagsDict[seenVisionAlertTag].time;
        if (goingToDistractionPoint) waitTime = distractions.checkingTime;

        yield return new WaitForSeconds(waitTime);

        if (checkEnemyPosition != Vector3.zero) {
            TurnToAttack();
            yield break;
        }

        FromIdleToWalk();
    }

    // triggers the movement and idle of the alert state
    void AlertStateMovementTrigger()
    {
        if (forceTurn || turningToCorner) return;

        callOthersAttackTimer = 0f;
        attackState.distanceFromEnemy = defaultDistanceFromEnemy;
        attackStateActive = false;

        // movement
        if (alertStateActive && !distracted) {
            if (reachedEnd) {
                if (WaypointRotationCheck() && !waypointInterrupted) TriggerWaypointRotation();
                else {
                    if (!wpIdleTriggered) {
                        StopAllCoroutines();
                        StartCoroutine(AlertStateIdle());
                    }
                }
            }else{
                animationManager.PlayAnimationState(alertState.moveAnimationName, alertState.moveAnimationTransition, alertState.useAnimations);
                wpIdleTriggered = false;
                MoveToDestination(endPoint);
            }
        }

        // patrol audio
        if (alertState.playAudiosOnPatrol && (alertState.currentAudio != null && !alertState.currentAudio.isPlaying)) {
            alertState.audioPlayTimer += Time.deltaTime;
            if (alertState.audioPlayTimer >= alertState.audioRandomTime && !distracted) {
                StopSystemsAudios("alert");
                alertState.PlayRandomPatrolAudio();
            }
        }
    }
    
    // count down timer to get back to normal state from alert state
    void ReturnNormalTimer()
    {
        if (state == State.alert) {
            if (alertState.returnToNormalState && !goingToDistractionPoint) {
                returnNormalTimer += Time.deltaTime;
                if (returnNormalTimer >= alertState.timeBeforeReturningNormal) {
                    
                    state = State.normal;

                    attackStateActive = false;
                    alertStateActive = false;
                    normalStateActive = false;

                    StopAllCoroutines();
                    
                    returnNormalTimer = 0f;
                    StartCoroutine(PrepareReturn());
                }
            }
        }else{
            returnNormalTimer = 0f;
        }
    }

    // preparing to return to normal state from alert state
    IEnumerator PrepareReturn()
    {
        // play audio
        if (alertState.playAudioOnReturn) {
            alertState.ChooseRandomAudioOnReturn();
        }

        // check if to use animations
        if (alertState.useAnimationOnReturn) {
            animationManager.PlayAnimationState(alertState.animationNameOnReturn, alertState.animationOnReturnTransition);
        }else{
            animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
        }

        state = State.normal;
        attackState.surprised.isSurprised = false;

        yield return new WaitForSeconds(alertState.returningDuration);

        BackToNormal();
    }

    // trigger going back to normal
    void BackToNormal()
    {
        state = State.normal;
                    
        alertStateActive = false;
        normalStateActive = true;
        checkEnemyPosition = Vector3.zero;

        lastCheckedEnemyPosition = Vector3.zero;
        returnNormalTimer = 0f;

        float temp = normalState.waitTime;
        normalState.waitTime = 1f;

        StartCoroutine(NormalStateIdle());
        normalState.waitTime = temp;
    }

    // call nearby agents to vision alert location
    void CallOthersToVisionAlert()
    {
        int callOthersToVisionInt = Physics.OverlapSphereNonAlloc(transform.position + centerPosition, attackState.callRadius, callOthersToVisionColl, attackState.agentLayersToCall);
        for (int i=0; i<callOthersToVisionInt; i++) {
            if (callOthersToVisionColl[i].transform != transform) {
                BlazeAI script = callOthersToVisionColl[i].GetComponent<BlazeAI>();
                if (script != null) {
                    if (!script.attackState.receiveCallFromOthers) return;
                    if (script.enemyToAttack != null) return;

                    if (!script.enemyToAttack || script.state != State.attack) {
                                     
                        if (script.state == State.hit) return;

                        // if alerting vision tag
                        if (isSeenVisionAlertTags) {
                            if (vision.alertTagsDict[seenVisionAlertTag].callOthersToLocation) {
                                script.endPoint = endPoint;
                                script.ChangeState("alert", true);
                            }else{
                                script.ChangeState("alert");
                            }
                        }
                    }
                }
            }
        }
    }
    
    #endregion

    #region Attack State
    
    // logic of attack state movement functions
    void AttackStateMovement()
    {
        // attack state movement
        if (attackStateActive) {

            // check if target tag changed to something non-hostile
            if (enemyToAttack) {
                if (CheckTargetTagChanged()) {
                    EndAttackState();
                    return;
                }
                ScheduleToEnemy();
                forceEndAttackState = false;
            } else {
                ResetStrafing();
            }

            // if AI vs AI -> remove carve
            if (!findCoverFired) CarveAccordingToEnemy();

            if (attackState.coverShooterOptions.coverShooter) {
                ResetStrafing();
                currentStrafeDirBlocked = false;
                
                if (enemyToAttack) {
                    if (shouldAttack) {
                        if (!targetManager.callEnemies) shouldAttack = false;

                        RaycastHit hit;
                        Vector3 startDir = transform.position + centerPosition;
                        Vector3 dir = enemyPosition - startDir;
                        
                        // move to attack distance
                        var distance = (new Vector3(enemyToAttack.transform.position.x, transform.position.y, enemyToAttack.transform.position.z) - transform.position).sqrMagnitude;
                        if (distance <= attackState.attackDistance * attackState.attackDistance) {
                            int layers = vision.hostileAndAlertLayers | attackState.coverShooterOptions.coverLayers | obstacleLayers | attackState.layersCheckBeforeAttacking;
                            
                            if (getEnemyCover) {
                                if (enemyCover != null) {
                                    if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, layers)) {
                                        if (enemyToAttack.transform.IsChildOf(hit.transform)) {
                                            enemyCover = null;
                                            attackDelayTimer += Time.deltaTime;
                                            if (tookCover) {
                                                if (attackDelayTimer >= 0.15f) Attack();
                                                else MoveFromCover(true);
                                            } else Attack();
                                        }else{
                                            if (enemyCover.IsChildOf(hit.transform)) {
                                                enemyCoverHitPoint = hit.point;
                                                attackDelayTimer += Time.deltaTime;
                                                if (tookCover) {
                                                    if (attackDelayTimer >= 0.15f) Attack();
                                                    else MoveFromCover(true);
                                                } else Attack();
                                            }else{
                                                MoveFromCover(true);
                                            }
                                        }
                                    }
                                }else{
                                    if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, layers)) {
                                        if (enemyToAttack.transform.IsChildOf(hit.transform)) {
                                            attackDelayTimer += Time.deltaTime;
                                            if (tookCover) {
                                                if (attackDelayTimer >= 0.15f) Attack();
                                                else MoveFromCover(true);
                                            } else Attack();
                                        }else{
                                            MoveFromCover(true);
                                        }
                                    }
                                }
                            }else{
                                enemyCover = null;
                                if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, layers)) {
                                    if (enemyToAttack.transform.IsChildOf(hit.transform)) {
                                        attackDelayTimer += Time.deltaTime;
                                        if (tookCover) {
                                            if (attackDelayTimer >= 0.15f) Attack();
                                            else MoveFromCover(true);
                                        } else Attack();
                                    }else{
                                        MoveFromCover(true);
                                    }
                                }
                            }
                            
                        }else{
                            MoveFromCover();
                        }
                    }else{
                        if (coverLocationSet) {
                            if (CoverOccupied(currentCover)) coverLocationSet = false;
                            else MoveToCover();
                        }else{
                            NormalAttackMovement();
                            FindCover(true);
                        }
                    }
                }else{
                    if (coverLocationSet) MoveToCover();
                    else NormalAttackMovement();
                }
                
            }else{
                NormalAttackMovement();
            }

            // alert others when in attack state
            callOthersAttackTimer += Time.deltaTime;
            if (callOthersAttackTimer >= attackState.callOthersTime) {
                callOthersAttackTimer = 0f;
                CallSurrounding();
            }
        }
    }
    
    // non-cover shooter movement
    void NormalAttackMovement()
    {   
        coverLocationSet = false;
        findCoverFired = false;

        // vision detected an enemy
        if (enemyToAttack) {
            float distance = (new Vector3(enemyToAttack.transform.position.x, transform.position.y, enemyToAttack.transform.position.z) - transform.position).sqrMagnitude;
            float minDistance = 0f;
            float backupDistTemp = 0f;
            lastCheckedEnemyPosition = endPoint;
            
            // start the interval timer if attack in intervals mode is on
            if (attackState.attackInIntervals) {
                if (!shouldAttack && targetManager.callEnemies) startIntervalTimer = true;
            }

            // change the min distance according to should attack and scheduler
            if (targetManager.callEnemies) {
                if (shouldAttack) {
                    ResetStrafing();
                    currentStrafeDirBlocked = false;

                    backupDistTemp = 0f;
                    minDistance = attackState.attackDistance;
                    attackState.distanceFromEnemy = defaultDistanceFromEnemy;
                    attackState.moveBackwardsDist = defaultMoveBackwardsDist;
                }else{
                    if (strafing) minDistance = attackState.distanceFromEnemy + 1f;
                    else minDistance = attackState.distanceFromEnemy;

                    if (attackState.moveBackwards) backupDistTemp = attackState.moveBackwardsDist - 0.35f;
                }
            }else{
                startIntervalTimer = false;
                intervalTimer = 0f;
                shouldAttack = false;

                if (strafing) minDistance = attackState.distanceFromEnemy + 1f;
                else minDistance = attackState.distanceFromEnemy;
                
                if (attackState.moveBackwards) backupDistTemp = attackState.moveBackwardsDist - 0.35f;
            }

            // don't move if turning to face enemy
            if (turningToTargetInAttack && !shouldAttack) {
                IdleAttackState();
                return;
            }

            // check if path to enemy is reachable every path recalculation
            if (attackMovementFrames >= pathRecalculationRate) {
                attackMovementFrames = 0;
                if (!IsPathReachable(enemyPosition)) {
                    if (distance > minDistance * minDistance) {
                        shouldAttack = false;
                        abortStrafe = true;
                        ResetStrafing();
                        AttackPosition();
                    }else{
                        abortStrafe = true;
                        ResetStrafing();
                        AttackPosition();
                    }
                    enemyPathNotReachable = true;
                    return;
                }else{
                    abortStrafe = false;
                    enemyPathNotReachable = false;
                }
            } else {
                attackMovementFrames += Time.deltaTime;
                if (enemyPathNotReachable) {
                    AttackPosition();
                    return;
                }
            }
            
            // move to distance
            if (distance + 0.3f > (backupDistTemp * backupDistTemp)) {
                if (distance > minDistance * minDistance) {
                    
                    // don't move if distance difference is 2f or less from minimum distance
                    if (!shouldAttack && idleAttack) {
                        float temp = distance - (minDistance * minDistance);
                        if (temp <= minDistance + 2f) {
                            AttackPosition();
                            return;
                        }
                    }

                    // must atleast pass 0.3 seconds in idle state before moving again
                    if (!shouldAttack && idleAttackTimer < 0.3f) {
                        AttackPosition();
                        return;
                    }

                    ResetStrafing();
                    currentStrafeDirBlocked = false;

                    attackState.distanceFromEnemy = defaultDistanceFromEnemy;
                    attackState.moveBackwardsDist = defaultMoveBackwardsDist;                  
                    
                    if (startAttackTimer) return;

                    attackBackUp = false; 
                    wpIdleTriggered = false;
                    idleAttack = false;
                    startIntervalTimer = false;
                    backedUpBy = null;
                    
                    animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);                    
                    CallPathSetup();
                    MoveToDestination(enemyToAttack.transform.position);
                }else{
                    if (attackState.coverShooterOptions.coverShooter && getEnemyCover && enemyCover != null) {
                        RaycastHit hit;
                        Vector3 dir = enemyToAttack.transform.position - transform.position;
                        int layers = vision.hostileAndAlertLayers | vision.layersToDetect | attackState.layersCheckBeforeAttacking | attackState.coverShooterOptions.coverLayers | obstacleLayers;
                        if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity)) {
                            if (hit.transform == enemyCover.transform || hit.transform == enemyToAttack.transform) {
                                AttackPosition();
                            }
                        }
                    }else{
                        AttackPosition();
                    }
                }
            }else{
                BackupMovement();
            }

        }else{
            idleAttackTimer = 1f;
    
            // if enemy position has been told by another agent
            if (checkEnemyPosition != Vector3.zero) {
                if (coverLocationSet) return;

                attackBackUp = false;
                idleAttack = false;
                tookCover = false;
                coverLocationSet = false;
                startIntervalTimer = false;
                lastCheckedEnemyPosition = checkEnemyPosition;
                
                RemoveFromEnemyScehduler();
                ResetAttackingTimer();

                float distance = (new Vector3(checkEnemyPosition.x, transform.position.y, checkEnemyPosition.z) - transform.position).sqrMagnitude;
                float minDistance = 2f;

                if (distance > (minDistance * minDistance)) {
                    animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);
                    reachedEnd = false;
                    MoveToDestination(checkEnemyPosition);
                    wpIdleTriggered = false;
                    backedUpBy = null;
                }else{
                    reachedEnd = true;
                    attackBackUp = false;
                    waitFrameRan = false;
                    
                    reachedEndEnemyPosition = checkEnemyPosition;
                    checkEnemyPosition = Vector3.zero;
                    lastCheckedEnemyPosition = Vector3.zero;
                    FromAttackStateReturnAlert();
                }

            }else{
                if (startAttackTimer || forceEndAttackState) return;
            
                attackState.distanceFromEnemy = defaultDistanceFromEnemy;
                attackState.moveBackwardsDist = defaultMoveBackwardsDist;
                float distance = Vector3.Distance(enemyPosition, transform.position);
                
                idleAttack = false;
                attackBackUp = false;
                startAttackTimer = false;
                tookCover = false;
                coverLocationSet = false;
                startIntervalTimer = false;
                backedUpBy = null;

                if (!attackState.coverShooterOptions.coverShooter) shouldAttack = false;
                RemoveFromEnemyScehduler();

                // if path is unreachable
                if (attackMovementFrames >= pathRecalculationRate) {
                    attackMovementFrames = 0;
                    if (!IsPathReachable(enemyPosition)) {
                        if (!canAttackEnemyUnreachable) {
                            FromAttackStateReturnAlert();
                            return;
                        }
                    }
                } else attackMovementFrames++;

                if (distance > navmeshAgent.radius * 2 + 0.3f) {
                    FixProxy();
                    animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);
                    MoveToDestination(enemyPosition);
                }else{
                    EndAttackState();
                }
            }
        }
    }

    // call other agents to attack situation
    void CallSurrounding()
    {
        // if not set to call others, quit
        if (!attackState.callOthers) return;

        int callOthersCollNum = Physics.OverlapSphereNonAlloc(transform.position + centerPosition, attackState.callRadius, callOthersColl, attackState.agentLayersToCall);

        for (int i=0; i<callOthersCollNum; i++) {
            if (callOthersColl[i].transform != transform) {
                BlazeAI script = callOthersColl[i].GetComponent<BlazeAI>();
                if (script != null) {
                     // quit if neighbouring agent doesn't receive alerts, already has a target or AI against me
                    if (!script.attackState.receiveCallFromOthers) return;
                    if (enemyToAttack) {
                        if (enemyToAttack.transform == script.transform) return;
                    }

                    if ((enemyToAttack && !script.enemyToAttack && ((script.captureEnemyTimeStamp < captureEnemyTimeStamp) || checkEnemyPosition != Vector3.zero))) {
                        if (enemyToAttack) {
                            script.checkEnemyPosition = ValidateEnemyYPoint(enemyToAttack.transform.position);
                            script.attackState.surprised.isSurprised = true;
                            if (script.state != State.attack) script.TurnToAttack(true);
                        }else{
                            if (checkEnemyPosition != Vector3.zero && script.lastCheckedEnemyPosition != checkEnemyPosition && checkEnemyPosition != script.reachedEndEnemyPosition && captureEnemyTimeStamp > script.captureEnemyTimeStamp) {
                                script.checkEnemyPosition = checkEnemyPosition;
                                script.attackState.surprised.isSurprised = true;
                                if (script.state != State.attack) script.TurnToAttack(true);
                            }
                        }
                    }
                }
            }
        }
    }

    // check if current target changed tag to something not hostile
    bool CheckTargetTagChanged()
    {
        if (System.Array.IndexOf(vision.hostileTags, enemyToAttack.transform.tag) < 0) {
            return true;
        }

        return false;
    }

    // this will trigger when agent is in attack position
    void AttackPosition()
    {
        if (shouldAttack) {
            idleAttackTimer = 0f;
            if (!startAttackTimer) Attack();
        } else {
            if (!idleAttack) idleAttackTimer = 0f;
            idleAttack = true;
            attackBackUp = false;
            waitFrameRan = false;
            IdleAttackState();
            wpIdleTriggered = false;
        }
    }

    // prepare to attack
    void AttackPreparations()
    {   
        StopAllCoroutines();

        normalStateActive = false;
        alertStateActive = false;

        distracted = false;
        reachedEnd = false;
        waitFrameRan = false;
        distractionTurn = false;

        startWaypointRotation = false;
        waypointRotationAnimationTimer = 0f;

        if (attackState.surprised.useSurprised) Surprised();
        else TurnToAttack();
    }
    
    // turn agent to attack state
    public void TurnToAttack (bool calledFromAnother = false)
    {   
        attackState.surprised.startSurprisedTimerState = false;
        attackState.surprised.startSurprisedTimer = 0f;
       
        StopSystemsAudios("attack");
        
        alertStateActive = false;
        normalStateActive = false;
        isSeenVisionAlertTags = false;
        reachedEnd = false;
        
        // called from another agent
        if (calledFromAnother && state != State.attack) {
            if (alertedByOther) return;
            alertedByOther = true;
            
            StopAllCoroutines();
            StartCoroutine(AlertWaitToAttack());
            SetAttackChance();
            
            return;
        }

        state = State.attack;
        alertState.DisableScripts();
        normalState.DisableScripts();

        attackStateActive = true;
        SetAttackChance();
    }

    // make this agent in an idle-attack state waiting for his turn to attack
    void IdleAttackState()
    {   
        if (attackState.turnToTarget) {
            Vector3 toOther = (new Vector3(enemyPosition.x, transform.position.y, enemyPosition.z) - transform.position).normalized;
            float dotProd = Vector3.Dot(toOther, transform.forward);

            if (turningToTargetInAttack) {
                if (dotProd >= 0.97f) turningToTargetInAttack = false;
                RotateToTarget(enemyPosition, attackState.turnSpeed);
            }else{
                if (dotProd <= Mathf.Clamp(attackState.turnSensitivity, -1f, 0.97f)) {
                    Vector3 heading = new Vector3(enemyPosition.x, transform.position.y, enemyPosition.z) - transform.position;
                    float dirNum = distractions.AngleDir(transform.forward, heading, transform.up);
                    
                    if (attackState.useTurnAnimations) {
                        // turn right
                        if (dirNum == 1) animationManager.PlayAnimationState(waypoints.rightTurnAnimAlert, waypoints.turningAnimT);

                        // turn left
                        if (dirNum == -1) animationManager.PlayAnimationState(waypoints.leftTurnAnimAlert, waypoints.turningAnimT);
                    }else{
                        animationManager.PlayAnimationState(attackState.idleAnimationName, attackState.idleAnimationTransition, attackState.useAnimations);
                    }

                    turningToTargetInAttack = true;
                }else{
                    if (!strafing) animationManager.PlayAnimationState(attackState.idleAnimationName, attackState.idleAnimationTransition);
                    TriggerStrafe();
                    idleAttackTimer += Time.deltaTime;
                }
            }
        }else{
            if (!strafing) animationManager.PlayAnimationState(attackState.idleAnimationName, attackState.idleAnimationTransition);
            TriggerStrafe();
            idleAttackTimer += Time.deltaTime;
        }
        
        backedUpBy = null;
    }
    
    // from attack state return to patrolling in alert state
    void FromAttackStateReturnAlert()
    {
        state = State.alert;
        justEndedAttackState = true;
        attackStateActive = false;
        enemyToAttack = null;
        waitFrameRan = false;
        backedUpBy = null;
        agentObstacle.carving = true;
        lastCheckedEnemyPosition = Vector3.zero;
        captureEnemyTimeStamp = 0f;
        
        StopAllCoroutines();
        attackBackUp = false;
        ResetAttackingTimer();
        
        float temp = alertState.waitTime;
        alertState.waitTime = attackState.timeToReturnAlert;
        SetAgentObstacle();
        
        StartCoroutine(AlertStateIdle());
        alertState.waitTime = temp;
    }

    // quit attack state
    void EndAttackState()
    {
        forceEndAttackState = true;
        ResetStrafing();
        wpIdleTriggered = false;
        attackBackUp = false;
        waitFrameRan = false;
        pathCorner = Vector3.zero;
        
        reachedEndEnemyPosition = checkEnemyPosition;
        checkEnemyPosition = Vector3.zero;
        lastCheckedEnemyPosition = Vector3.zero;

        startAttackTimer = false;
        coverLocationSet = false;
        startIntervalTimer = false;
        intervalTimer = 0f;

        ResetAttackingTimer();
        FromAttackStateReturnAlert();
    }

    // trigger the surprised emotion
    void Surprised()
    {
        // if not surprised before
        if (!attackState.surprised.isSurprised && state == State.normal) {
            attackState.surprised.isSurprised = true;
            animationManager.PlayAnimationState(attackState.surprised.surprisedAnimationName, attackState.surprised.surprisedAnimationTransition, attackState.surprised.useAnimations);
            StopSystemsAudios("attack");
            attackState.PlaySurprisedAudio();
            attackState.surprised.startSurprisedTimerState = true;
            CarveAccordingToEnemy();
        }else{
            if (!attackState.surprised.startSurprisedTimerState) TurnToAttack();
        }
    }

    // if this agent got alerted by another, turn to alert for a brief moment
    // then resume to attack mode moving to hostile location
    IEnumerator AlertWaitToAttack()
    {
        normalStateActive = false;
        alertStateActive = false;
        distracted = false;
        idleAttack = false;

        animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
        yield return new WaitForSeconds(0.4f);

        state = State.attack;
        attackStateActive = true;

        alertedByOther = false;
    }

    // backup movement
    void BackupMovement()
    {
        if (isAgent || shouldAttack) return;

        Vector3 targetPosition = transform.position - (transform.forward * (proxyOffset * 2));
        Vector3 backupPoint = ValidateEnemyYPoint(targetPosition);
        RaycastHit hit;

        // set the layers
        int layers;
        if (attackState.coverShooterOptions.coverShooter) {
            layers = vision.layersToDetect | obstacleLayers | attackState.coverShooterOptions.coverLayers;
        }else{
            layers = vision.layersToDetect | obstacleLayers;
        }

        // check if obstacle is behind and if point is on navmesh
        if (Physics.Raycast(transform.position + centerPosition, transform.TransformDirection(-Vector3.forward), out hit, controller.radius * 2 + 0.5f, layers))
        {
            backupPoint = Vector3.zero;
        }else{
            if (!IsPointOnNavMesh(backupPoint, 0.3f)) backupPoint = Vector3.zero;
            else {
                if (!IsPathReachable(backupPoint)) backupPoint = Vector3.zero;
            }
        }

        if (backupPoint == Vector3.zero) {
            attackBackUp = false;
            AttackPosition();
        }else{
            ResetStrafing();
            currentStrafeDirBlocked = false;
            attackBackUp = true;
            RotateToTarget(enemyToAttack.transform.position, attackState.rotationSpeed);
            animationManager.PlayAnimationState(attackState.moveBackwardsAnimationName, attackState.moveBackwardsAnimationTransition, attackState.useAnimations);
            FixProxy("backup");
            MoveToDestination(backupPoint);
        }

        wpIdleTriggered = false;
    }
    
    // let this agent schedule itself to the list of enemies inside the enemy
    void ScheduleToEnemy()
    {
        if (enemyToAttack == enemyScheduled) return;

        BlazeAIEnemyManager script = enemyToAttack.GetComponent<BlazeAIEnemyManager>();

        if (script == null) {
            enemyToAttack.AddComponent<BlazeAIEnemyManager>();
            script = enemyToAttack.GetComponent<BlazeAIEnemyManager>();
        }

        if (!attackState.attackInIntervals) {
            if (!script.enemiesScheduled.Contains(this) && (enemyToAttack == script.transform.gameObject)) {
                script.enemiesScheduled.Add(this);
            }
        }

        targetManager = script;
        enemyScheduled = enemyToAttack;
    }
    
    // gets called by the enemy manager to let this agent go for an attack
    public void GoForAttack()
    {
        if (!targetManager.callEnemies) return;

        waitFrameRan = false;
        shouldAttack = true;

        if (!attackState.moveBackwardsAttack) attackBackUp = false;
        
        idleAttack = false;
    }

    // the attack function of interval attacks
    void IntervalAttack()
    {
        if (!targetManager.callEnemies) return;

        shouldAttack = true;
        startIntervalTimer = false;
        intervalTimer = 0f;
        attackBackUp = false;
        waitFrameRan = false;
        wpIdleTriggered = false;
        coverLocationSet = false;
    }

    // agent has reached attack position
    void Attack()
    {   
        ResetStrafing();
        attackDelayTimer = 0f;
        tookCover = false;

        if (navmeshAgent.enabled && !agentObstacle.enabled) navmeshAgent.Stop();
        
        if (!attackState.coverShooterOptions.coverShooter) {
            int layers = vision.hostileAndAlertLayers | attackState.layersCheckBeforeAttacking;
            RaycastHit hit;
            Vector3 startDir = transform.position + centerPosition;
            Vector3 dir = enemyColPoint - startDir;

            if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, layers)) {
                if (!enemyToAttack.transform.IsChildOf(hit.transform)) {
                    IdleAttackState();
                    return;
                }
            }
        }
        
        if (enemyPositionOnAttack == Vector3.zero) enemyPositionOnAttack = enemyToAttack.transform.position;
        if (attackState.attackScript != null) attackState.attackScript.enabled = true;
        attackState.PlayAttackAudio();

        startAttackTimer = true;
        attackState.SetRandomAttackAnimation();
        animationManager.PlayAnimationState(attackState.currentAttackAnimation, attackState.attackAnimationTransition);
    }

    // stop attack and return to attack idle state
    void StopAttack()
    {
        idleAttack = true;
        shouldAttack = false;
        startAttackTimer = false;
        waitFrameRan = false;
        wpIdleTriggered = false;
        attackTimer = 0f;
        
        enemyPositionOnAttack = Vector3.zero;
        if (attackState.attackScript != null) attackState.attackScript.enabled = false;

        // if attacking is set to intervals and interval randomization is enabled
        attackState.RandomizeAttackIntervals();
        if (attackState.attackInIntervals) startIntervalTimer = true;
    }

    // remove this agent from the scheduler of the targeted enemy
    void RemoveFromEnemyScehduler()
    {
        if (enemyToAttack == null || attackState.attackInIntervals) return; 
        BlazeAIEnemyManager script = enemyToAttack.GetComponent<BlazeAIEnemyManager>();

        if (script == null) return;
        script.RemoveEnemy(this);

        enemyScheduled = null;
    }

    // reset the timer that lets the ranged agent fire repeatedly
    void ResetAttackingTimer()
    {
        startIntervalTimer = false;
        intervalTimer = 0f;
    }

    #endregion

    #region Strafing
    
    // trigger strafe in idle attack state
    void TriggerStrafe()
    {
        if (attackState.coverShooterOptions.coverShooter) return;
        if (!abortStrafe) {
            if (attackState.strafe && (!strafeWait && !strafing && !strafeRan)) {
                strafeWaitTime = Random.Range(attackState.strafeWaitTime.x, attackState.strafeWaitTime.y);
                strafeWait = true;
            }
        }
    }

    // trigger strafing
    void Strafe(string directionArg="automatic")
    {
        if (attackBackUp || turningToTargetInAttack || strafeRan) return;

        strafeRan = true;
        strafeWait = false;
        strafeWaitTimer = 0f;

        string direction = "";

        if (!strafing) {
            if (directionArg == "automatic") {
                // get strafing direction
                if (attackState.strafeDirection == AttackState.Strafing.leftAndRight) {
                    int rand = Random.Range(0, 2);
                    if (rand == 0) direction = "left";
                    else direction = "right";
                }else{
                    if (attackState.strafeDirection == AttackState.Strafing.left) direction = "left";
                    else direction = "right";
                }
            }else{
                // overriding direction with argument
                direction = directionArg;
            }

            // if registered that the previous direction is blocked -> change to opposite direction
            if (currentStrafeDirBlocked && attackState.strafeDirection == AttackState.Strafing.leftAndRight) {
                if (currentStrafingDirection == "left") direction = "right";
                else direction = "left";
            
                currentStrafeDirBlocked = false;
            }

            // fix the proxy
            if (direction == "left") FixProxy("leftStrafe");
            else FixProxy("rightStrafe");

            // set the strafe time
            if (attackState.strafeTime.x == -1 && attackState.strafeTime.y == -1) {
                strafeTime = Mathf.Infinity;
            }else{
                if (attackState.strafeTime.x < 0) attackState.strafeTime.x = 0;
                if (attackState.strafeTime.y < 0) attackState.strafeTime.y = 0;

                strafeTime = Random.Range(attackState.strafeTime.x, attackState.strafeTime.y);
            }
        }

        if (direction.Length > 0) StrafeMovement(direction);
        else StrafeMovement(currentStrafingDirection);
    }

    // strafe movement
    void StrafeMovement(string direction)
    {
        if (attackBackUp || turningToTargetInAttack) return;
        
        Vector3 targetPosition;
        RaycastHit hit;
        Vector3 strafePoint;
        int layersToHit = vision.hostileAndAlertLayers | vision.layersToDetect | obstacleLayers | layersToAvoid | attackState.coverShooterOptions.coverLayers;

        if (direction == "left") {
            var offsetPlayer = transform.position - enemyColPoint;
            var dir = Vector3.Cross(offsetPlayer, Vector3.up);
            strafePoint = ValidateEnemyYPoint(dir);

            // if on navmesh
            if (IsPointOnNavMesh(pathFindingProxy.transform.position, 0.3f)) {
                // if hit something set the point to zero
                if (Physics.Raycast(transform.position + centerPosition, -strafePoint, out hit, 1.5f, layersToHit))
                {   
                    if (!enemyToAttack.transform.IsChildOf(hit.transform)) strafePoint = Vector3.zero;
                }

                // check before strafing to position if enemy will not be visible
                if (strafePoint != Vector3.zero) {
                    var offset = transform.TransformPoint(new Vector3(-1f, 0f, 0f) + centerPosition);
                    if (Physics.Raycast(offset, enemyColPoint - offset, out hit, Mathf.Infinity, layersToHit))
                    {   
                        if (!enemyToAttack.transform.IsChildOf(hit.transform)) strafePoint = Vector3.zero;
                    }
                }
            }else{
                strafePoint = Vector3.zero;
            }
        }else{
            var offsetPlayer = enemyToAttack.transform.position - transform.position;
            var dir = Vector3.Cross(offsetPlayer, Vector3.up);
            strafePoint = ValidateEnemyYPoint(dir);

            // if has navmesh
            if (IsPointOnNavMesh(pathFindingProxy.transform.position, 0.3f)) {
                // for raycasting to the right
                var rayDir = transform.position - enemyColPoint;
                var rayDirCross = Vector3.Cross(offsetPlayer, Vector3.up);

                // if hit something set the point to zero
                if (Physics.Raycast(transform.position + centerPosition, -rayDirCross, out hit, 1.5f, layersToHit))
                {
                    if (!enemyToAttack.transform.IsChildOf(hit.transform)) strafePoint = Vector3.zero;
                }

                // check before strafing to position if enemy will not be visible
                if (strafePoint != Vector3.zero) {
                    var offset = transform.TransformPoint(new Vector3(1f, 0f, 0f) + centerPosition);
                    if (Physics.Raycast(offset, enemyColPoint - offset, out hit, Mathf.Infinity, layersToHit))
                    {   
                        if (!enemyToAttack.transform.IsChildOf(hit.transform)) strafePoint = Vector3.zero;
                    }
                }
            }else{
                strafePoint = Vector3.zero;
            }
        }

        if (strafePoint == Vector3.zero) {
            // can't strafe -> change direction if possible
            ResetStrafing();
            reachedEnd = true;
            currentStrafeDirBlocked = true;
        }else{
            // strafe move
            if (direction == "left") animationManager.PlayAnimationState(attackState.leftStrafeAnimName, attackState.strafeT);
            else animationManager.PlayAnimationState(attackState.rightStrafeAnimName, attackState.strafeT);

            currentStrafingDirection = direction;
            currentStrafeDirBlocked = false;
            endPoint = transform.position + strafePoint;
            
            if (!strafing) SetupPath(endPoint);
            strafing = true;
            
            RotateToTarget(enemyToAttack.transform.position, 10f);
            MoveToDestination(endPoint);
        }

        strafeRan = false;
    }
    
    // reset strafing flags
    void ResetStrafing()
    {
        strafing = false;
        strafeTimer = 0f;
        strafeWait = false;
        strafeWaitTimer = 0f;
        strafeRan = false;
    }

    #endregion

    #region Cover Shooter
    
    // moving to cover
    void MoveToCover()
    {   
        attackBackUp = false;
        canAttackEnemyUnreachable = false;
        if (startCoverTimer || shouldAttack) return;
        
        Vector3 startDir = transform.position + centerPosition;
        Vector3 dir = enemyPosition - startDir;
        RaycastHit hit;

        int layers = vision.hostileAndAlertLayers | attackState.coverShooterOptions.coverLayers;
        if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, layers)) {
            if (hit.transform == currentCover.transform) {
                coverNormal = hit.normal * 5f;
                coverNormal.z = 0f;
                
                if (!attackState.coverShooterOptions.moveToCoverCenter) {
                    timerToTakeCover += Time.deltaTime;
                    if (timerToTakeCover >= 0.2f) {
                        TakeCover();
                        return;
                    }
                }
            }
        }

        MoveToCoverMovement();
    }

    void MoveToCoverMovement()
    {
        idleAttack = false;
        animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);
        MoveToDestination(endPoint);
        startIntervalTimer = true;
    }

    // in cover method
    void TakeCover()
    {   
        StopAttack();
        
        timerToTakeCover = 0f;
        startIntervalTimer = false;
        intervalTimer = 0f;
        coverTimer = 0f;

        idleAttack = true;
        findCoverFired = false;
        string animToPlay = "";
        
        // high cover
        if (coverHeight >= attackState.coverShooterOptions.coverAnimations.highCoverHeight) {
            animToPlay = attackState.coverShooterOptions.coverAnimations.highCoverAnimation;
            attackState.coverShooterOptions.coverAnimations.EnableCoverScript("high"); 
        }
        
        // low cover
        if (coverHeight <= attackState.coverShooterOptions.coverAnimations.lowCoverHeight) {
            animToPlay = attackState.coverShooterOptions.coverAnimations.lowCoverAnimation;
            attackState.coverShooterOptions.coverAnimations.EnableCoverScript("low");
        }
        
        animationManager.PlayAnimationState(animToPlay, attackState.coverShooterOptions.coverAnimationTransition);
        
        if (attackState.randomizeAttackIntervals) actualCoverTime = Random.Range(attackState.randomizeAttackIntervalsBetween.x, attackState.randomizeAttackIntervalsBetween.y);
        else actualCoverTime = attackState.attackInIntervalsTime;

        tookCover = true;
        startCoverTimer = true;
    }

    // trigger attack when in cover
    void FromCoverAttack()
    {
        // hit target cover or not
        if (attackState.coverShooterOptions.attackEnemyCover == CoverShooterOptions.AttackEnemyCover.AlwaysAttackCover) {
            getEnemyCover = true;
        } else if (attackState.coverShooterOptions.attackEnemyCover == CoverShooterOptions.AttackEnemyCover.Randomize) {
            int random = Random.Range(1,3);
            if (random % 2 == 0) getEnemyCover = true;
            else getEnemyCover = false;
        } else {
            getEnemyCover = false;
        }

        coverLocationSet = false;
        startCoverTimer = false;
        coverTimer = 0f;
        shouldAttack = true;
        startIntervalTimer = false;
        intervalTimer = 0f;
    }
    
    // move to enemy from cover
    void MoveFromCover(bool distanceClosed = false)
    {
        if (startAttackTimer) return;
        timerToTakeCover = 0f;
        
        if (IsPathReachable(enemyPosition)) {
            canAttackEnemyUnreachable = false;
            idleAttack = false;
            animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);
            MoveToDestination(enemyPosition);
        }else{
            if (distanceClosed) {
                canAttackEnemyUnreachable = true;
                idleAttack = false;
                animationManager.PlayAnimationState(attackState.moveForwardAnimationName, attackState.moveForwardAnimationTransition, attackState.useAnimations);
                MoveToDestination(enemyPosition);
            }else{
                canAttackEnemyUnreachable = false;
            }
        }
    }
    
    // search for cover
    public void FindCover(bool overrun = false, Transform cover = null)
    {   
        tookCover = false;
        
        if (overrun) {
            if (findCoverFired || !enemyToAttack || shouldAttack) return;
        }else{
            if (coverLocationSet || findCoverFired || !enemyToAttack || shouldAttack || startIntervalTimer) return;
        }

        if (!overrun) {
            if (hideFrames >= 5) hideFrames = 0;
            else { 
                hideFrames++;
                return;
            }
        }else{
            attackStateActive = false;
            startCoverTimer = false;
            coverLocationSet = false;
        }

        findCoverFired = true;

        coverColls = new Collider[10];
        int hits = Physics.OverlapSphereNonAlloc(transform.position, attackState.coverShooterOptions.searchDistance, coverColls, attackState.coverShooterOptions.coverLayers, queryTriggerInteraction: QueryTriggerInteraction.Collide);
        int hitReduction = 0;
        if (hits > 0) agentObstacle.carving = false;

        // eliminate bad cover options
        for (int i=0; i<hits; i++) {

            if (Vector3.Distance(coverColls[i].transform.position, enemyPosition) >= vision.visionDuringAttackState.sightRange || coverColls[i].bounds.size.y < attackState.coverShooterOptions.minObstacleHeight) {
                coverColls[i] = null;
                hitReduction++;
            }else{
                Collider[] col = new Collider[10];
                Collider[] col2 = new Collider[10];

                int hits2 = Physics.OverlapSphereNonAlloc(new Vector3(coverColls[i].transform.position.x, transform.position.y, coverColls[i].transform.position.z), (coverColls[i].bounds.size.x + coverColls[i].bounds.size.z), col, attackState.agentLayersToCall);
                int hits3 = Physics.OverlapSphereNonAlloc(transform.position, attackState.coverShooterOptions.searchDistance, col2, attackState.agentLayersToCall);

                // check if cover has agent occupying
                for (int x=0; x<hits2; x++) {
                    BlazeAI script = col[x].GetComponent<BlazeAI>();
                    if (script != null) {
                        if (cover != null) {
                            if (script.currentCover != null && coverColls[i] != null) {
                                if (coverColls[i].transform == cover || script.currentCover == cover) {
                                    coverColls[i] = null;
                                    hitReduction++;
                                    continue;
                                }
                            }
                        }else{
                            if (script != this) {
                                if (script.currentCover != null && coverColls[i] != null) {
                                    if (script.currentCover == coverColls[i].transform) {
                                        coverColls[i] = null;
                                        hitReduction++;
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                // check if nearby agent going to cover
                for (int y=0; y<hits3; y++) {
                    BlazeAI npc = col2[y].GetComponent<BlazeAI>();
                    if (npc != null) {
                        if (npc != this) {
                            if (npc.currentCover != null && coverColls[i] != null) {
                                if (cover != null) {
                                    if (npc.currentCover == cover) {
                                        coverColls[i] = null;
                                        hitReduction++;
                                        break;
                                    }
                                }else{
                                    if (npc.currentCover == coverColls[i].transform) {
                                        coverColls[i] = null;
                                        hitReduction++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        hits -= hitReduction;
        System.Array.Sort(coverColls, ColliderArraySortComparer);

        // if no obstacles found
        if (hits <= 0) {
            NoCoversFound();
            return;
        }

        NavMeshHit hit = new NavMeshHit();
        NavMeshHit hit2 = new NavMeshHit();
        NavMeshHit closestEdge = new NavMeshHit();
        NavMeshHit closestEdge2 = new NavMeshHit();
        
        // if found obstacles
        for (int i = 0; i < hits; i++)
        {
            Vector3 boundSize = coverColls[i].GetComponent<Collider>().bounds.size;
            if (NavMesh.SamplePosition(coverColls[i].transform.position, out hit, boundSize.x + boundSize.z, navmeshAgent.areaMask))
            {
                if (!NavMesh.FindClosestEdge(hit.position, out closestEdge, NavMesh.AllAreas)) {
                    currentCover = null;
                }

                if (Vector3.Dot(closestEdge.normal, (enemyToAttack.transform.position - closestEdge.position).normalized) < attackState.coverShooterOptions.hideSensitivity)
                {
                    if (!IsPathReachable(closestEdge.position)) {
                        currentCover = null;
                        continue;
                    }

                    ChooseCover(closestEdge, coverColls[i]);
                    break;
                }
                else {
                    // Since the previous spot wasn't facing "away" enough from the target, we'll try on the other side of the object
                    if (NavMesh.SamplePosition(coverColls[i].transform.position - (enemyToAttack.transform.position - hit.position).normalized * 2, out hit2, boundSize.x + boundSize.z, navmeshAgent.areaMask))
                    {
                        if (!NavMesh.FindClosestEdge(hit2.position, out closestEdge2, NavMesh.AllAreas)) {
                            currentCover = null;
                        }

                        if (Vector3.Dot(closestEdge2.normal, (enemyToAttack.transform.position - closestEdge2.position).normalized) < attackState.coverShooterOptions.hideSensitivity)
                        {
                            if (!IsPathReachable(closestEdge2.position)) {
                                currentCover = null;
                                continue;
                            }

                            ChooseCover(closestEdge2, coverColls[i]);
                            break;
                        }
                    }
                }
            }else{
                currentCover = null;
            }
        }

        // no cover found
        if (!currentCover) {
            NoCoversFound();
            return;
        }else{
            if (CoverOccupied(currentCover)) coverLocationSet = false;
        }

        attackStateActive = true;
        findCoverFired = false;
    }

    void ChooseCover(NavMeshHit hit, Collider cover)
    {
        currentCover = cover.transform;
        endPoint = hit.position;
        coverHeight = cover.bounds.size.y;
        currentCoverColl = currentCover.GetComponent<Collider>();
        noCoversFound = false;
        coverLocationSet = true;
    }

    // check if cover is occupied by another agent
    bool CoverOccupied(Transform cover)
    {
        if (cover == null) return true;

        if (checkCoverOccFrames >= 10) {
            checkCoverOccFrames = 0;
            int checkCoverOccupiedInt = Physics.OverlapSphereNonAlloc(new Vector3(cover.position.x, transform.position.y, cover.position.z), currentCoverColl.bounds.size.x, checkCoverOccupiedColl, attackState.agentLayersToCall);

            for (int i=0; i<checkCoverOccupiedInt; i++) {
                BlazeAI script = checkCoverOccupiedColl[i].GetComponent<BlazeAI>();
                if (script != null) {
                    if (script != this) {
                        if (script.currentCover != null) {
                            if (script.currentCover == cover) {
                                float thisDistance = (cover.position - transform.position).sqrMagnitude;
                                float otherDistance = (cover.position - script.transform.position).sqrMagnitude;
                                
                                // if this agent is more far from cover
                                if (thisDistance > otherDistance) {
                                    findCoverFired = false;
                                    return true;
                                }else{
                                    if (script.startCoverTimer) {
                                        findCoverFired = false;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }else{
            checkCoverOccFrames++;
        }
    
        return false;
    }

    int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }

    // set the attack chance if cover shooter
    void SetAttackChance()
    {
        if (attackState.coverShooterOptions.coverShooter) {
            
            // set first sight attack chance
            if (attackState.coverShooterOptions.firstSightChance != CoverShooterOptions.FirstSightChance.TakeCover) {
                if (attackState.coverShooterOptions.firstSightChance == CoverShooterOptions.FirstSightChance.Randomize) {
                    int luck = Random.Range(1, 3);
                    if (luck % 2 == 0) shouldAttack = false;
                    else shouldAttack = true;
                }else{
                    shouldAttack = true;
                }
            }else{
                shouldAttack = false;
            }
            
            // set cover attack state
            if (attackState.coverShooterOptions.attackEnemyCover == CoverShooterOptions.AttackEnemyCover.AlwaysAttackCover) {
                getEnemyCover = true;
            } else if (attackState.coverShooterOptions.attackEnemyCover == CoverShooterOptions.AttackEnemyCover.Randomize) {
                int random = Random.Range(1,3);
                if (random % 2 == 0) getEnemyCover = true;
                else getEnemyCover = false;
            } else {
                getEnemyCover = false;
            }
            
        }
    }

    // temporarily disable cover shooter mode until covers are found to avoid errors
    void NoCoversFound() 
    {
        noCoversFound = true;
        coverLocationSet = false;
        findCoverFired = false;
        attackStateActive = true;
        currentCover = null;
    }

    #endregion

    #region Movement

    // trigger the state walk
    public void StateWalk(bool turnReachedEndTrue = false)
    {
        if (!Application.isPlaying) return;
        ResetAllFlags();

        // if random mode chosen
        if (waypoints.randomize) {
            if (!wpRandomMode) {
                wpRandomMode = true;
                if (waypointInterrupted) {
                    endPoint = endPointBeforeDistraction;
                    waypointInterrupted = false;
                    CallPathSetup();
                }else{
                    RandomNavmeshLocation();
                    waitFrameRan = false;
                    CallPathSetup();
                }
            }
        }else{
            if ((reachedEnd || turnReachedEndTrue) && !goingToDistractionPoint) {
                if (!waypointInterrupted) {
                    if (waypointIndex >= (waypoints.waypoints.Length - 1)) {
                        if (waypoints.loop) waypointIndex = 0;

                        if (endPoint == waypoints.waypoints[waypointIndex] && waypoints.waypointsRotation[waypointIndex] != Vector2.zero) {
                            Vector3 toOther = (new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, transform.position.y, transform.position.z + waypoints.waypointsRotation[waypointIndex].y) - transform.position).normalized;
                            float dotProd = Vector3.Dot(toOther, transform.forward);
                            if (dotProd <= 0.95f) startWaypointRotation = true;
                        }
                    }else{
                        waypointIndex++;
                    }
                }else{
                    waypointInterrupted = false;
                }
            }

            waitFrameRan = false;
            if (endPoint != waypoints.waypoints[waypointIndex]) {
                endPoint = waypoints.waypoints[waypointIndex];
                wpRandomMode = false;
                CallPathSetup();
            }else{
                CallPathSetup();
            }
        }

        // trigger the scripts and the animations
        if (state == State.normal) {
            normalState.TriggerScripts("walking");
            normalStateActive = true;
        }else{
            alertState.TriggerScripts("walking");
            alertStateActive = true;
        }
    }

    // move to passed destination
    void MoveToDestination(Vector3 pos)
    {
        float reachDistance = 0f;

        if (goingToVisionAlertTag) reachDistance = 2f;
        else {
            if (isAgent || ranEnableAgent) reachDistance = (navmeshAgent.radius * 2);
            else reachDistance = controller.radius * 2;
        }

        distractionTurn = false;
        pathFramesElapsed += Time.deltaTime;
        endPoint = pos;

        if ((new Vector3(pos.x, transform.position.y, pos.z) - transform.position).sqrMagnitude <= reachDistance * reachDistance) {
            reachedEnd = true;
            waitFrameRan = false;
    
            reachedEndEnemyPosition = checkEnemyPosition;
            if (!enemyToAttack) checkEnemyPosition = Vector3.zero;
            pathFramesElapsed = 0f;
            hasPath = false;

            if (coverLocationSet) {
                TakeCover();
                return;
            }

            // if the location the agent is going to is the distraction point, then play audio
            if (goingToDistractionPoint) {
                StopSystemsAudios("distraction");
                distractions.TriggerDistractionSearchAudio();
            }
        }else{
            reachedEnd = false;

            // recalculate path every n seconds
            if (pathFramesElapsed >= pathRecalculationRate) {
                pathFramesElapsed = 0f;

                if (isAgent && !ranDisableAgent) navmeshAgent.SetDestination(pos);
                else SetupPath(pos);
            }

            // move the agent
            if (!isAgent) MoveToPoint(reachDistance);
        }
        
        // trigger a raycast when distance is at the ray distance value 
        // to avoid facing an obstacle too closely
        if (state == State.normal || state == State.alert) {
            if (avoidFacingObstacles) {
                pos = new Vector3(pos.x, transform.position.y, pos.z);
                if ((pos - pathFindingProxy.position).sqrMagnitude <= obstacleRayDistance * obstacleRayDistance) {
                    if (Vector3.Dot((pos - transform.position).normalized, transform.forward) >= 0.8f) activateRay = true;
                    else activateRay = false;
                }
            }
        }
    }

    // smooth rotate agent to target
    void RotateToTarget(Vector3 location, float speed)
    {
        Quaternion lookRotation = Quaternion.LookRotation((location - transform.position).normalized);
        lookRotation = new Quaternion(0f, lookRotation.y, 0f, lookRotation.w);
        transform.rotation = Quaternion.Slerp(new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w), lookRotation, speed * Time.deltaTime);
    }

    // setup up the navmesh path to the destination
    void SetupPath(Vector3 destination)
    {
        if (state != State.attack) {
            // sets path corners and checks for reachability
            if (!IsPathReachable(destination)) {
                if (!IsPointOnNavMesh(ValidateEnemyYPoint(pathFindingProxy.position), 0.3f)) {
                    forceTurn = true;
                    StopAllCoroutines();
                }else{
                    StopAgent();
                    StateWalk();
                }
                return;
            }
        }
        else {
            if (!isAgent) NavMesh.CalculatePath(ValidateEnemyYPoint(pathFindingProxy.position), ValidateEnemyYPoint(destination), NavMesh.AllAreas, path);
        }

        if (!isAgent) {
            cornerQueue = new Queue<Vector3>(PathSmoothing(path.corners));
            GetNextCorner();
        }
    }
    
    // smooth the path
    Vector3[] PathSmoothing(Vector3[] corners)
    {
        // use path smoothing only if not strafing, going to cover, attacking or backing up
        if (!shouldAttack && !attackBackUp && !coverLocationSet && !strafing) {    
            if (pathSmoothing) {
                // need atleast 3 corners
                if (corners.Length < 3) return corners;

                List<Vector3> cornersList = new List<Vector3>(corners);
                int max = corners.Length;

                for (int i=0; i<max; i++) {
                    if (i+1 < max && i-1 >= 0) {
                        var t1 = (cornersList[i] - cornersList[i-1]).normalized;
                        var t2 = (cornersList[i+1] - cornersList[i]).normalized;

                        // this is the average tangent
                        var avgTangent = Vector3.Lerp(t1, t2, 0.5f).normalized;
                        
                        Vector3 originalPoint = cornersList[i];
                        Vector3 newPoint1 = originalPoint - avgTangent * pathSmoothingFactor;
                        Vector3 newPoint2 = originalPoint + avgTangent * pathSmoothingFactor;

                        // check over smoothing (point not in same direction)
                        Vector3 dir = (originalPoint - cornersList[i-1]).normalized;
                        Vector3 target = (newPoint2 - newPoint1).normalized;
                        float dot = Vector3.Dot(target, dir);

                        float agentRadius;
                        if (isAgent) agentRadius = navmeshAgent.radius;
                        else agentRadius = controller.radius;
                        
                        // check if new corners are on navmesh and not over smoothed
                        if (IsPointOnNavMesh(newPoint1, 0.5f) && IsPointOnNavMesh(newPoint2, 0.5f) && dot >= 0.5f && Vector3.Distance(originalPoint, cornersList[i-1]) > agentRadius) {
                            cornersList[i] = newPoint1;
                            cornersList.Insert(i+1, newPoint2);
                        }
                    }
                }
                return cornersList.ToArray();
            }else{
                return corners;
            }
        }else{
            return corners;
        }
    }

    // get the next corner
    void GetNextCorner()
    {
        if (cornerQueue.Count > 0) {
            pathCorner = cornerQueue.Dequeue();
            hasPath = true;
        }else{
            hasPath = false;
        }
    }
    
    // move the controller
    void MoveToPoint(float reachDistance)
    {
        float currentDistance = (transform.position - new Vector3(pathCorner.x, transform.position.y, pathCorner.z)).sqrMagnitude;
        float minDistance = 0f;
        
        if (cornerQueue.Count > 0) minDistance = proxyOffset + 0.1f;
        else minDistance = reachDistance;

        if (currentDistance <= minDistance * minDistance) GetNextCorner();
        
        // set the speeds of movement and rotation and minimum distance
        float speed;
        float rotationSpeed;
        
        if (state == State.normal) {
            speed = normalState.moveSpeed;
            rotationSpeed = normalState.rotationSpeed;
        } else if (state == State.alert) {
            speed = alertState.moveSpeed;
            rotationSpeed = alertState.rotationSpeed;
        } else {
            if (attackBackUp) speed = attackState.moveBackwardsSpeed;
            else {
                if (strafing) speed = attackState.strafeSpeed;
                else speed = attackState.moveSpeed;
            }
            rotationSpeed = attackState.rotationSpeed;
        }
        
        if (!attackBackUp && !strafing) RotateToTarget(pathCorner, rotationSpeed);
        
        if (!useRootMotion) { 
            var direction = (new Vector3(pathFindingProxy.position.x, transform.position.y, pathFindingProxy.position.z) - transform.position);
            if (controller.enabled && !navmeshAgent.enabled) controller.Move(direction * Time.deltaTime * speed); 
        } else { 
            if (controller.enabled && shouldAttack && !navmeshAgent.enabled) controller.Move(rootMotionDirection * Time.deltaTime); 
        }
    }

    // apply a gravity force to keep controller grounded
    void KeepToGravity()
    {
        if (stop) return;
        if (isAgent) return;
        if (!controller.enabled) return;

        if (controller.isGrounded) verticalVelocity = 0f;
        else verticalVelocity -= Time.deltaTime * gravityStrength;

        controller.Move(new Vector3(0f, verticalVelocity, 0f));
    }

    // call the wait frame setup
    void CallPathSetup()
    {
        if (waitFrameRan) return;
        if (isAgent) return;
        if (attackState.coverShooterOptions.coverShooter && shouldAttack) return;
        
        StopAllCoroutines();
        StartCoroutine(WaitFrameSetup());
    }

    // obstacle disabling takes a frame to compeltely close
    // so wait and then set up the path
    IEnumerator WaitFrameSetup() 
    {   
        waitFrameRan = true;
        
        if (state != State.attack) {
            agentObstacle.carving = false;
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!goingToDistractionPoint) FixProxy();

        if (enemyToAttack) SetupPath(enemyPosition);
        else SetupPath(endPoint);

        reachedEnd = false;
        if (!enemyToAttack) agentObstacle.carving = true;
    }

    // set the proxy position
    public void FixProxy(string state="normal")
    {
        pathFindingProxy.transform.SetParent(transform);
        pathFindingProxy.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        
        if (state == "normal") {
            pathFindingProxy.transform.localPosition = new Vector3(0f, 0f, proxyOffset);
        } else if (state == "backup") {
            pathFindingProxy.transform.localPosition = new Vector3(0f, 0f, -proxyOffset);
        } else if (state == "leftStrafe") {
            pathFindingProxy.transform.localPosition = new Vector3(-proxyOffset, 0f, 0f);
        } else {
            pathFindingProxy.transform.localPosition = new Vector3(proxyOffset, 0f, 0f);
        }
    }

    // from state idle to respective state walk
    void FromIdleToWalk()
    {
        if (state == State.normal) {

            if (normalState.instantMoveChange) normalState.InstantMoveReturnsVals();
            distractions.DisableScript();

            StateWalk();
        }

        if (state == State.alert) {

            if (alertState.instantMoveChange) alertState.InstantMoveReturnsVals();
            
            if (goingToDistractionPoint) {
                goingToDistractionPoint = false;
                distractions.DisableScript();
                StateWalk();
            }else{
                StateWalk();
            }

            alertStateActive = true;
        }
    }

    // check if current waypoint stop has a rotation
    bool WaypointRotationCheck() 
    {
        if (goingToDistractionPoint || (state != State.normal && state != State.alert) || waypoints.randomize || isSeenVisionAlertTags || goingToVisionAlertTag) return false;
        if ( (waypointIndex+1) > waypoints.waypoints.Length || waypointIndex < 0) return false;

        if ((waypoints.waypointsRotation[waypointIndex].x != 0 || waypoints.waypointsRotation[waypointIndex].y != 0)) {
            float dotProd = Vector3.Dot((new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, transform.position.y, transform.position.z + waypoints.waypointsRotation[waypointIndex].y) - transform.position).normalized, transform.forward);
            if (dotProd < 0.97f) return true;
            else return false;
        }else{
            return false;
        }
    }

    // trigger the rotation
    void TriggerWaypointRotation() 
    {
        if (startWaypointRotation && waypointRotationAnimationTimer < waypoints.timeBeforeTurning) {
            reachedEnd = true;
            if (state == State.normal) animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
            if (state == State.alert) animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
        }          

        waypointTurnDir = (int)distractions.AngleDir(
                            transform.forward, 
                            new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, 0f, transform.position.z + waypoints.waypointsRotation[waypointIndex].y) - transform.position, 
                            transform.up);
        startWaypointRotation = true;
    }

    // smooth rotate to target
    void rotateToWaypoint(float[] degrees, float speed)
    {
        Quaternion lookRotation = Quaternion.LookRotation((new Vector3(transform.position.x + degrees[0], transform.position.y, transform.position.z + degrees[1]) - transform.position).normalized);
        lookRotation = new Quaternion(0f, lookRotation.y, 0f, lookRotation.w);
        transform.rotation = Quaternion.Slerp(new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w), lookRotation, speed * Time.deltaTime);
    }

    // enable agent
    void EnableAgent()
    {
        if (ranEnableAgent || isAgent || agentChange) return;
        agentChange = true;

        ranDisableAgent = false;
        ranDisableAgentTimer = 0f;
        agentObstacle.enabled = false;

        ranEnableAgent = true;
    }

    // disable agent
    void DisableAgent()
    {
        if (ranDisableAgent || !isAgent || agentChange) return;
        agentChange = true;

        ranEnableAgent = false;
        ranEnableAgentTimer = 0f;

        FixProxy();
        navmeshAgent.enabled = false;

        ranDisableAgent = true;
    }

    // method for stopping the agent completely
    public void StopAgent()
    {
        reachedEnd = true;
        navmeshAgent.enabled = false;
        isAgent = false;
    }

    void StopMovement()
    {
        normalStateActive = false;
        alertStateActive = false;
        attackStateActive = false;
        controller.Move(Vector3.zero);
    }
    
    // call agent to passed location
    public void CallAgentToLocation(Vector3 location, float time=0f, string animationName = null, string stateToTurn=null)
    {
        if (state == State.attack) return;

        // if agent is disabled save parameters to trigger on start
        if (!enabled || !gameObject.activeInHierarchy || !controller) {
            calledAgentToLocationWhileDisabled = true;
            CATLObject.location = location;
            CATLObject.time = time;
            CATLObject.animationName = animationName;
            CATLObject.stateToTurn = stateToTurn;
            return;
        }

        calledAgentToLocationWhileDisabled = false;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(ValidateEnemyYPoint(location), out hit, navmeshAgent.radius * 2, NavMesh.AllAreas)) {
            StopMovement();
            waypointInterrupted = true;

            if (stateToTurn == null) {
                if (state == State.normal) stateToTurn = "normal";
                else stateToTurn = "alert";
            }

            // play the provided anim
            if (animationName != null) animationManager.PlayAnimationState(animationName, 0.3f, true, true);
            else {
                // play the idle anim for states
                if (stateToTurn == "normal") animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition);
                else animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition);
            }
            
            endPoint = hit.position;
            if (stateToTurn == "attack") checkEnemyPosition = endPoint;

            waitFrameRan = false;
            CallPathSetup();
            StartCoroutine(CallAgentToLocationCoroutine(time, stateToTurn));
        }
    }

    // wait before moving to called location
    IEnumerator CallAgentToLocationCoroutine(float time, string stateToTurn)
    {
        yield return new WaitForSeconds(time);

        MoveToDestination(endPoint);
        if (waypoints.useMovementTurning) turningToCorner = true;

        if (stateToTurn == "normal") {
            normalStateActive = true;
            state = State.normal;
        } 
        else if (stateToTurn == "alert") {
            alertStateActive = true;
            state = State.alert;
        } 
        else {
            attackStateActive = true;
            state = State.attack;
        }
    }

    #endregion

    #region Distractions

    // responsible for distracting the enemy and calling stop/idle on all states
    public void Distract(Transform location, bool groupDistraction = false)
    {
        if (enabled && distractions.alwaysUse && !distracted && (state != State.attack) && !isHit && !attackState.surprised.startSurprisedTimerState && checkEnemyPosition == Vector3.zero) {
            StopAllCoroutines();
            StopSystemsAudios("distraction");

            StopMovement();

            activateRay = false;
            distracted = true;

            startWaypointRotation = false;
            waypointRotationAnimationTimer = 0f;

            // if set, on distraction turn to alert
            if (distractions.turnAlertOnDistraction && state != State.alert) {
                state = State.alert;
            }

            if (state == State.normal) {
                StartCoroutine(NormalStateIdle(true));
                normalState.StopCurrentAudio();
            }

            if (state == State.alert) {
                StartCoroutine(AlertStateIdle(true));
                alertState.StopCurrentAudio();
            }

            if (groupDistraction) passDistractionCheck = true;
            else passDistractionCheck = false;
            
            // validate and enable script
            distractions.EnableScript();
            
            distractionPosition = location.position;
            endPointBeforeDistraction = endPoint;

            if (distractions.autoTurn) StartCoroutine(WaitBeforeTurn(distractions.turnReactionTime, location.position, false));
            else StartCoroutine(WaitBeforeTurn(distractions.turnReactionTime, location.position, true));
        }
    }

    // coroutine for waiting before turning to distraction
    IEnumerator WaitBeforeTurn(float time, Vector3 location, bool skipAutoTurn = false)
    {
        yield return new WaitForSeconds(time);
        if (!skipAutoTurn) distractionTurn = true;
        
        // get distraction direction (left or right)
        // and play the corresponding animation
        if (distractions.useTurnAnimations) {
            float dotProd = Vector3.Dot((location - transform.position).normalized, transform.forward);
            // play animation only when distraction is NOT in front of agent
            if (dotProd < 0.98f) {
                float dirNum = distractions.AngleDir(transform.forward, (location - transform.position), transform.up);

                // dir to turn
                if (dirNum == 1) {
                    if (state == State.normal) animationManager.PlayAnimationState(waypoints.rightTurnAnimNormal, waypoints.turningAnimT);
                    else animationManager.PlayAnimationState(waypoints.rightTurnAnimAlert, waypoints.turningAnimT);
                }
                
                if (dirNum == -1) {
                    if (state == State.normal) animationManager.PlayAnimationState(waypoints.leftTurnAnimNormal, waypoints.turningAnimT);
                    else animationManager.PlayAnimationState(waypoints.leftTurnAnimAlert, waypoints.turningAnimT);
                }
            }
        }
        
        // if false - means he wasn't in a group so control flow should be normal
        if (!passDistractionCheck) {
            // normal control flow check whether agent is supposed to check location or not
            if (distractions.moveToDistractionLocation) {
                yield return StartCoroutine(WaitBeforeMovingToLocation(distractions.moveToDistractionReactTime, location));
            }else
                yield return StartCoroutine(TurnToDistractionNoMoving(distractions.moveToDistractionReactTime));
        }else{
            yield return StartCoroutine(TurnToDistractionNoMoving(distractions.moveToDistractionReactTime));
        }
    }

    // coroutine for waiting before moving to distraction location
    IEnumerator WaitBeforeMovingToLocation(float time, Vector3 location)
    {
        yield return new WaitForSeconds(time);

        StartCoroutine(distractions.EnableAudiosToBePlayedAgain());
        Vector3 pos = ValidateEnemyYPoint(location);
        waypointInterrupted = true;
        reachedEnd = true;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, navmeshAgent.radius * 2, NavMesh.AllAreas)) {
            distracted = false;
            endPoint = hit.position;
            waitFrameRan = false;
    
            goingToDistractionPoint = true;
            CallPathSetup();
            
            if (state == State.normal) { 
                normalState.TriggerScripts("walking");
                normalStateActive = true;
            }

            if (state == State.alert) {
                alertState.TriggerScripts("walking");
                alertStateActive = true;
            }
        }else{
            goingToDistractionPoint = false;
            StateWalk();
        }
    }

    // if auto turn is chosen but moving to location isn't
    IEnumerator TurnToDistractionNoMoving(float time)
    {
        yield return new WaitForSeconds(time);

        wpRandomMode = false;
        passDistractionCheck = false;
        waypointInterrupted = true;
        goingToDistractionPoint = false;

        if (state == State.normal) normalStateActive = true;
        if (state == State.alert) alertStateActive = true;

        distractions.DisableScript();
        StartCoroutine(distractions.EnableAudiosToBePlayedAgain());

        StateWalk(true);
        if (waypoints.useMovementTurning) turningToCorner = true;
    }

    #endregion

    #region Navmesh

    // get random point from navmesh
    void RandomNavmeshLocation() 
    {
        float walkRadius = 20f;
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        Vector3 point;

        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        point = hit.position + (-transform.forward * (proxyOffset+0.1f));

        float distance = (new Vector3(point.x, transform.position.y, point.y) - transform.position).sqrMagnitude;
        float radius = controller.radius + proxyOffset;

        if (distance <= radius * radius) {
            RandomNavmeshLocation();
            return;
        }

       endPoint = point;
    }

    // check whether point is on navmesh or not
    bool IsPointOnNavMesh(Vector3 point, float radius = 2f)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(point, out hit, radius, NavMesh.AllAreas)) return true;
        else return false;
    }

    // get random position within point
    Vector3 GetSamplePosition(Vector3 point, float range)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(point, out hit, range, NavMesh.AllAreas)) return hit.position;
        else return Vector3.zero;
    }

    // get the correct y position of an enemy
    Vector3 ValidateEnemyYPoint(Vector3 pos)
    {
        if (!IsPointOnNavMesh(pos, 0.3f)) {
            RaycastHit downHit;
            if (Physics.Raycast(pos, -Vector3.up, out downHit, Mathf.Infinity, groundLayers)) {
                return downHit.point;
            }else{
                return new Vector3(pos.x, transform.position.y, pos.z);
            }
        }else{
            return pos;
        }
    }

    // is path status complete
    bool IsPathReachable(Vector3 position) 
    {
        bool pathValidation = NavMesh.CalculatePath(ValidateEnemyYPoint(pathFindingProxy.position), ValidateEnemyYPoint(position), NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete) return true;
        else return false;
    }

    #endregion
    
    #region Agent Setup

    // restructure the gameobjects heirarchy for use by BlazeAI
    public void BuildNPC()
    {      
        //make a new container gameobject
        GameObject containerGO = new GameObject();
        containerGO.name = gameObject.name + "Container";
        containerGO.transform.localPosition = transform.position;

        //make this current object a child of the generated object
        transform.parent = containerGO.transform;

        GameObject proxyGO = new GameObject();
        proxyGO.name = "PathFindingProxy";

        proxyGO.transform.parent = containerGO.transform;
        proxyGO.transform.localPosition = new Vector3(0f, 0f, 0f);
        pathFindingProxy = proxyGO.transform;

        CharacterController buildController = GetComponent<CharacterController>();
        agentObstacle = GetComponent<NavMeshObstacle>();

        buildController.center = new Vector3 (0f, 0.9f, 0f);
        buildController.radius = 0.3f;
        buildController.height = 1.75f;
        buildController.stepOffset = 0.1f;
        buildController.skinWidth = 0.01f;

        navmeshAgent = GetComponent<NavMeshAgent>();
        navmeshAgent.enabled = false;
        
        AgentSpeeds();
        SetAgentObstacle(true);

        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.height = buildController.height;
        capsuleCollider.radius = buildController.radius;
        capsuleCollider.center = buildController.center;
        capsuleCollider.isTrigger = true;
        
        Debug.Log("Set the proxy offset property in the inspector to a good offset position. This positions the pathfinding proxy. Make sure the proxy is outside the navmesh obstacle carve.");
        Debug.Log("You can now edit the navmesh obstacle properties in it's new heirarchal structure.");
        Debug.Log("Make sure you set the character controller properties (height, center, radius) to whatever suits your needs.");
        Debug.Log("Don't change the name of the PathFindingProxy gameobject (for identification purposes)");
        Debug.Log("If you're going to use animations then don't forget to set the Animator Controller and avatar");
    }

    // check whether the build structure is correct
    public bool CheckNPCBuild()
    {
        if (transform.parent != null) {
            foreach (Transform item in transform.parent)
            {
                if (item.gameObject.name == "PathFindingProxy") {
                    return true;
                }
            }
            return false;
        }else{
            return false;
        }
    }

    // best obstacle settings
    // obstacle is imperative for agents avoiding each other
    void SetAgentObstacle(bool buildRun = false)
    {
        //OVERRIDE THE SETTINGS HERE IF NEEDED
        agentObstacle.carving = true;
        agentObstacle.carvingMoveThreshold = 0f;
        agentObstacle.carvingTimeToStationary = 0f;
        agentObstacle.carveOnlyStationary = false;
        
        if (buildRun) agentObstacle.size = new Vector3(0.5f, 1f, 0.1f);
    }

    // remove the carve from enemy if AI vs AI
    public void TurnCarveOff()
    {
        agentObstacle.carving = false;
    }

    // disable/enable carve on enemy
    void CarveAccordingToEnemy()
    {
        if (enemyToAttack) {
            if (enemyToAttack.transform == lastEnemy) {
                // if the same last enemy and is a BlazeAI agent
                if (lastEnemyScript) {
                    lastEnemyScript.TurnCarveOff();
                    TurnCarveOff();
                }else{
                    SetAgentObstacle();
                }
            }else{
                BlazeAI script = enemyToAttack.GetComponent<BlazeAI>();
                
                if (script != null) {
                    lastEnemyScript = script;
                    script.TurnCarveOff();
                    TurnCarveOff();
                }else{
                    SetAgentObstacle();
                }
                
                lastEnemy = enemyToAttack.transform;
            }
        }
    }

    // set the agent speeds
    void AgentSpeeds()
    {
        navmeshAgent.speed = attackState.moveSpeed;
        navmeshAgent.angularSpeed = 2000f;
        navmeshAgent.acceleration = 9999f;
    }
    
    // adjust the important components that need to be enabled on start
    void AdjustComponentsStatesOnStart()
    {
        navmeshAgent.enabled = false;
        capsuleCollider.enabled = true;
        capsuleCollider.isTrigger = false;
        
        agentObstacle.enabled = true;
        controller.enabled = true;
        controller.minMoveDistance = 0;
        navmeshAgent.stoppingDistance = 0;

        if (useRootMotion) anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
        else anim.updateMode = AnimatorUpdateMode.Normal;

        if (attackState.coverShooterOptions.coverShooter) navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        normalState.CurrentAudioValidate();
        normalState.GenerateAudioRandomTime();
        alertState.CurrentAudioValidate();
        alertState.GenerateAudioRandomTime();
    }

    #endregion

    #region Vision
    
    // vision cone
    void VisionCheck()
    {
        if (visionFramesElapsed < vision.pulseRate) {
            visionFramesElapsed++;
            return;
        }
        
        visionFramesElapsed = 0;
        enemiesToAttack.Clear();

        float radius, angle;
        Vector3 npcDir = transform.position + centerPosition;
        
        // set the radius and angle according to state
        switch (state) {
            case State.normal:
                if (attackState.surprised.startSurprisedTimerState) {
                    angle = vision.visionDuringAttackState.coneAngle;
                    radius = vision.visionDuringAttackState.sightRange;
                }
                else {
                    angle = vision.visionDuringNormalState.coneAngle;
                    radius = vision.visionDuringNormalState.sightRange;
                }
                break;
            case State.alert:
                angle = vision.visionDuringAlertState.coneAngle;
                radius = vision.visionDuringAlertState.sightRange;
                break;
            case State.attack:
                angle = vision.visionDuringAttackState.coneAngle;
                radius = vision.visionDuringAttackState.sightRange;
                break;
            default:
                angle = vision.visionDuringNormalState.coneAngle;
                radius = vision.visionDuringNormalState.sightRange;
                break;
        }
        
        int visionCollNum = Physics.OverlapSphereNonAlloc(transform.position, radius, visionColl, vision.hostileAndAlertLayers);

        for (int i=0; i<visionCollNum; i++) {
            if (enemiesToAttack.Count >= 5) break;

            // check for alert tags
            if (state != State.attack && !isSeenVisionAlertTags) {
                int alertLayers = vision.layersToDetect;

                if (vision.alertTagsDict.ContainsKey(visionColl[i].tag)) {
                    Collider hit = visionColl[i];

                    // check if not within vision angle
                    if (Vector3.Angle(visionT.forward, (hit.transform.position - npcDir)) > (angle * 0.5f))
                    {
                        continue;
                    }

                    // check height
                    float alertHeight = (hit.transform.position.y) - (controller.center.y + visionT.position.y + vision.sightLevel + vision.maxSightLevel);
                    if (alertHeight > 0f)
                    {
                        continue;
                    }
                            
                    Collider[] objColliders = hit.transform.GetComponentsInChildren<Collider>();
                    int detectionScore = 0;
                    
                    foreach (var item in objColliders) {
                        RaycastHit rayHit;
                        Vector3 colDir = item.ClosestPoint(item.bounds.center) - npcDir;
                        
                        // check center
                        if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, alertLayers)) {
                            if (item.transform.IsChildOf(rayHit.transform)) {
                                detectionScore++;
                            }else{
                                // checking top left
                                colDir = (item.ClosestPoint(item.bounds.max) - npcDir);
                                if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, alertLayers)) {
                                    if (item.transform.IsChildOf(rayHit.transform)) {
                                        detectionScore++;
                                    }else{
                                        // checking top right
                                        colDir = (item.ClosestPoint(new Vector3(item.bounds.center.x - item.bounds.extents.x, item.bounds.center.y + item.bounds.extents.y, item.bounds.center.z + item.bounds.extents.z)) - npcDir);
                                        if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, alertLayers)) {
                                            if (item.transform.IsChildOf(rayHit.transform)) {
                                                detectionScore++;
                                            }else{
                                                // checking bottom right
                                                colDir = (item.ClosestPoint(item.bounds.min) - npcDir);
                                                if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, alertLayers)) {
                                                    if (item.transform.IsChildOf(rayHit.transform)) {
                                                        detectionScore++;
                                                    }else{
                                                        // checking bottom left
                                                        colDir = (item.ClosestPoint(new Vector3(item.bounds.center.x + item.bounds.extents.x, item.bounds.center.y - item.bounds.extents.y, item.bounds.center.z + item.bounds.extents.z)) - npcDir);
                                                        if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, alertLayers)) {
                                                            if (item.transform.IsChildOf(rayHit.transform)) {
                                                                detectionScore++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // if detection score is bigger than 0 means something has been seen
                    if (detectionScore > 0) {
                        state = State.alert;
                        seenVisionAlertTag = hit.transform.tag;

                        normalStateActive = false;
                        string fallBackTag = vision.alertTagsDict[seenVisionAlertTag].fallBackTag;

                        if (fallBackTag.Length > 0) hit.transform.tag = fallBackTag;
                        else hit.transform.tag = "Untagged";

                        // play random audio specific of this tag
                        StopSystemsAudios("vision");
                        vision.AlertTagsPlayRandomAudio(seenVisionAlertTag);
                        isSeenVisionAlertTags = true;

                        // vision alert tags options such as animations playing and moving to location
                        if (vision.alertTagsDict[seenVisionAlertTag].moveToLocation) {
                            endPoint = ValidateEnemyYPoint(hit.transform.position);
                            CallPathSetup();
                            goingToVisionAlertTag = true;
                            alertStateActive = true;
                        }else{
                            StartCoroutine(AlertStateIdle(true));
                        }

                        waypointInterrupted = true;
                        CallOthersToVisionAlert();
                    }
                }
            }
            
            // check for hostile tags
            if (System.Array.IndexOf(vision.hostileTags, visionColl[i].tag) >= 0) {
                Collider hostile = visionColl[i];
    
                // check if not within vision angle
                if (Vector3.Angle(visionT.forward, (hostile.transform.position - npcDir)) > (angle * 0.5f))
                {
                    continue;
                }

                // check height
                float suspectHeight = hostile.transform.position.y - (controller.center.y + visionT.position.y + vision.sightLevel + vision.maxSightLevel);
                if (suspectHeight > 0f)
                {
                    continue;
                }

                Collider[] enemyToAttackColliders = hostile.transform.GetComponentsInChildren<Collider>();
                int colSize = enemyToAttackColliders.Length;
                int detectionScore = 0;

                // set the raycast layers
                int layersToHit;
                if (state != State.attack) layersToHit = vision.layersToDetect | vision.hostileAndAlertLayers;
                else {
                    if (attackState.coverShooterOptions.coverShooter) layersToHit = vision.hostileAndAlertLayers;
                    else layersToHit = vision.layersToDetect | vision.hostileAndAlertLayers;
                }
                
                foreach (var item in enemyToAttackColliders) {
                    RaycastHit rayHit;
                    Vector3 colDir = item.ClosestPoint(item.bounds.center) - npcDir;
                    
                    // start with center raycast, if caught nothing -> top left, if caught nothing -> top right
                    if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                        if (item.transform.IsChildOf(rayHit.transform)) {
                            detectionScore++;
                        }else{
                            // checking top left
                            colDir = (item.ClosestPoint(item.bounds.max) - npcDir);
                            if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                                if (item.transform.IsChildOf(rayHit.transform)) {
                                    detectionScore++;
                                }else{
                                    // checking top right
                                    colDir = (item.ClosestPoint(new Vector3(item.bounds.center.x - item.bounds.extents.x, item.bounds.center.y + item.bounds.extents.y, item.bounds.center.z + item.bounds.extents.z)) - npcDir);
                                    if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                                        if (item.transform.IsChildOf(rayHit.transform)) {
                                            detectionScore++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // prevent adding colliders of the same gameobject
                bool exists = false;
                if (enemiesToAttack.Count > 0) {
                    foreach (var coll in enemiesToAttack) {
                        if (coll.transform.IsChildOf(hostile.transform)) {
                            exists = true;
                            break;
                        }
                    }
                }
                
                if (!exists) {
                    // check the detection score
                    if (colSize <= 2) {
                        if (detectionScore >= 1) {
                            if (enemiesToAttack.Count < 5) {
                                enemiesToAttack.Add(hostile);
                            }
                        }
                    }else{
                        // enemy is seen if more than half of it's colliders are seen
                        if (detectionScore >= colSize/2) {
                            if (enemiesToAttack.Count < 5) {
                                enemiesToAttack.Add(hostile);
                            }
                        }
                    }
                }
            }else{
                continue;
            }
        }

        // choose the nearest enemy
        if (enemiesToAttack.Count > 0) {
            // order the enemies by distance
            enemiesToAttack.Sort((x, y) => { return (transform.position - x.transform.position).sqrMagnitude.CompareTo((transform.position - y.transform.position).sqrMagnitude); });

            if (enemyToAttack) lastEnemy = enemyToAttack.transform;

            // target the least distance -> first item (index 0)
            enemyToAttack = enemiesToAttack[0].transform.gameObject;
            enemyPosition = ValidateEnemyYPoint(enemyToAttack.transform.position);
            enemyColPoint = enemiesToAttack[0].ClosestPoint(enemiesToAttack[0].bounds.center);

            captureEnemyTimeStamp = Time.time;  // make a timestamp
            checkEnemyPosition = Vector3.zero;
            if (state != State.attack && !isHit) AttackPreparations();
        }else{
            if (startAttackTimer) return;
            enemyToAttack = null;
        }
    }

    void CollisionFunc(BlazeAI script, GameObject hit) 
    {
        if (state == State.attack) {
            if (!reachedEnd || !attackState.pushAgentsBackwards || strafing) return;
            if (!attackState.coverShooterOptions.coverShooter) {
                if (enemyToAttack != null) {
                    if (hit != enemyToAttack) {
                        // backup if backing up agent hits this one
                        if (idleAttack && script.attackBackUp && !attackBackUp && !script.shouldAttack) {
                            attackState.moveBackwardsDist += 1.2f;
                            attackState.distanceFromEnemy += 1.2f;
                            backedUpBy = script;
                            return;
                        } else return;
                    } else return;
                } else return;
            } else return;
        }else{
            if (script != null) {
                if (script.stopPriority < stopPriority) {
                    // if the other agent has already stopped then quit
                    if (script.stop || script.reachedEnd) return;

                    normalStateActive = false;
                    alertStateActive = false;
                    
                    if (state == State.normal) animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition, normalState.useAnimations);
                    if (state == State.alert) animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition, alertState.useAnimations);
                    
                    stop = true;

                    controllerRadius = controller.radius;
                    controller.radius = 0.01f;
                }
            }
        }
    }

    // detect nearby enemies when in root motion only to stop 
    void SphereDetection()
    {
        if (sphereDetectionFrame >= vision.pulseRate) {
            sphereDetectionFrame = 0;
            float radius;
            
            if (attackState.coverShooterOptions.coverShooter) {
                if (state == State.attack) radius = controller.radius + 1f;
                else radius = controller.radius + 1f;
            }else{
                if (state == State.attack) radius = navmeshAgent.radius + 0.3f;
                else radius = controller.radius + 0.3f;
            }
            
            int sphereDetectionInt = Physics.OverlapSphereNonAlloc(transform.position, radius, sphereDetectionColl, layersToAvoid);
            sphereDetectedScripts.Clear();

            for (int i=0; i<sphereDetectionInt; i++) {
                var script = sphereDetectionColl[i].GetComponent<BlazeAI>();
                if (script != null && script != this) {
                    if (!sphereDetectedScripts.Contains(script) && script != this) sphereDetectedScripts.Add(script);
                }
            }

            if (sphereDetectedScripts.Count > 0) {
                foreach (var script in sphereDetectedScripts) {
                    Vector3 targetDir = script.transform.position - transform.position;
                    if (state == State.attack && attackState.coverShooterOptions.coverShooter) {
                        if (script.backedUpBy != null && script.backedUpBy != this) 
                            CollisionFunc(script, script.transform.gameObject);
                    }else{
                        if (Vector3.Angle(targetDir, transform.forward) <= (100f * 0.5f)) 
                            CollisionFunc(script, script.transform.gameObject);
                    }
                }
            }

        } else { sphereDetectionFrame++; }
    }

    #endregion

    #region Audios
    
    //reset and fix the audio patrol timers depending on the state
    void PatrolAudioTimersFix(State state)
    {
        if (state == State.normal) {
            alertState.audioPlayTimer = 0f;
        }

        if (state == State.alert) {
            normalState.audioPlayTimer = 0f;
        }

        if (state == State.attack) {
            normalState.audioPlayTimer = 0f;
            alertState.audioPlayTimer = 0f;
        }
    }

    //play random NPC audio sources of patrols
    public void PlayPatrolAudio()
    {
        if (state == State.normal){
            normalState.PlayRandomPatrolAudio();
        }
    }

    //stop audios of all systems to leave room for a particular one
    public void StopSystemsAudios(string audioSystemNotToStop)
    {
        if (audioSystemNotToStop == "normal") {
            alertState.StopCurrentAudio();
            distractions.StopCurrentAudio();
            vision.StopCurrentAudio();
        }
        else if (audioSystemNotToStop == "alert") {
            normalState.StopCurrentAudio();
            distractions.StopCurrentAudio();
            vision.StopCurrentAudio();
        }
        else if (audioSystemNotToStop == "distraction") {
            normalState.StopCurrentAudio();
            alertState.StopCurrentAudio();
            vision.StopCurrentAudio();
        }
        else if (audioSystemNotToStop == "hit") {
            normalState.StopCurrentAudio();
            alertState.StopCurrentAudio();
            vision.StopCurrentAudio();
            distractions.StopCurrentAudio();
        }
        else if (audioSystemNotToStop == "attack") {
            normalState.StopCurrentAudio();
            alertState.StopCurrentAudio();
            vision.StopCurrentAudio();
            distractions.StopCurrentAudio();
        }
        else if (audioSystemNotToStop == "death") {
            normalState.StopCurrentAudio();
            alertState.StopCurrentAudio();
            vision.StopCurrentAudio();
            distractions.StopCurrentAudio();
            attackState.StopCurrentAudio();
        }
    }

    #endregion

    #region Hits

    // hit this agent
    public void Hits (GameObject enemy = null)
    {
        if (!enabled) return;
        
        attackStateActive = false;
        normalStateActive = false;
        alertStateActive = false;

        StopAllCoroutines();
        ResetAllFlags();

        StopSystemsAudios("attack");
        StopSystemsAudios("hit");

        hits.PlayAudio();
        isHit = true;
        coverLocationSet = false;

        if (hits.cancelAttackIfHit) {
            shouldAttack = false;
            startAttackTimer = false;
            attackTimer = 0f;
        }

        if (hits.canPlayAnim) animationManager.PlayAnimationState(hits.animationName, hits.animationTransition, hits.useAnimation, true);
        hits.canPlayAnim = false;

        if (!startCoverTimer) state = State.hit;
        StartCoroutine(ReturnFromHit(enemy));
    }

    // return to alert state
    IEnumerator ReturnFromHit (GameObject enemyObject = null)
    {
        yield return new WaitForSeconds(hits.hitDuration);

        hits.canPlayAnim = true;
        hits.gapTimer = 0f;

        if (attackState.coverShooterOptions.coverShooter && enemyToAttack) {
            FindCover(true, currentCover);
            attackStateActive = true;
            isHit = false;
            yield break;
        }
        
        if (enemyObject == null) {
            state = State.alert;
            alertStateActive = true;
            StateWalk();
        }else{
            NavMeshHit hit;
            if (NavMesh.SamplePosition(ValidateEnemyYPoint(enemyObject.transform.position), out hit, navmeshAgent.radius, NavMesh.AllAreas)) {
                if (IsPathReachable(hit.position)) {
                    wpIdleTriggered = true;
                    captureEnemyTimeStamp = Time.time;
                    checkEnemyPosition = hit.position;
                    waitFrameRan = false;
                    CallPathSetup();
                    TurnToAttack();
                }else{
                    state = State.alert;
                    alertStateActive = true;
                    StateWalk();
                }
            }else{
                state = State.alert;
                alertStateActive = true;
                StateWalk();
            }
        }

        isHit = false;
    }

    #endregion

    #region Death
    
    // kill the agent
    public void Death()
    {
        if (!enabled) return;

        StopAllCoroutines();

        alertState.DisableScripts();
        normalState.DisableScripts();
        distractions.DisableScript();
        attackState.DisableScript();
        ResetAllFlags();
        
        hits.canPlayAnim = true;
        hits.gapTimer = 0f;
        isHit = false;

        animationManager.PlayAnimationState(death.animationName, death.animationTransition, death.useAnimation);
        
        StopSystemsAudios("death");
        death.PlayAudio();
        
        agentObstacle.enabled = false;
        controller.enabled = false;
        navmeshAgent.enabled = false;
        capsuleCollider.enabled = false;

        death.TriggerScript();
        enabled = false;
    }

    // return the agent to be alive
    public void Undeath()
    {
        if (enabled) return;
        Start();
        enabled = true;
    }

    #endregion

    #region Methods Running in Update
    
    // functionalities that require state check in Update
    void FlaggedFunctions()
    {
        // turn navmesh agent on/off accordingly
        if (attackState.coverShooterOptions.coverShooter) {
            if (state == State.attack && !idleAttack && !startAttackTimer && !attackBackUp && !canAttackEnemyUnreachable) {
                if (noCoversFound) DisableAgent();
                else EnableAgent();
            } else {
                DisableAgent();
            }
        }else{
            if (state == State.attack && !attackBackUp && !enemyToAttack && !shouldAttack) {
                if (checkEnemyPosition != Vector3.zero) EnableAgent();
                else DisableAgent();
            }else{
                DisableAgent();
            }
        }

        // time threshold between enabling/disabling agent
        if (agentChange) {
            agentChangeTimer += Time.deltaTime;
            if (agentChangeTimer >= 0.1f) {
                agentChange = false;
                agentChangeTimer = 0f;
            }
        }

        // timer to enable agent
        if (ranEnableAgent) {
            ranEnableAgentTimer += Time.deltaTime;
            if (ranEnableAgentTimer >= 0.05f) {
                controller.enabled = false;
                navmeshAgent.enabled = true;
                isAgent = true;

                ranEnableAgent = false;
                ranEnableAgentTimer = 0f;
            }
        }

        // timer to disable agent
        if (ranDisableAgent) {
            ranDisableAgentTimer += Time.deltaTime;
            if (ranDisableAgentTimer >= 0.05f) {
                waitFrameRan = false;
                agentObstacle.enabled = true;

                controller.enabled = true;
                isAgent = false;

                ranDisableAgent = false;
                ranDisableAgentTimer = 0f;
            }
        }

        // run the hits anim. gap timer
        if (isHit && !hits.canPlayAnim) {
            hits.gapTimer += Time.deltaTime;
            if (hits.gapTimer >= hits.animPlayGap) {
                hits.canPlayAnim = true;
                hits.gapTimer = 0f;
            }
        }

        // wait time before strafing
        if (enemyToAttack && strafeWait && !strafeRan) {
            currentStrafeDirBlocked = false;
            strafeWaitTimer += Time.deltaTime;
            if (strafeWaitTimer >= strafeWaitTime) {
                Strafe();
            }
        }

        // agent is strafing
        if (enemyToAttack && strafing) {
            StrafeMovement(currentStrafingDirection);
            strafeTimer += Time.deltaTime;
            if (strafeTimer >= strafeTime) {
                ResetStrafing();
                reachedEnd = true;
                IdleAttackState();
            }
        }

        // turn to path corner
        if (waypoints.useMovementTurning) {
            if ((state == State.normal || state == State.alert) && !reachedEnd && hasPath && !distracted && !isHit && !attackState.surprised.isSurprised && !justEndedAttackState) {
                Vector3 destination = new Vector3(pathCorner.x, transform.position.y, pathCorner.z);
                Vector3 toOther = (destination - transform.position).normalized;
                float dotProd = Vector3.Dot(toOther, transform.forward);

                if (turningToCorner) {
                    if (dotProd >= 0.97f) {
                        turningToCorner = false;
                        if (state == State.normal) normalStateActive = true;
                        else alertStateActive = true;
                    } else {
                        waitFrameRan = false;
                        StopMovement();
                        CallPathSetup();
                        RotateToTarget(destination, waypoints.turnSpeed);

                        Vector3 heading = pathCorner - transform.position;
                        int dirNum = (int) distractions.AngleDir(transform.forward, heading, transform.up);

                        if (dirNum == 1) {
                            if (state == State.normal) animationManager.PlayAnimationState(waypoints.rightTurnAnimNormal, waypoints.turningAnimT);
                            else animationManager.PlayAnimationState(waypoints.rightTurnAnimAlert, waypoints.turningAnimT);
                        }
                    
                        if (dirNum == -1) {
                            if (state == State.normal) animationManager.PlayAnimationState(waypoints.leftTurnAnimNormal, waypoints.turningAnimT);
                            else animationManager.PlayAnimationState(waypoints.leftTurnAnimAlert, waypoints.turningAnimT);
                        }
                    }
                }else{
                    if (dotProd <= waypoints.movementTurningSensitivity) turningToCorner = true;
                }

            }else{
                if (state == State.attack) turningToCorner = false;
            }
        }

        // turn to distraction when flagged
        if (distractionTurn && !attackState.surprised.startSurprisedTimerState) {
            RotateToTarget(distractionPosition, distractions.turnSpeed);
            Vector3 toOther = (new Vector3(distractionPosition.x, transform.position.y, distractionPosition.z) - transform.position).normalized;
            float dotProd = Vector3.Dot(toOther, transform.forward);

            if (dotProd >= 0.98f) {
                if (state == State.normal) animationManager.PlayAnimationState(normalState.idleAnimationName, normalState.idleAnimationTransition);
                else animationManager.PlayAnimationState(alertState.idleAnimationName, alertState.idleAnimationTransition);
            }
        }
        
        // waypoint rotation
        if (startWaypointRotation && reachedEnd) {
            waypointRotationAnimationTimer += Time.deltaTime;

            // play animation
            if (waypointRotationAnimationTimer >= waypoints.timeBeforeTurning) {

                // turn right
                if (waypointTurnDir == 1) {
                    if (state == State.normal) {
                        animationManager.PlayAnimationState(waypoints.rightTurnAnimNormal, waypoints.turningAnimT);
                    } else {
                        animationManager.PlayAnimationState(waypoints.rightTurnAnimAlert, waypoints.turningAnimT);
                    }
                }

                // turn left
                if (waypointTurnDir == -1) {
                    if (state == State.normal) {
                        animationManager.PlayAnimationState(waypoints.leftTurnAnimNormal, waypoints.turningAnimT);
                    } else {
                        animationManager.PlayAnimationState(waypoints.leftTurnAnimAlert, waypoints.turningAnimT);
                    }
                }

                float[] tempArr = new float[2];
                tempArr[0] = waypoints.waypointsRotation[waypointIndex].x;
                tempArr[1] = waypoints.waypointsRotation[waypointIndex].y;

                rotateToWaypoint(tempArr, waypoints.turnSpeed);
                float dotProd = Vector3.Dot(
                    (new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, 
                                transform.position.y, 
                                transform.position.z + waypoints.waypointsRotation[waypointIndex].y
                                ) 
                    - transform.position).normalized, transform.forward
                );
                
                if (dotProd >= 0.97f) {
                    startWaypointRotation = false;
                    waypointRotationAnimationTimer = 0f;

                    if (state == State.normal) StartCoroutine(NormalStateIdle());
                    if (state == State.alert) StartCoroutine(AlertStateIdle());
                }
            }
        }else{
            if (!reachedEnd) startWaypointRotation = false;
        }
        
        // if gravity is enabled, trigger gravity method
        if (enableGravity) KeepToGravity();

        // count the duration of surprised state
        if (attackState.surprised.startSurprisedTimerState) {
            attackState.surprised.startSurprisedTimer += Time.deltaTime;

            if (attackState.surprised.alwaysRotate) RotateToTarget(enemyToAttack.transform.position, 7f);
            if (attackState.surprised.startSurprisedTimer >= attackState.surprised.surprisedDuration) {
                TurnToAttack();
            }
        }

        // get enemy cover
        if (attackState.coverShooterOptions.coverShooter && getEnemyCover && enemyToAttack && shouldAttack) {
            if (getEnemyCoverFrames >= 10) {
                getEnemyCoverFrames = 0;
                
                if (enemyToAttack.transform != lastEnemy || enemyCol == null) enemyCol = enemyToAttack.GetComponent<Collider>();
                
                if (enemyCol == null) enemyCover = null;
                else {
                    enemyCoverColl[0] = null;
                    Physics.OverlapSphereNonAlloc(enemyPosition, (enemyCol.bounds.size.x + enemyCol.bounds.size.z) + 0.3f, enemyCoverColl, attackState.coverShooterOptions.coverLayers);
                    if (enemyCoverColl[0]) {
                        if (currentCover != null) {
                            if (currentCover != enemyCoverColl[0].transform) enemyCover = enemyCoverColl[0].transform;
                            else enemyCover = null;
                        }else{
                            enemyCover = enemyCoverColl[0].transform;
                        }
                    } 
                    else enemyCover = null;
                }
            }
            else getEnemyCoverFrames++;
        }

        // timer of attack duration
        if (startAttackTimer) {
            attackTimer += Time.deltaTime;
            // if in cover shooter and attacking -> stop and hide if line of sight is broken
            if (attackState.coverShooterOptions.coverShooter) {
                if (enemyToAttack) RotateToTarget(enemyPosition, attackState.attackRotateSpeed);
                if (coverShooterAttackingFrames >= 7) {
                    coverShooterAttackingFrames = 0;
                    RaycastHit hit;
                    int hitLayers;

                    if (getEnemyCover) hitLayers = vision.hostileAndAlertLayers | vision.layersToDetect | attackState.coverShooterOptions.coverLayers | obstacleLayers;
                    else hitLayers = vision.hostileAndAlertLayers | vision.layersToDetect | obstacleLayers;
                    
                    if (Physics.Raycast(transform.position + centerPosition, enemyPosition - (transform.position + centerPosition), out hit, Mathf.Infinity, hitLayers)) {
                        if (System.Array.IndexOf(vision.hostileTags, hit.transform.tag) < 0) {
                            if (getEnemyCover) {
                                if (hit.transform != enemyCover || hit.transform == currentCover) StopAttack();
                            }
                            else StopAttack();
                        }
                    }
                }
                else coverShooterAttackingFrames++;

            }else{
                if (attackState.onAttackRotate) {
                    if (enemyToAttack) RotateToTarget(enemyToAttack.transform.position, attackState.attackRotateSpeed);
                }else{
                    if (enemyPositionOnAttack != Vector3.zero) RotateToTarget(enemyPositionOnAttack, attackState.attackRotateSpeed);
                }
            }

            if (attackTimer >= attackState.currentAttackTime) StopAttack();
        }

        // start waiting in cover
        if (startCoverTimer) {
            coverTimer += Time.deltaTime;
            
            // rotate to match normal
            if (attackState.coverShooterOptions.coverAnimations.rotateToNormal) {
                Quaternion rot = Quaternion.FromToRotation(Vector3.forward, coverNormal);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 300f * Time.deltaTime);
            }
            
            if (enemyToAttack) {
                if (coverTimer >= 0.2f) {
                    RaycastHit hit;
                    Vector3 startDir = transform.position + centerPosition;
                    Vector3 dir = enemyColPoint - startDir;
                    
                    // check if cover is blown
                    int hitLayers = vision.hostileAndAlertLayers | vision.layersToDetect | attackState.coverShooterOptions.coverLayers | obstacleLayers;
                    if (Physics.Raycast(startDir, dir, out hit, Mathf.Infinity, hitLayers)) {
                        if (System.Array.IndexOf(vision.hostileTags, hit.transform.tag) >= 0) {
                            if (attackState.coverShooterOptions.coverBlownState == CoverShooterOptions.CoverBlownState.AlwaysAttack) FromCoverAttack();
                            else if (attackState.coverShooterOptions.coverBlownState == CoverShooterOptions.CoverBlownState.TakeCover) FindCover(true, currentCover);
                            else {
                                int chance = Random.Range(1, 3);
                                if (chance == 1) FromCoverAttack();
                                else FindCover(true, currentCover);

                                startCoverTimer = false;
                                coverTimer = 0f;
                            }
                        }
                    }
                }
            }else{
                startCoverTimer = false;
                coverTimer = 0f;
                NormalAttackMovement();
            }

            if (coverTimer >= actualCoverTime) FromCoverAttack();

        }else{
            if (attackState.coverShooterOptions.coverShooter) {
                attackState.coverShooterOptions.coverAnimations.DisableCoverScripts();
            }
        }

        // stop agent for one second if touches another agent
        if (stop) {
            stopTimer += Time.deltaTime;
            if (stopTimer >= 1f) {
                stop = false;

                controller.radius = controllerRadius;
                
                if (state == State.normal) normalStateActive = true;
                if (state == State.alert) alertStateActive = true;

                stopTimer = 0f;
            }
        }

        // the attack interval timer
        if (startIntervalTimer && state == State.attack) {
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= attackState.attackInIntervalsTime) {
                if (attackState.coverShooterOptions.coverShooter) {
                    FromCoverAttack();
                }else{
                    IntervalAttack();
                }
            }
        }else{
            if (state != State.attack) {
                startIntervalTimer = false;
                intervalTimer = 0f;
            }
        }

        // fix and set the audio patrol timers
        PatrolAudioTimersFix(state);

        // if in alert mode will return to normal mode (if set so)
        ReturnNormalTimer();

        // check collisions with other Blaze AI agents
        if (!ranEnableAgent && !isAgent) SphereDetection();
        
        // always turn off attack flag after two seconds if there's no enemy
        if (enemyToAttack == null) {
            emptyEnemyTimer += Time.deltaTime;
            if (emptyEnemyTimer >= 2f) {
                emptyEnemyTimer = 0f;
                shouldAttack = false;
            }
        }else{
            emptyEnemyTimer = 0f;
        }

        // flags to reset if attack state
        if (state == State.attack) {
            goingToDistractionPoint = false;
            goingToVisionAlertTag = false;
        }

        // rotate agent if there is no path in order to find a path around
        if (forceTurn) {
            transform.Rotate(0f, 300f * Time.deltaTime, 0f);
            
            if (state == State.normal) {
                animationManager.PlayAnimationState(waypoints.rightTurnAnimNormal, waypoints.turningAnimT);
            }
            else {
                animationManager.PlayAnimationState(waypoints.rightTurnAnimAlert, waypoints.turningAnimT);
            }

            bool pathValidation = NavMesh.CalculatePath(ValidateEnemyYPoint(pathFindingProxy.position), ValidateEnemyYPoint(transform.position + (transform.forward * 2f)), NavMesh.AllAreas, path);
            if (pathValidation) {
                forceTurn = false;
                StateWalk();
            }
        }
    }

    // this function updates properties from this main script to the other classes
    void MainToClassesUpdate()
    {
        if (attackState.surprised.startSurprisedTimerState) distractions.inAttack = true;
        else distractions.inAttack = attackStateActive;
    }

    #endregion
    

    // validate the states on inspector change
    // can only use one state on start
    void StatesInspectorValidation()
    {
        if (alertState == null || normalState == null) return;

        if (!alertState.useAlertStateOnStart && !normalState.useNormalStateOnStart) {
            normalState.useNormalStateOnStart = true;
            alertStateActive = false;
        }

        if (alertState.useAlertStateOnStart && normalState.useNormalStateOnStart) {
            alertState.useAlertStateOnStart = !alertState.inspectorStateOfStart;
            normalState.useNormalStateOnStart = !normalState.inspectorStateOfStart;
        }

        normalState.inspectorStateOfStart = normalState.useNormalStateOnStart;
        alertState.inspectorStateOfStart = alertState.useAlertStateOnStart;
    }
    
    // change state from another agent
    public void ChangeState(string stringState, bool goToVisionAlertTag = false)
    {
        if (stringState == "alert")
        {
            state = State.alert;
            normalStateActive = false;
            alertStateActive = true;
        }

        if (goToVisionAlertTag) {
            StopAllCoroutines();
            waitFrameRan = false;
            CallPathSetup();
            goingToVisionAlertTag = true;
        }
    }

    // disable all scripts on game start
    void DisableAllSystemScripts()
    {
        normalState.DisableScripts();
        alertState.DisableScripts();
        distractions.DisableScript();
        death.DisableScript();
        attackState.DisableScript();
        attackState.coverShooterOptions.coverAnimations.DisableCoverScripts();
    }

    // reset all the flags
    void ResetAllFlags()
    {
        distracted = false;
        distractionTurn = false;
        goingToDistractionPoint = false;
        wpRandomMode = false;
        activateRay = false;
        isSeenVisionAlertTags = false;
        waitFrameRan = false;
        goingToVisionAlertTag = false;
        startWaypointRotation = false;
        waypointRotationAnimationTimer = 0f;
        justEndedAttackState = false;
        ResetStrafing();
        currentStrafeDirBlocked = false;
    }

    // load the profile properties
    public void LoadProfile(BlazeProfile profile)
    {
        if (profile == null) return;

        groundLayers = profile.groundLayers;
        pathRecalculationRate = profile.pathRecalculationRate;
        pathSmoothing = profile.pathSmoothing;
        pathSmoothingFactor = profile.pathSmoothingFactor;
        proxyOffset = profile.proxyOffset;
        enableGravity = profile.enableGravity;
        gravityStrength = profile.gravityStrength;
        useRootMotion = profile.useRootMotion;
        centerPosition = profile.centerPosition;
        showCenterPosition = profile.showCenterPosition;

        avoidFacingObstacles = profile.avoidFacingObstacles;
        obstacleRayDistance = profile.obstacleRayDistance;
        obstacleRayOffset = profile.obstacleRayOffset;
        obstacleLayers = profile.obstacleLayers;

        layersToAvoid = profile.layersToAvoid;

        waypoints.instantMoveAtStart = profile.waypoints.instantMoveAtStart;
        waypoints.loop = profile.waypoints.loop;
        waypoints.randomize = profile.waypoints.randomize;
        waypoints.rightTurnAnimNormal = profile.waypoints.rightTurnAnimNormal;
        waypoints.leftTurnAnimNormal = profile.waypoints.leftTurnAnimNormal;
        waypoints.rightTurnAnimAlert = profile.waypoints.rightTurnAnimAlert;
        waypoints.leftTurnAnimAlert = profile.waypoints.leftTurnAnimAlert;
        waypoints.turningAnimT = profile.waypoints.turningAnimT;
        waypoints.useMovementTurning = profile.waypoints.useMovementTurning;
        waypoints.movementTurningSensitivity = profile.waypoints.movementTurningSensitivity;
        
        vision.layersToDetect = profile.vision.layersToDetect;
        vision.hostileAndAlertLayers = profile.vision.hostileAndAlertLayers;
        vision.hostileTags = profile.vision.hostileTags;
        vision.alertTags = new Vision.AlertOptions[profile.vision.alertTags.Length];
        for (var i=0; i<vision.alertTags.Length; i++) {
            vision.alertTags[i].tag = profile.vision.alertTags[i].tag;
            vision.alertTags[i].fallBackTag = profile.vision.alertTags[i].fallBackTag;
            vision.alertTags[i].animationName = profile.vision.alertTags[i].animationName;
            vision.alertTags[i].time = profile.vision.alertTags[i].time;
            vision.alertTags[i].moveToLocation = profile.vision.alertTags[i].moveToLocation;
            vision.alertTags[i].callOthersToLocation = profile.vision.alertTags[i].callOthersToLocation;
        }
        vision.visionDuringNormalState =  new Vision.normalVision(profile.vision.visionDuringNormalState.coneAngle, profile.vision.visionDuringNormalState.sightRange);
        vision.visionDuringAlertState =  new Vision.alertVision(profile.vision.visionDuringAlertState.coneAngle, profile.vision.visionDuringAlertState.sightRange);
        vision.visionDuringAttackState =  new Vision.attackVision(profile.vision.visionDuringAttackState.coneAngle, profile.vision.visionDuringAttackState.sightRange);
        vision.sightLevel = profile.vision.sightLevel;
        vision.maxSightLevel = profile.vision.maxSightLevel;
        vision.pulseRate = profile.vision.pulseRate;

        normalState.moveSpeed = profile.normalState.moveSpeed;
        normalState.rotationSpeed = profile.normalState.rotationSpeed;
        normalState.waitTime = profile.normalState.waitTime;
        normalState.randomizeWaitTime = profile.normalState.randomizeWaitTime;
        normalState.randomizeWaitTimeBetween = profile.normalState.randomizeWaitTimeBetween;
        normalState.useAnimations = profile.normalState.useAnimations;
        normalState.idleAnimationName = profile.normalState.idleAnimationName;
        normalState.idleAnimationTransition = profile.normalState.idleAnimationTransition;
        normalState.moveAnimationName = profile.normalState.moveAnimationName;
        normalState.moveAnimationTransition = profile.normalState.moveAnimationTransition;
        normalState.useRandomAnimationsOnIdle = profile.normalState.useRandomAnimationsOnIdle;
        normalState.randomIdleAnimationNames = profile.normalState.randomIdleAnimationNames;
        normalState.randomIdleAnimationTransition = profile.normalState.randomIdleAnimationTransition;
        normalState.enableScripts = profile.normalState.enableScripts;
        normalState.playAudiosOnPatrol = profile.normalState.playAudiosOnPatrol;
        normalState.playAudioEvery = profile.normalState.playAudioEvery;

        alertState.useAlertStateOnStart = profile.alertState.useAlertStateOnStart;
        alertState.moveSpeed = profile.alertState.moveSpeed;
        alertState.rotationSpeed = profile.alertState.rotationSpeed;
        alertState.waitTime = profile.alertState.waitTime;
        alertState.randomizeWaitTime = profile.alertState.randomizeWaitTime;
        alertState.randomizeWaitTimeBetween = profile.alertState.randomizeWaitTimeBetween;
        alertState.useAnimations = profile.alertState.useAnimations;
        alertState.idleAnimationName = profile.alertState.idleAnimationName;
        alertState.idleAnimationTransition = profile.alertState.idleAnimationTransition;
        alertState.moveAnimationName = profile.alertState.moveAnimationName;
        alertState.moveAnimationTransition = profile.alertState.moveAnimationTransition;
        alertState.useRandomAnimationsOnIdle = profile.alertState.useRandomAnimationsOnIdle;
        alertState.randomIdleAnimationNames = profile.alertState.randomIdleAnimationNames;
        alertState.randomIdleAnimationTransition = profile.alertState.randomIdleAnimationTransition;
        alertState.returnToNormalState = profile.alertState.returnToNormalState;
        alertState.timeBeforeReturningNormal = profile.alertState.timeBeforeReturningNormal;
        alertState.useAnimationOnReturn = profile.alertState.useAnimationOnReturn;
        alertState.animationNameOnReturn = profile.alertState.animationNameOnReturn;
        alertState.animationOnReturnTransition = profile.alertState.animationOnReturnTransition;
        alertState.playAudioOnReturn = profile.alertState.playAudioOnReturn;
        alertState.enableScripts = profile.alertState.enableScripts;
        alertState.playAudiosOnPatrol = profile.alertState.playAudiosOnPatrol;
        alertState.playAudioEvery = profile.alertState.playAudioEvery;

        attackState.coverShooterOptions.coverShooter = profile.attackState.coverShooterOptions.coverShooter;
        attackState.coverShooterOptions.coverLayers = profile.attackState.coverShooterOptions.coverLayers;
        attackState.coverShooterOptions.hideSensitivity = profile.attackState.coverShooterOptions.hideSensitivity;
        attackState.coverShooterOptions.searchDistance = profile.attackState.coverShooterOptions.searchDistance;
        attackState.coverShooterOptions.minObstacleHeight = profile.attackState.coverShooterOptions.minObstacleHeight;
        attackState.coverShooterOptions.firstSightChance = (CoverShooterOptions.FirstSightChance)((int)profile.attackState.coverShooterOptions.firstSightChance);
        attackState.coverShooterOptions.coverBlownState = (CoverShooterOptions.CoverBlownState) ((int)profile.attackState.coverShooterOptions.coverBlownState);
        attackState.coverShooterOptions.attackEnemyCover = (CoverShooterOptions.AttackEnemyCover) ((int)profile.attackState.coverShooterOptions.attackEnemyCover);
        attackState.coverShooterOptions.coverAnimations.highCoverHeight = profile.attackState.coverShooterOptions.coverAnimations.highCoverHeight;
        attackState.coverShooterOptions.coverAnimations.highCoverAnimation = profile.attackState.coverShooterOptions.coverAnimations.highCoverAnimation;
        attackState.coverShooterOptions.coverAnimations.lowCoverHeight = profile.attackState.coverShooterOptions.coverAnimations.lowCoverHeight;
        attackState.coverShooterOptions.coverAnimations.lowCoverAnimation = profile.attackState.coverShooterOptions.coverAnimations.lowCoverAnimation;
        attackState.coverShooterOptions.coverAnimations.rotateToNormal = profile.attackState.coverShooterOptions.coverAnimations.rotateToNormal;
        attackState.coverShooterOptions.coverAnimations.useScripts = profile.attackState.coverShooterOptions.coverAnimations.useScripts;
        attackState.coverShooterOptions.coverAnimationTransition = profile.attackState.coverShooterOptions.coverAnimationTransition;
        attackState.distanceFromEnemy = profile.attackState.distanceFromEnemy;
        attackState.attackDistance = profile.attackState.attackDistance;
        attackState.layersCheckBeforeAttacking = profile.attackState.layersCheckBeforeAttacking;
        attackState.callOthers = profile.attackState.callOthers;
        attackState.callRadius = profile.attackState.callRadius;
        attackState.agentLayersToCall = profile.attackState.agentLayersToCall;
        attackState.callOthersTime = profile.attackState.callOthersTime;
        attackState.receiveCallFromOthers = profile.attackState.receiveCallFromOthers;
        attackState.timeToReturnAlert = profile.attackState.timeToReturnAlert;
        attackState.attackInIntervals = profile.attackState.attackInIntervals;
        attackState.attackInIntervalsTime = profile.attackState.attackInIntervalsTime;
        attackState.randomizeAttackIntervals = profile.attackState.randomizeAttackIntervals;
        attackState.randomizeAttackIntervalsBetween = profile.attackState.randomizeAttackIntervalsBetween;
        attackState.moveBackwards = profile.attackState.moveBackwards;
        attackState.moveBackwardsDist = profile.attackState.moveBackwardsDist;
        attackState.pushAgentsBackwards = profile.attackState.pushAgentsBackwards;
        attackState.moveBackwardsAttack = profile.attackState.moveBackwardsAttack;
        attackState.turnToTarget = profile.attackState.turnToTarget;
        attackState.turnSensitivity = profile.attackState.turnSensitivity;
        attackState.turnSpeed = profile.attackState.turnSpeed;
        attackState.useTurnAnimations = profile.attackState.useTurnAnimations;
        attackState.strafe = profile.attackState.strafe;
        attackState.strafeDirection = (AttackState.Strafing) ((int) profile.attackState.strafeDirection);
        attackState.strafeSpeed = profile.attackState.strafeSpeed;
        attackState.strafeTime = profile.attackState.strafeTime;
        attackState.strafeWaitTime = profile.attackState.strafeWaitTime;
        attackState.leftStrafeAnimName = profile.attackState.leftStrafeAnimName;
        attackState.rightStrafeAnimName = profile.attackState.rightStrafeAnimName;
        attackState.strafeT = profile.attackState.strafeT;
        attackState.moveSpeed = profile.attackState.moveSpeed;
        attackState.rotationSpeed = profile.attackState.rotationSpeed;
        attackState.moveBackwardsSpeed = profile.attackState.moveBackwardsSpeed;
        attackState.useAnimations = profile.attackState.useAnimations;
        attackState.idleAnimationName = profile.attackState.idleAnimationName;
        attackState.idleAnimationTransition = profile.attackState.idleAnimationTransition;
        attackState.moveForwardAnimationName = profile.attackState.moveForwardAnimationName;
        attackState.moveForwardAnimationTransition = profile.attackState.moveForwardAnimationTransition;
        attackState.moveBackwardsAnimationName = profile.attackState.moveBackwardsAnimationName;
        attackState.moveBackwardsAnimationTransition = profile.attackState.moveBackwardsAnimationTransition;
        attackState.moveBackwardsAttackAnimationName = profile.attackState.moveBackwardsAttackAnimationName;
        attackState.moveBackwardsAttackAnimationTransition = profile.attackState.moveBackwardsAttackAnimationTransition;
        attackState.attackAnimations = profile.attackState.attackAnimations;
        attackState.onAttackRotate = profile.attackState.onAttackRotate;
        attackState.attackDuration = profile.attackState.attackDuration;
        attackState.attackAnimationTransition = profile.attackState.attackAnimationTransition;
        attackState.useAudio = profile.attackState.useAudio;
        attackState.surprised.useSurprised = profile.attackState.surprised.useSurprised;
        attackState.surprised.surprisedDuration = profile.attackState.surprised.surprisedDuration;
        attackState.surprised.alwaysRotate = profile.attackState.surprised.alwaysRotate;
        attackState.surprised.useAnimations = profile.attackState.surprised.useAnimations;
        attackState.surprised.surprisedAnimationName = profile.attackState.surprised.surprisedAnimationName;
        attackState.surprised.surprisedAnimationTransition = profile.attackState.surprised.surprisedAnimationTransition;
        attackState.surprised.useAudio = profile.attackState.surprised.useAudio;

        distractions.alwaysUse = profile.distractions.alwaysUse;
        distractions.autoTurn = profile.distractions.autoTurn;
        distractions.turnSpeed = profile.distractions.turnSpeed;
        distractions.turnReactionTime = profile.distractions.turnReactionTime;
        distractions.turnAlertOnDistraction = profile.distractions.turnAlertOnDistraction;
        distractions.moveToDistractionLocation = profile.distractions.moveToDistractionLocation;
        distractions.checkDistractionPriorityLevel = profile.distractions.checkDistractionPriorityLevel;
        distractions.moveToDistractionReactTime = profile.distractions.moveToDistractionReactTime;
        distractions.checkingTime = profile.distractions.checkingTime;
        distractions.useTurnAnimations = profile.distractions.useTurnAnimations;
        distractions.distractionCheckAnimation = profile.distractions.distractionCheckAnimation;
        distractions.distractionCheckAnimationName = profile.distractions.distractionCheckAnimationName;
        distractions.distractionCheckTransition = profile.distractions.distractionCheckTransition;
        distractions.enableScript = profile.distractions.enableScript;
        distractions.playAudios = profile.distractions.playAudios;
        distractions.playDistractionSearchAudio = profile.distractions.playDistractionSearchAudio;
    
        hits.hitDuration = profile.hits.hitDuration;
        hits.cancelAttackIfHit = profile.hits.cancelAttackIfHit;
        hits.useAnimation = profile.hits.useAnimation;
        hits.animationName = profile.hits.animationName;
        hits.animationTransition = profile.hits.animationTransition;
        hits.animPlayGap = profile.hits.animPlayGap;
        hits.useAudios = profile.hits.useAudios;

        death.useAnimation = profile.death.useAnimation;
        death.animationName = profile.death.animationName;
        death.animationTransition = profile.death.animationTransition;
        death.useAudio = profile.death.useAudio;
        death.enableScript = profile.death.enableScript;
    }

    // validate properties on start
    void ValidateProperties()
    {
        if (attackState.attackDistance < 0.1f) attackState.attackDistance = 0.1f;
    }
}