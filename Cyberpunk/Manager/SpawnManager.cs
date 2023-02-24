using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SpawnData
{
    public enum eSpawnType
    {
        None = 0,
        Enemy = 1,
        Robot = 2,
        Item = 3,
        Obstacle = 4,
    }

    [Header("[Spawn Data]")]
    public eSpawnType SpawnType = eSpawnType.None;
    public List<GameObject> SpawnPrefab = default;
    [Range(1, 100)] public int SpawnCount = 1;
}

public class SpawnManager : MonoSingleton<SpawnManager>
{
    [Header("[Spawn - Data]")]
    public List<SpawnData> SpawnDatas = new List<SpawnData>();

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    public GameObject GetSpawnPrefab(SpawnData.eSpawnType type, int index)
    {
        return SpawnDatas.Find(obj => obj.SpawnType == type).SpawnPrefab[index];
    }

    /// <summary>
    /// 단일 스폰
    /// </summary>
    /// <param name="spawnObject"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    public void Spawn(GameObject spawnObject, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        var obj = Instantiate(spawnObject, spawnPosition, spawnRotation);
    }

    /// <summary>
    /// 다수 스폰
    /// </summary>
    /// <param name="spawnObject"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    /// <param name="spawnCount"></param>
    /// <param name="callback"></param>
    public void Spawn(GameObject spawnObject, Vector3 spawnPosition, Quaternion spawnRotation, int spawnCount, UnityAction callback = null)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            var obj = Instantiate(spawnObject, spawnPosition, spawnRotation);
        }
        callback?.Invoke();
    }

    /// <summary>
    /// 다수 스폰 (딜레이)
    /// </summary>
    /// <param name="spawnObject"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    /// <param name="spawnCount"></param>
    /// <param name="duration"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator SpawnDelay(GameObject spawnObject, Vector3 spawnPosition, Quaternion spawnRotation, int spawnCount, float duration, UnityAction callback = null)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            yield return new WaitForSeconds(duration);
            var obj = Instantiate(spawnObject, spawnPosition, spawnRotation);
        }
        callback?.Invoke();
    }
}
