using UnityEngine;

public class TestMonsterSpawner : MonoBehaviour
{
    [Header("스폰 대상")]
    public GameObject monsterPrefab;     // MonsterController 포함된 프리팹
    public MonsterData monsterData;      // 몬스터 데이터

    [Header("스폰 위치")]
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    void Start()
    {
        SpawnMonster();
    }

    public void SpawnMonster()
    {
        if (monsterPrefab == null || monsterData == null)
        {
            Debug.LogError("몬스터 프리팹 또는 데이터가 설정되지 않았습니다.");
            return;
        }

        GameObject monster = Instantiate(monsterPrefab, transform.position, Quaternion.identity);

        MonsterController controller = monster.GetComponent<MonsterController>();
        if (controller == null)
        {
            Debug.LogError("MonsterController 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        MonsterInstance instance = new MonsterInstance(monsterData);
        controller.Initialize(instance);

        // Debug.Log($"몬스터 {monsterData.monsterName} 스폰됨 (체력 {instance.CurrentHealth})");
    }
}
