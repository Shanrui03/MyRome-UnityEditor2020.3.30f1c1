using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "MilkCondition",menuName = "CustomCondition/MilkCondition")]
public class MilkCondition : CustomCondition
{
    public override bool IsMet(Actor player)
    {
        if (GameLogicMaster.finalmilkScore >= 20)
            return true;
        else
            return false;
        
    }
}
