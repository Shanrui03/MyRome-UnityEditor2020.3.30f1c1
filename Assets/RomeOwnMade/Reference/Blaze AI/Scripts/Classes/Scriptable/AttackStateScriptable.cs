using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class AttackStateScriptable 
    {
        [Header("Cover Shooter Mode")]
        [Tooltip("Options for using cover shooter behaviour.")]
        public CoverShooterOptionsScriptable coverShooterOptions;

        [Header("General")]
        [Space(5)]
        [Tooltip("It's the safe distance between this NPC and the enemy. If cover shooter is enabled, it's the safe distance if there's no cover to hide.")]
        public float distanceFromEnemy = 10f;
        [Min(0.01f), Tooltip("The distance between the NPC and the enemy to deliver the actual attack. So for example: if this NPC is to punch the enemy then it needs to get pretty close. This can't be bigger or equal to [Distance From Enemy].")]
        public float attackDistance = 1f;
        [Tooltip("Select the layers to check before attacking to insure there's a clear view at the enemy. Example: adding the layers of the other agents to avoid friendly fire.")]
        public LayerMask layersCheckBeforeAttacking = Physics.AllLayers;

        [Header("Calling Others")]
        [Tooltip("When this agent is in attack state it'll call other within a radius.")]
        public bool callOthers = true;
        [Tooltip("When this agent is in attack state it will call others in this surrounding radius. Appears as a Blue Sphere in editor view")]
        public float callRadius = 10f;
        [Tooltip("The layers of the other Blaze AI agents to call to this position in attack state.")]
        public LayerMask agentLayersToCall = Physics.AllLayers;
        [Min(0), Tooltip("Will call others continuously when in attack state each time this value passes in seconds. It's always best to leave a gap of about 2 seconds to give the player a chance to kill the enemy before being compromised by others.")]
        public float callOthersTime = 2f;
        [Tooltip("If enabled, this agent will get called by others. If disabled, won't be called.")]
        public bool receiveCallFromOthers = true;
        
        [Space(5)]
        [Tooltip("The amount of time to transition to alert state from attack state when there is no longer any enemies.")]
        public float timeToReturnAlert = 2f;

        [Header("Interval Based Attacks")]
        [Space(5)]
        [Tooltip("If set to true, this NPC will not be added to the enemy scheduler, but rather will attack every certain amount of seconds. Best for ranged enemies where you want an NPC to shoot or throw arrows at the player every certain amount of time repeatedly. This will be forced on if cover shooter is enabled.")]
        public bool attackInIntervals = false;
        [Tooltip("The amount of time in seconds for this NPC to attack ~ ONLY READ IF [Attack In Intervals] IS TRUE.")]
        public float attackInIntervalsTime = 0f;
        [Tooltip("If set to true, the [Attack In Intervals Time] will be randomized after each attack ~ ONLY WORKS IF [Attack In Intervals] IS SET TO TRUE.")]
        public bool randomizeAttackIntervals = false;
        [Tooltip("Randomize the attack intervals between these two values (minimum of X is 0.5) ~ ONLY WORKS IF [Randomize Attack Intervals] IS SET TO TRUE.")]
        public Vector2 randomizeAttackIntervalsBetween;
        
        [Header("Moving Backwards")]
        [Space(5)]
        [Tooltip("If the enemy gets too near, this agent will backup.")]
        public bool moveBackwards;
        [Tooltip("Move backwards if distance is less than this.")]
        public float moveBackwardsDist = 3f;
        [Tooltip("Push other agents backwards when moving backwards.")]
        public bool pushAgentsBackwards;
        [Tooltip("When the NPC is backing up it can continue enabling the attack script, like a shooter where getting too close makes it backup while shooting you.")]
        public bool moveBackwardsAttack;

        [Header("Turning")]
        [Space(5)]
        [Tooltip("Turn to face the target when in attack state. Will be turned off if cover shooter mode enabled.")]
        public bool turnToTarget;
        [Range(-1f, 1f), Tooltip("The agent will turn to face the target when dot product between the two is equal to or less than this value.")]
        public float turnSensitivity;
        [Tooltip("The speed of turning.")]
        public float turnSpeed;
        [Tooltip("Will use the Alert State turn animations found in the waypoints class.")]
        public bool useTurnAnimations;

        [Header("Strafing")]
        [Space(5)]
        [Tooltip("Do you want the agent to strafe when waiting to attack. Does not support cover shooter (strafing will be disabled).")]
        public bool strafe;
        public enum Strafing {
            left,
            right,
            leftAndRight
        }
        public Strafing strafeDirection = Strafing.leftAndRight;
        [Tooltip("Only works if root motion is disabled.")]
        public float strafeSpeed = 3f;
        [Min(-1), Tooltip("The amount of time to strafe for. Value is randomized between the two inputs. For a constant time, set the two inputs to the same value. For infinity, set both inputs to -1.")]
        public Vector2 strafeTime = new Vector2(1f, 3f);
        [Min(0), Tooltip("The amount of time to wait before strafing again. Value is randomized between the two inputs. For a constant time, set the two inputs to the same value.")]
        public Vector2 strafeWaitTime = new Vector2(0f, 1f);
        [Tooltip("The name of the left strafe animation in the Animator. If empty, no animation will be played.")]
        public string leftStrafeAnimName;
        [Tooltip("The name of the right strafe animation in the Animator. If empty, no animation will be played.")]
        public string rightStrafeAnimName;
        [Tooltip("The transition time from current animation to strafe animation.")]
        public float strafeT = 0.25f;

        [Header("Movement")]
        [Space(5)]
        [Tooltip("Movement speed of agent in attack state.")]
        public float moveSpeed = 5f;
        [Tooltip("Rotation speed of agent in attack state.")]
        public float rotationSpeed = 3f;
        [Tooltip("Set the speed of moving backwards. When the targeted enemy is too close this NPC will back up.")]
        public float moveBackwardsSpeed = 2f;
        
        [Header("Animations")]
        [Space(5)]
        [Tooltip("Set whether you want to use animations when in attack state or not. If not, the animations used will be those of the alert state.")]
        public bool useAnimations;
        [Space(3)]
        [Tooltip("Animation name to play when this NPC is in idle-attack state (when waiting for it's turn to attack) will ignore if empty!")]
        public string idleAnimationName;
        [Tooltip("The amount of time to transition to the idle-attack animation.")]
        public float idleAnimationTransition = 0.25f;

        [Space(3)]
        [Tooltip("Move forward animation name when this NPC is in attack state and needs to move (when running after player, getting to attack position, running to cover, etc..) will ignore if empty!")]
        public string moveForwardAnimationName;
        [Tooltip("The amount of time to transition to this animation.")]
        public float moveForwardAnimationTransition = 0.25f;

        [Space(3)]
        [Tooltip("Move backwards animation name when this NPC is in attack state and needs to move (backing up when the player gets closer.")]
        public string moveBackwardsAnimationName;
        [Tooltip("The amount of time to transition to this animation.")]
        public float moveBackwardsAnimationTransition = 0.25f;

        [Space(3)]
        [Tooltip("The animation name to play when NPC is backing up and set to attack while moving backwards.")]
        public string moveBackwardsAttackAnimationName;
        [Tooltip("The transition time from the current animation and this one.")]
        public float moveBackwardsAttackAnimationTransition = 0.25f;

        [Header("Attacking")]
        [Space(5)]
        [Tooltip("Animation names to play on attack. One will be chosen at random to play in each attack. If only one is set. Then that one will always be chosen. If empty, will be ignored.")]
        public string[] attackAnimations;
        [Tooltip("Set the attack duration for each attack animation before backing up (attack finishes). This is automatically set according to attack animations array amount.")]
        public float[] attackDuration;
        [Tooltip("Transition time from current animation to the attack animation.")]
        public float attackAnimationTransition = 0.25f;
        [Tooltip("Check whether you always want the npc to keep looking at the enemy while attacking. Will always be enabled on cover shooter.")]
        public bool onAttackRotate;
        [Tooltip("The speed of rotating to player when attacking.")]
        public float attackRotateSpeed;

        [Header("Attack Audios")]
        [Tooltip("Play an audio when attacking.")]
        public bool useAudio;

        [Header("Emotions")]
        [Space(5)]
        [Tooltip("Surprised is when the NPC is in NORMAL STATE and sees an enemy (hostile tag) for the first time. You can play animations like being shocked, scared or something like a battlecry or calling for help.")]
        public SurprisedScriptable surprised;

        
        public void Validate()
        {
            if (!moveBackwards) moveBackwardsAttack = false;

            //minimum of x in randomizing attack intervals is 0.5f
            if (randomizeAttackIntervalsBetween.x < 0.5f) randomizeAttackIntervalsBetween.x = 0.5f;

            //disable randomizeAttackIntervals if attackInIntervals is false
            if (!attackInIntervals) randomizeAttackIntervals = false;

            if (distanceFromEnemy - attackDistance < 0.5f) {
                if (distanceFromEnemy > 1f) attackDistance = distanceFromEnemy - 1f;
            }

            //attack distance can't be bigger than distance from enemy
            if (distanceFromEnemy <= attackDistance) attackDistance = distanceFromEnemy - 1f;

            if (moveBackwardsDist == distanceFromEnemy) {
                moveBackwardsDist = distanceFromEnemy - 1f;
            }

            if (attackDuration.Length != attackAnimations.Length) SetDurationArray();
            if (coverShooterOptions.coverShooter) CoverShooterProperties();  
        }

        //set the attack duration array to the same number as attack animations array
        void SetDurationArray()
        {
            float[] arrCopy;
            arrCopy = new float[attackDuration.Length];

            attackDuration.CopyTo(arrCopy, 0);
            attackDuration = new float[attackAnimations.Length];

            for (var i=0; i<attackDuration.Length; i+=1) {
                if (i <= arrCopy.Length-1) {
                    attackDuration[i] = arrCopy[i];
                }
            }
        }

        //set the cover shooter properties
        void CoverShooterProperties()
        {
            attackInIntervals = true;
            if (coverShooterOptions.searchDistance >= distanceFromEnemy) coverShooterOptions.searchDistance = distanceFromEnemy - 2f;
        }
    }
}
