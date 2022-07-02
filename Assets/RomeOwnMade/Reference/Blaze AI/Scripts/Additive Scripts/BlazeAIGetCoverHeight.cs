using UnityEngine;

namespace BlazeAISpace
{
    [ExecuteInEditMode]
    public class BlazeAIGetCoverHeight : MonoBehaviour
    {
        Collider collider;

        void OnEnable()
        {
            collider = GetComponent<Collider>();
            if (collider != null) {
                print($"The object with name {gameObject.name} has a height of: {collider.bounds.size.y}");
            }else{
                print($"The object with name {gameObject.name} has no collider.");
            }
        }
    }
}
