using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !PlayerMovement.isDefensing)
        {
            if (other.TryGetComponent<HealthSystemForDummies>(out HealthSystemForDummies healSystem))
            {
                healSystem.AddToCurrentHealth(-200);
            }
        }
    }
}
