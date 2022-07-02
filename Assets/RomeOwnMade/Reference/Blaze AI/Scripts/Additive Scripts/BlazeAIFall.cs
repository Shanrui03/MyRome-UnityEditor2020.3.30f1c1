using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BlazeAISpace
{
    [RequireComponent(typeof(BlazeAI))]
    public class BlazeAIFall : MonoBehaviour
    {
        [Tooltip("Enable/disable the fall system.")]
        public bool enableFall = true;

        [Space(5)]
        [Tooltip("Add a gameobject part of the agent that you want to start the measure to ground. Can be foot, chest, etc... If empty will use transform.position.")]
        public Transform centerOfMeasure;
        [Tooltip("The normal ground distance between the transform.position of the agent and the ground it walks on.")]
        public float normalGroundDistance = 0.2f;

        [Space(5)]
        [Tooltip("The name of the fall animation name.")]
        public string fallAnimationName;
        [Tooltip("The transition/blend time from current animation to the falling animation.")]
        public float fallBlend = 0.25f;
        [Tooltip("Play the fall animation if distance from ground equals or greater than this.")]
        public float fallIfDistanceFromGround;

        [Space(5)]
        [Tooltip("Enabling landing will also play a landing animation when reaching ground.")]
        public bool enableFallLanding = false;
        [Tooltip("The animation name to play during landing.")]
        public string landingAnimName;
        [Tooltip("The transition/blend time from falling animation to the landing animation.")]
        public float landingBlend = 0.25f;
        [Tooltip("Amount of time to be in landing state and playing the landing animation.")]
        public float landingStateTime = 1f;

        [Space(5)]
        [Tooltip("Enable Blaze AI to trigger Death when falling from certain distances.")]
        public bool enableDeathFromFall;
        [Tooltip("Blaze AI will trigger Death if agent is falling from a distance that equals or is bigger than this.")]
        public float dieIfDistance;


        [Header("DEBUG"), Space(5)]
        [Tooltip("Enabling debug will show you the raycast projection from the center of measure property to below (in the scene view shown as blue line) and print the distance if hit a ground layer.")]
        public bool enableDebug;


        BlazeAI blaze;
        NavMeshAgent agent;
        CharacterController controller;

        bool fell = false;
        bool landingCoroutineFired = false;

        float pastDist = 0f;
        float currentDist = 0f;

        bool shouldDie = false;
        bool shouldEnableAgent = false;

        void Start()
        {
            blaze = GetComponent<BlazeAI>();
            controller = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (enableFall) DetectGroundDistance();
            if (fell) Gravity();
        }

        // detect the ground distance below
        void DetectGroundDistance()
        {
            RaycastHit hit;

            if (Physics.Raycast(centerOfMeasure.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, blaze.groundLayers)) {
                float dist = (centerOfMeasure.position - hit.point).sqrMagnitude;

                if (fell) {
                    // landing
                    if (dist <= normalGroundDistance * normalGroundDistance) {
                        if (shouldDie) FallDeath();
                        else Landing();
                    }
                }else{
                    // falling
                    if (enableDeathFromFall && dist >= dieIfDistance * dieIfDistance) shouldDie = true;
                    if (dist >= fallIfDistanceFromGround * fallIfDistanceFromGround) Fall();
                }
            }
        }

        // trigger the falling
        void Fall()
        {
            blaze.enabled = false;
            
            if (agent.enabled) {
                agent.enabled = false;
                shouldEnableAgent = true;
            }

            blaze.animationManager.PlayAnimationState(fallAnimationName, fallBlend, true);
            fell = true;
        }

        // Trigger the landing
        void Landing()
        {
            fell = false;

            if (enableFallLanding) {
                blaze.animationManager.PlayAnimationState(landingAnimName, landingBlend, true);
                if (!landingCoroutineFired) StartCoroutine(LandingState());
            }else{
                blaze.enabled = true;
            }
        }

        // re-enable blaze after landing state time has finished
        IEnumerator LandingState()
        {
            landingCoroutineFired = true;
            yield return new WaitForSeconds(landingStateTime);
            
            landingCoroutineFired = false;
            blaze.enabled = true;
            
            if (shouldEnableAgent) {
                agent.enabled = true;
                shouldEnableAgent = false;
            }
        }

        // on value changes
        void OnValidate()
        {
            if (centerOfMeasure == null) centerOfMeasure = transform;
            if (!blaze) blaze = GetComponent<BlazeAI>();
        }

        // show the debug
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            if (enableDebug) {
                
                if (centerOfMeasure == null) centerOfMeasure = transform;
                RaycastHit hit;
                Gizmos.color = Color.blue;

                if (Physics.Raycast(centerOfMeasure.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, blaze.groundLayers)) {    
                    Gizmos.DrawLine(centerOfMeasure.position, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.7f);
                    
                    float dist = Vector3.Distance(centerOfMeasure.position, hit.point);
                    currentDist = dist;
                    if (currentDist != pastDist) print("The distance between agent and ground: " + currentDist);
                    pastDist = dist;
                }else{
                    Debug.LogWarning("No ground layer detected below. Ground layers are set in the BlazeAI component.");
                }
            }
        }

        // gravity during fall
        void Gravity()
        {
            bool isGrounded = controller.isGrounded;
            float verticalVelocity = 0f;

            if (isGrounded) {
                verticalVelocity -= 0f;
            }else{
                verticalVelocity -= Time.deltaTime * blaze.gravityStrength;
            }

            var moveVector = new Vector3(0, verticalVelocity, 0);
            controller.Move(moveVector);
        }

        // call death to blaze and disable this component
        void FallDeath()
        {
            blaze.enabled = true;
            blaze.Death();
            enabled = false;
            shouldDie = false;
            fell = false;
        }
    }
}
