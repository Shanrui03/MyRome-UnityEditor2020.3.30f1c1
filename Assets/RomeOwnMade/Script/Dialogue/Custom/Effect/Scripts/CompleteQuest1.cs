using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "CompeleteQuest1", menuName = "CustomEffect/CompeleteQuest1")]
public class CompleteQuest1 : CustomEffect
{
    public QuestData FinalQuest;
    public override void DoEffect(Actor player)
    {
        FinalQuest.desc = "Please Compelete 3 Quests!\r\n2.Answer the consul's questions.";
    }
}
