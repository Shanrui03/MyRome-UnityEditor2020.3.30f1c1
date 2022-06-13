using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    static InventoryManager instance;

    public ScriptableInventory myBag;
    public GameObject slotGrid;
    public ItemSlot slotPrefab;
    public Text itemInFormation;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        instance.itemInFormation.text = "";
    }

    public static void UpdateItemInfo(string itemDescription)
    {
        instance.itemInFormation.text = itemDescription;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CreateNewItem(Item item)
    {
        ItemSlot newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);
        newItem.gameObject.transform.SetParent(instance.slotGrid.transform);
        newItem.slotItem = item;
        newItem.slotImage.sprite = item.itemImage;
        newItem.slotNum.text = item.itemNum.ToString();
    }

    public static void RefreshItem()
    {
        for(int i = 0;i < instance.slotGrid.transform.childCount; i++)
        {
            if (instance.slotGrid.transform.childCount == 0)
                break;
            Destroy(instance.slotGrid.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < instance.myBag.itemList.Count; i++)
        {
            CreateNewItem(instance.myBag.itemList[i]);
        }
    }
}
