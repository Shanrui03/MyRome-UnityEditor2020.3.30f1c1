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
    bool isClicked = false;
    // Start is called before the first frame update
    void Start()
    {
        isClicked = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ItemOnClicked()
    {
        isClicked = !isClicked;
        DropBtn.SetActive(isClicked);
        InventoryManager.UpdateItemInfo(slotItem.itemInfo);
    }

    public void DropOnClicked()
    {
        InventoryManager.DropItem(slotItem);
       
        if(slotItem.isDroped)
        {
            InventoryManager.UpdateItemInfo("");
            slotItem.isDroped = false;
            Destroy(this.gameObject);
        }
    }
}
