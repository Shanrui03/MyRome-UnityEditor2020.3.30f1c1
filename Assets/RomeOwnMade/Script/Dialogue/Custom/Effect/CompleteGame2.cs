using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;


[CreateAssetMenu(fileName = "CompleteGame2", menuName = "CustomEffect/CompleteGame2")]
public class CompleteGame2 : CustomEffect
{
    public QuestData FinalQuest;
    public override void DoEffect(Actor player)
    {
        FinalQuest.desc = "Please Compelete 3 Quests!\r\n3.Win the battle in the arena.";
    }
}
