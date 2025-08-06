using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]

public class GameData
{
    public List<Inventory_Item> itemList;
    public SerializableDictionary<string, int> inventory;
    public SerializableDictionary<string, int> storageItems;
    public SerializableDictionary<string, int> storageMaterials;

    //public SerializableDictionary<string, ItemType> equipmentItems;

    public SerializableDictionary<string, string> skillTreeUI;

    public GameData()
    {
        inventory = new SerializableDictionary<string, int>();
        storageItems = new SerializableDictionary<string, int>();
        storageMaterials = new SerializableDictionary<string, int>();

        //equipedItems=new SerializableDictionary<string, ItemType>();
    }
}
