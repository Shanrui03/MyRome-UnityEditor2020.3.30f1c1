using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item slotItem;
    public Image slotImage;
    public Text slotNum;
    public GameObject DropBtn;
    public bool isClicked = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        isClicked = false;
    }

    public void ItemOnClicked()
    {
        OnClickEnable();
        isClicked = !isClicked;
        DropBtn.SetActive(isClicked);
        InventoryManager.UpdateItemInfo(slotItem.itemInfo);
    }

    public void DropOnClicked()
    {
        if (!slotItem.isTreasure)
        {
            InventoryManager.DropItem(slotItem);
            if (slotItem.isDroped)
            {
                InventoryManager.UpdateItemInfo("");
                slotItem.isDroped = false;
                Destroy(this.gameObject);
            }
        }
        else
        {
            InventoryManager.ShowTreasureNotice();
        }
    }
    public void OnClickEnable()
    {
        foreach (Transform child in this.gameObject.transform.parent.transform)
        {
            if(child.gameObject != this.gameObject)
            {
                child.gameObject.GetComponent<ItemSlot>().isClicked = false;
                child.gameObject.GetComponent<ItemSlot>().DropBtn.SetActive(false);
            }
               
        }

    }

}
