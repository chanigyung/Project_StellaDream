using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerManager : MonoBehaviour
{
    [Header("몬스터 데이터")]
    public List<MonsterData> monsterDataList;

    private List<GameObject> spawnedMonsters = new();

    private void Start()
    {
        SpawnAllMonsters();
        PrintSpawnSummary();
    }

    private void SpawnAllMonsters()
    {
        MonsterSpawnPoint[] spawnPoints = FindObjectsOfType<MonsterSpawnPoint>();

        foreach (var point in spawnPoints)
        {
            //스폰 포인트의 태그에 따라 스폰할 몬스터 목록 가져오기
            var candidates = MonsterDataManager.Instance.GetMonsterDataByTag(point.monsterTag);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[스폰 실패] 태그 '{point.monsterTag}'에 해당하는 몬스터 없음");
                continue;
            }

            //몬스터 리스트에서 랜덤으로 하나 소환
            var selectedData = candidates[Random.Range(0, candidates.Count)];
            GameObject monsterObj = Instantiate(selectedData.monsterPrefab, point.GetSpawnPosition(), Quaternion.identity);

            var controller = monsterObj.GetComponent<MonsterController>();
            if (controller == null)
            {
                Debug.LogError("[스폰 실패] MonsterController 없음");
                continue;
            }

            controller.Initialize(new MonsterInstance(selectedData));
            spawnedMonsters.Add(monsterObj);
        }
    }

    private void PrintSpawnSummary()
    {
        Dictionary<string, int> tagCount = new();

        foreach (GameObject monster in spawnedMonsters)
        {
            var instance = monster.GetComponent<MonsterController>()?.instance as MonsterInstance;
            if (instance == null) continue;

            foreach (string tag in instance.data.tags)
            {
                if (!tagCount.ContainsKey(tag))
                    tagCount[tag] = 0;
                tagCount[tag]++;
            }
        }

        // Debug.Log("[몬스터 스폰 요약]");
        // foreach (var kvp in tagCount)
        // {
        //     Debug.Log($" - {kvp.Key}: {kvp.Value}마리");
        // }
    }
}
