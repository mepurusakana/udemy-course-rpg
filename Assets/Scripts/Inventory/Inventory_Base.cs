using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour, ISaveable
{
    protected Player player;
    //public event Action OnInventoryChange;

    //public int maxInventorySize = 10;
    //public List<Inventory_Item> itemList = new List<Inventory_Item>();

    //[Header("ITEM DATA BASE")]
    //[SerializeField] protected ItemListDataSO itemDataBase;

    //protected virtual void Awake()
    //{
    //    player = GetComponent<Player>();
    //}

    public virtual void LoadData(GameData data)
    {

    }

    public virtual void SaveData(ref GameData data)
    {

    }
}
