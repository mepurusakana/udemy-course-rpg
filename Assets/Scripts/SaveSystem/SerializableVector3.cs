using System;
using UnityEngine;

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    // 從 Vector3 創建
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    // 直接指定 x, y, z
    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // 轉換回 Vector3
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    // 靜態屬性：返回 (0, 0, 0)
    public static SerializableVector3 Zero => new SerializableVector3(0, 0, 0);
}