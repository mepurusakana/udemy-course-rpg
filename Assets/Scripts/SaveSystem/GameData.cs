using System;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public SerializableVector3 savedCheckpoint;
    public int playerHealth;
    public string lastCheckpointSceneName;
    public string lastCheckpointId;
    public SerializableVector3 lastCheckpointPosition;

    public GameData()
    {
        savedCheckpoint = SerializableVector3.Zero;  //  
        playerHealth = 100;
        lastCheckpointSceneName = "";
        lastCheckpointId = "";
        lastCheckpointPosition = SerializableVector3.Zero;
    }
}