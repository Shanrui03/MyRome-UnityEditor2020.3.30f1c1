using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "FightEnemyCondition", menuName = "CustomCondition/FightEnemyCondition")]
public class FightEnemyCondition : CustomCondition
{
    public override bool IsMet(Actor player)
    {
        return EnemyController.enemyIsDead;
    }
}
