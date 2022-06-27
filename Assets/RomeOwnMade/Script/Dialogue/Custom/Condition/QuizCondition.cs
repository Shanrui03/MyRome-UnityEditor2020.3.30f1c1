using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "QuizCondition", menuName = "CustomCondition/QuizCondition")]
public class QuizCondition : CustomCondition
{
    public override bool IsMet(Actor player)
    {
        if(GameLogicMaster.lastAccuracy >= 50f && GameLogicMaster.lastAnserint == 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
