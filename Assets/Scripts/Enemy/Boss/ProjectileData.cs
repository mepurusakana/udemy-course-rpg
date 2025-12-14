using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(menuName="NewBullet")]

public class ProjectileData : ScriptableObject
{
    public Vector3 P_Offset = Vector3.zero; //位置的偏移量
    public Vector3 R_Offset = Vector3.zero; //初始旋轉的偏移量
    public int Count = 1;                   //一次生成的子彈的數量
    public float LifeTime = 4f;             //子彈生命周期
    public float CdTime = 0.1f;             //子彈生成間隔時間
    public float Speed = 10;                //子彈移動速度
    public float Angle = 0;                 //相鄰子彈間的旋轉角度
    public float Distance = 0;              //相粌子彈間的距離
    public float CenterDis = 0;             //與發射點的距離
    public float SelfRotation = 0;          //每幀自轉角度增量
    public float RotationSpeed = 0f;        //每幀初始旋轉的偏移量增量

    public GameObject Prefab;               //子彈的預制體
}
