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

            if(other.TryGetComponent<Animator>(out Animator m_ani))
            {
                m_ani.SetTrigger("hit");
            }
        }
    }


}
