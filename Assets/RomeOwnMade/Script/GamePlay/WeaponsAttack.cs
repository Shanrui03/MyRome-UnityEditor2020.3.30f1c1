using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && PlayerMovement.isAttacking)
        {
            if(other.TryGetComponent<HealthSystemForDummies>(out HealthSystemForDummies healSystem))
            {
                healSystem.AddToCurrentHealth(-100);
            }

        }
    }


}
