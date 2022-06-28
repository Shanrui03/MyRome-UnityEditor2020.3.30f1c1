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
    public GameObject TreasureNotice;
    public float fadeSpeed = 1f;
    public float waitTime = 1f;



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

    public static void DropItem(Item itemDroping)
    {
        if(itemDroping.itemNum == 1)
        {
            instance.myBag.itemList.Remove(itemDroping);
            itemDroping.isDroped = true;
            instance.itemInFormation.text = "";
        }
        else
        {
            itemDroping.itemNum -= 1;
        }
        RefreshItem();
    }

    // Update is called once per frame
    void Update()
    {
        if (instance.TreasureNotice.activeSelf)
        {
            if (waitTime >= 0)
            {
                waitTime = waitTime - Time.deltaTime;
            }
            if (instance.TreasureNotice.GetComponent<Text>().color.a != 0 && waitTime <= 0)
            {
                instance.TreasureNotice.GetComponent<Text>().color = new Color(
                    instance.TreasureNotice.GetComponent<Text>().color.r,
                    instance.TreasureNotice.GetComponent<Text>().color.g,
                    instance.TreasureNotice.GetComponent<Text>().color.b,
                    Mathf.Lerp(instance.TreasureNotice.GetComponent<Text>().color.a
                    , 0, fadeSpeed * Time.deltaTime));
                if (Mathf.Abs(0 - instance.TreasureNotice.GetComponent<Text>().color.a) <= 0.01f)
                {
                    instance.TreasureNotice.GetComponent<Text>().color = new Color(
                    instance.TreasureNotice.GetComponent<Text>().color.r,
                    instance.TreasureNotice.GetComponent<Text>().color.g,
                    instance.TreasureNotice.GetComponent<Text>().color.b,
                    0f);
                    instance.TreasureNotice.SetActive(false);
                }

            }
        }
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
        instance.itemInFormation.text = "";
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

    public static void ShowTreasureNotice()
    {
        instance.TreasureNotice.SetActive(true);
        instance.TreasureNotice.GetComponent<Text>().color = new Color(
          instance.TreasureNotice.GetComponent<Text>().color.r,
          instance.TreasureNotice.GetComponent<Text>().color.g,
          instance.TreasureNotice.GetComponent<Text>().color.b,
          1f);
        instance.waitTime = 1f;
    }

    
}
