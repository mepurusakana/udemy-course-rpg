using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_Player : Inventory_Base
{
    //public event Action<int> OnQuickSlotUsed;
    //public List<Inventory_EquipmentSlot> equipList;

    public override void SaveData(ref GameData data)
    {
        //data.equipedItems.Clear();

        //foreach (var item in itemList)
        //{
        //    if (item != null) && item.itemData != null){
        //        string saveId = item.itemData.saveID;

        //        if (data.inventory.ContainsKey(saveId) == false)
        //            data.inventory[saveId] = 0;

        //        data.inventory[saveId] += item.stackSize;
        //    }
        //}

        //foreach(var slot in equipList)
        {
            //if (slot.HasItem())
            //data.equipedItems[slot.equipmentItem.itemData.saveId] = slot.slotType;
        }
    }

    public override void LoadData(GameData data)
    {
        //foreach (var entry in data.inventory)
        //{
        //    string saveId = entry.Key;
        //    int stackSize = entry.Value;

        //    //ItemDataSO itemData = itemDataBase.GetItemData(saveId);

        //    if (ItemData == null)
        //    {
        //        Debug.LogWarning("Item not found:" + saveId);
        //        continue;
        //    }

        //    Inventory_Item itemToLoad = new Inventory_Item(ItemData);
        //    //itemToLoad.stackSize=stackSize;

        //    itemList.Add(itemToLoad);
        //}

        //foreach (var entry in data.equipedItems)
        //{
        //    string savdId = entry.Key;
        //    ItemType loadedSlotType=entry.Value;

        //    ItemDataSO itemData=itemDataBase.GetItemData(savdId);
        //    Inventory_Item inventory_Item = new Inventory_Item(itemData);

        //    var slot = equipList.Find(slot => slot.slotType == loadedSlotType && slot.HasItem() == false);

        //    slot.equipedItem = itemToLoad;
        //    slot.equipedItem.AddModifiers(player.stats);
        //    slot.equipedItem.AddItemEffect(player); 
        //}

        //TriggerUpdateUI();
    }
}
