using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]

public class GameData
{
    public Vector3 savedCheckpoint;
    public int playerHealth;


    public GameData()
    {
        savedCheckpoint = Vector3.zero;
        playerHealth = 100;
    }
}
