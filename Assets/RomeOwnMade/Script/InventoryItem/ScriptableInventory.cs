using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventroy")]
public class ScriptableInventory : ScriptableObject
{
    public List<Item> itemList = new List<Item>();

}
