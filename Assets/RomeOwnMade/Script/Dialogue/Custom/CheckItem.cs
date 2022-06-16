using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;


[CreateAssetMenu(fileName = "CheckItem", menuName = "CustomEffect/CheckItem")]
public class CheckItem : CustomEffect
{
    public Item QuestItem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoEffect(Actor player)
    {
        InventoryManager.DropItem(QuestItem);
    }
}
