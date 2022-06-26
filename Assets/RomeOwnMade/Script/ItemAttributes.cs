using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAttributes : MonoBehaviour
{
    public Item itemAttri;
    public ScriptableInventory inventoryAttri;

    private void Start()
    {
        itemAttri.itemNum = 1;
        inventoryAttri.itemList.Clear();
    }
    private void Update()
    {

    }

    public void ItemCollected()
    {
        if (!inventoryAttri.itemList.Contains(itemAttri))
        {
            inventoryAttri.itemList.Add(itemAttri);
        }
        else
        {
            itemAttri.itemNum += 1;
        }

        InventoryManager.RefreshItem();
        
    }
}
