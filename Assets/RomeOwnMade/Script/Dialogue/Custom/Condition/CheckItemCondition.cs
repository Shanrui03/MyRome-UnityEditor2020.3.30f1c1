using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueQuests;

[CreateAssetMenu(fileName = "CheckItemCondition", menuName = "CustomCondition/CheckItemCondition")]
public class CheckItemCondition : CustomCondition
{
    public ScriptableInventory myBag;
    public Item QuestItem;
    public override bool IsMet(Actor player)
    {
        if(myBag.itemList.Find(item => item.itemNmae == QuestItem.itemNmae))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
