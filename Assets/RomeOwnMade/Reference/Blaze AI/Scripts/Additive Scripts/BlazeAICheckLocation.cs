using UnityEngine;

public class BlazeAICheckLocation : MonoBehaviour
{
    [Tooltip("The radius to find and call agents"), Min(0f)]
    public float radius;
    [Tooltip("The amount of seconds to pass before the agents start running to location"), Min(0f)]
    public float secondsToRun = 1f;

    // run the method
    public void Trigger()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        
        foreach (var agent in hitColliders) {
            BlazeAI script = agent.GetComponent<BlazeAI>();
            if (script) script.CallAgentToLocation(transform.position, secondsToRun);
        }
    }

    //show radius
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
