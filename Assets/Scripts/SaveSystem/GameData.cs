using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public SerializableVector3 savedCheckpoint;
    public int playerHealth;
    public string lastCheckpointSceneName;
    public string lastCheckpointId;
    public SerializableVector3 lastCheckpointPosition;

    public HashSet<string> deadEnemyIds = new HashSet<string>();
    public HashSet<string> finishedDialogueFlowIds = new HashSet<string>();

    

    public GameData()
    {
        finishedDialogueFlowIds = new HashSet<string>();
        savedCheckpoint = SerializableVector3.Zero;  //  
        playerHealth = 100;
        lastCheckpointSceneName = "";
        lastCheckpointId = "";
        lastCheckpointPosition = SerializableVector3.Zero;
    }
}