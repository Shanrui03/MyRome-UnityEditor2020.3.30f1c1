using UnityEngine;

namespace BlazeAISpace
{
    [System.Serializable]
    public class CoverAnimations
    {
        [Tooltip("If height of obstacle (collider) is this or more, then it's a high cover and the high cover animation will be used. Obstacle height is measured using collider.bounds.y. Use the [GetCoverHeight] script to print cover height in console.")]
        public float highCoverHeight = 2f;
        [Tooltip("The animation name to play when in high cover.")]
        public string highCoverAnimation;
        
        [Space(2)]
        
        [Tooltip("If height of obstacle (collider) is this or less, then it's a low cover and the low cover animation will be used. Obstacle height is measured using collider.bounds.y. Use the [GetCoverHeight] script to print cover height in console.")]
        public float lowCoverHeight = 1f;
        [Tooltip("The animation name to play when in low cover.")]
        public string lowCoverAnimation;
        
        [Space(2)]
        [Tooltip("If set to true, the npc will rotate to match the cover normal. Meaning the back of the character will be on the cover. If set to false, will take cover in the same current rotation.")]
        public bool rotateToNormal = true;

        [Space(5)]
        [Tooltip("Enable scripts for high and low cover when taking cover. This is useful to change character controller height according to the cover taken. Scripts will disable when out of cover and this process will be ignored if scripts field is empty.")]
        public bool useScripts = false;
        [Tooltip("Script to enable when in high cover. Will be disabled when out of cover.")]
        public MonoBehaviour highCoverScript;
        [Tooltip("Script to enable when in low cover. Will be disabled when out of cover.")]
        public MonoBehaviour lowCoverScript;

        //enable the cover script
        public void EnableCoverScript(string type) 
        {
            if (!useScripts) return;

            if (type == "high") {
                if (highCoverScript != null) {
                    lowCoverScript.enabled = false;
                    highCoverScript.enabled = true;
                }
            }

            if (type == "low") {
                if (lowCoverScript != null) {
                    highCoverScript.enabled = false;
                    lowCoverScript.enabled = true;
                }
            }
        }

        //disable both cover scripts
        public void DisableCoverScripts()
        {
            if (lowCoverScript != null) lowCoverScript.enabled = false;
            if (highCoverScript != null) highCoverScript.enabled = false;
        }
    }
}
