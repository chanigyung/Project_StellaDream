using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterDataManager : MonoBehaviour
{
    public static MonsterDataManager Instance { get; private set; }

    [Header("로드된 몬스터 데이터")]
    public List<MonsterData> monsterDataList = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllMonsterData();
    }

    public void LoadAllMonsterData()
    {
        if (monsterDataList.Count == 0)
        {
            monsterDataList = Resources.LoadAll<MonsterData>("MonsterData").ToList();
            Debug.Log($"[MonsterDataManager] {monsterDataList.Count}개 데이터 로드");
        }
    }

    public List<MonsterData> GetMonsterDataByTag(string tag)
    {
        return monsterDataList.Where(data =>
            data.tags != null &&
            data.tags.Contains(tag) &&
            data.monsterPrefab != null
        ).ToList();
    }
}
