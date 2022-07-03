using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "MilkUICondition", menuName = "CustomCondition/MilkUICondition")]
public class MilkUICondition : CustomCondition
{
    public override bool IsMet(Actor player)
    {
        return GameLogicMaster.isMilkUIShown;
    }
}
