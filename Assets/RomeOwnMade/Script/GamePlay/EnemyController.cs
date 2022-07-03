using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static bool enemyIsDead = false;
    public Collider enemySwordCollider;
    public Animator enemyAnimator;
    public AudioSource weaponAudio;
    private float enemyHp;

    private void Awake()
    {
        enemyAnimator = this.gameObject.GetComponent<Animator>();
        enemyAnimator.SetBool("death", false);
        enemyIsDead = false;
    }

    public void StartAIorNot(bool isStart)
    {
        if(this.gameObject.TryGetComponent<BlazeAI>(out BlazeAI m_AI))
        {
            if(isStart)
                m_AI.enabled = true;
            else
                m_AI.enabled = false;
        }
    }
    public void WeaponEnable()
    {
        enemySwordCollider.enabled = true;
        weaponAudio.PlayOneShot(weaponAudio.clip);
    }
    public void WeaponDisable()
    {
        enemySwordCollider.enabled = false;
    }
    public void KillSoldier()
    {
        enemyAnimator.SetBool("death", true);
        if (this.gameObject.TryGetComponent<BlazeAI>(out BlazeAI m_AI))
        {
            m_AI.enabled = false;
        }
    }
    public void CheckEnemyState()
    {
        enemyIsDead = true;
    }
}
