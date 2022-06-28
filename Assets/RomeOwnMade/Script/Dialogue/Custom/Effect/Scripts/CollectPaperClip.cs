using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;


[CreateAssetMenu(fileName = "CollectPaperClip", menuName = "CustomEffect/CollectPaperClip")]
public class CollectPaperClip : CustomEffect
{
    public Item questItem;
    public ScriptableInventory myBag;

    public override void DoEffect(Actor player)
    {
        if (!myBag.itemList.Contains(questItem))
        {
            myBag.itemList.Add(questItem);
        }
        else
        {
            questItem.itemNum += 1;
        }

        InventoryManager.RefreshItem();
    }
}
