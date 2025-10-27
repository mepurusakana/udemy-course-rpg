using UnityEngine;

/// <summary>
/// 場景內的出生點：
/// 1) 若 SceneGate 指定了 spawnId，優先落在相同 spawnId 的點；
/// 2) 否則若該場景有「上次離開位置」紀錄，回到紀錄位置；
/// 3) 否則用 isDefault=true 的點，若沒有就用第一個 SpawnPoint。
/// </summary>
[DisallowMultipleComponent]
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("此出生點的識別字串。要和 SceneGate.targetSpawnId 對上才會落在這裡。")]
    public string spawnId = "";

    [Tooltip("是否為此場景的預設出生點（當沒有指定 spawnId 且沒有位置紀錄時使用）")]
    public bool isDefault = false;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
#endif
}
