using UnityEngine;

namespace BlazeAISpace
{
    public class BlazeAIRandomEnemyDistance : MonoBehaviour
    {
        [Header("On start will randomize the DistanceFromEnemy property in BlazeAI")]
        public BlazeAI script;
        public float minDistance;
        public float maxDistance;

        // Start is called before the first frame update
        void Start()
        {
            script.attackState.distanceFromEnemy = Random.Range(minDistance, maxDistance);
        }

        //on script add, get the current BlazeAI component
        void OnValidate() 
        {
            script = GetComponent<BlazeAI>();
        }
    }
}

