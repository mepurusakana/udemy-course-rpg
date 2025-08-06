using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Storage : Inventory_Base
{
    public override void SaveData(ref GameData data)
    {
        base.SaveData(ref data);

        data.storageItems.Clear();

        //foreach (var item in itemList)
        //{
        //    if (item != null) && item.itemData != null){
        //        string saveId = item.itemData.saveID;

        //        if (data.storageItems.ContainsKey(saveId) == false)
        //            data.storageItems[saveId] = 0;

        //        data.stoargeItems[saveId] += item.stackSize;
        //    }
        //}

        data.storageMaterials.Clear();

        //foreach (var item in materialStash)
        //{
        //    if (item != null) && item.itemData != null){
        //        string saveId = item.itemData.saveID;

        //        if (data.storageMaterials.ContainsKey(saveId) == false)
        //            data.storageMaterials[saveId] = 0;

        //        data.stoargeMaterials[saveId] += item.stackSize;
        //    }
        //}
    }

    public override void LoadData(GameData data)
    {
        //itemList.Clear();
        //materialStash.Clear();

        //foreach (var entry in data.storageItems)
        //{
        //    string saveId = entry.Key;
        //    int stackSize = entry.Value;

        //    //ItemDataSO itemData = itemDataBase.GetItemData(saveId);

        //    if (ItemData == null)
        //    {
        //        Debug.LogWarning("Item not found:" + saveId);
        //        continue;
        //    }

        //    for (int i = 0; i < stackSize; i++)
        //    {
        //        Inventory_Item itemToLoad = new Inventory_Item(ItemData);
        //        AddItem(itemToLoad);
        //    }
        //}

        //foreach (var entry in data.storageMaterials)
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
        //    for (int i = 0; i < stackSize; i++)
        //    {
        //    materialStash.Add(itemToLoad);
        //    }

        //}

        //TriggerUpdateUI();
    }
}
