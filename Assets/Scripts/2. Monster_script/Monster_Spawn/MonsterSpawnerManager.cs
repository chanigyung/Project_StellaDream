using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerManager : MonoBehaviour
{
    [Header("몬스터 데이터 (태그 + 프리팹 포함)")]
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
            // 1. 태그에 해당하는 MonsterData들 필터링
            List<MonsterData> candidates = monsterDataList.FindAll(data =>
                data.monsterTag != null &&
                data.monsterTag.Contains(point.monsterTag) &&
                data.monsterPrefab != null);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[스폰 실패] 태그 '{point.monsterTag}'에 해당하는 몬스터가 없거나 프리팹이 비어 있음");
                continue;
            }

            // 2. 랜덤으로 하나 선택
            MonsterData selectedData = candidates[Random.Range(0, candidates.Count)];

            // 3. 프리팹 생성 및 초기화
            GameObject monsterObj = Instantiate(
                selectedData.monsterPrefab,
                point.GetSpawnPosition(),
                Quaternion.identity
            );

            MonsterController controller = monsterObj.GetComponent<MonsterController>();
            if (controller == null)
            {
                Debug.LogError("[스폰 실패] 프리팹에 MonsterController가 없음");
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

            foreach (string tag in instance.data.monsterTag)
            {
                if (!tagCount.ContainsKey(tag))
                    tagCount[tag] = 0;
                tagCount[tag]++;
            }
        }

        Debug.Log("[몬스터 스폰 요약]");
        foreach (var kvp in tagCount)
        {
            Debug.Log($" - {kvp.Key}: {kvp.Value}마리");
        }
    }
}
