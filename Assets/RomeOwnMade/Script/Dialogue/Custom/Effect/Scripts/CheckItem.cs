using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;


[CreateAssetMenu(fileName = "CheckItem", menuName = "CustomEffect/CheckItem")]
public class CheckItem : CustomEffect
{
    public Item QuestItem;
    public override void DoEffect(Actor player)
    {
        InventoryManager.DropItem(QuestItem);
    }
}
