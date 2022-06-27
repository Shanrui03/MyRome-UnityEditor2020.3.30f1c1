using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "StatueTriggerEffect", menuName = "CustomEffect/StatueTriggerEffect")]
public class StatueTriggerEffect : CustomEffect
{
    public override void DoEffect(Actor player)
    {
        StatueTrigger.StatueTriggerOn();
        LadderTrigger.LadderTriggerOn();
    }
}
