using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAniCtrl : MonoBehaviour
{
    public Animator enemyAnimator;
    public AudioSource enemyAudio;
    public void OnHitEnter()
    {
        enemyAnimator.SetLayerWeight(enemyAnimator.GetLayerIndex("Hit"), 0.0f);
    }
    public void OnHitExit()
    {
        enemyAnimator.SetLayerWeight(enemyAnimator.GetLayerIndex("Hit"), 1.0f);
        enemyAudio.PlayOneShot(enemyAudio.clip);
    }
}
