using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/New Item")]
public class Item : ScriptableObject
{
    public string itemNmae;
    public Sprite itemImage;
    public int itemNum;
    [TextArea]
    public string itemInfo;
    public bool isTreasure;
    public bool isDroped = false;
}
