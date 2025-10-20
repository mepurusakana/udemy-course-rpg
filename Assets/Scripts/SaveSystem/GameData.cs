using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]

public class GameData
{
    public Vector3 savedCheckpoint;
    public int playerHealth;

    public string lastCheckpointSceneName;
    public string lastCheckpointId;
    public Vector3 lastCheckpointPosition;

    public GameData()
    {
        savedCheckpoint = Vector3.zero;
        playerHealth = 100;

        // 新增:初始化存檔點資料
        lastCheckpointSceneName = "";
        lastCheckpointId = "";
        lastCheckpointPosition = Vector3.zero;
    }
}
