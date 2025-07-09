using System.Collections.Generic;
using UnityEngine;

public static class MaterialDropManager
{
    // 태그별 재료 매핑 (나중에 ScriptableObject로 바꿔도 좋음)
    private static Dictionary<string, List<ItemData>> tagToMaterials = new Dictionary<string, List<ItemData>>()
    {
        // { "slime", new List<ItemData> { ItemDB.Get("SlimeGel"), ItemDB.Get("StickyResidue") } },
        // { "fire",  new List<ItemData> { ItemDB.Get("FlameEssence") } },
        // { "default", new List<ItemData> { ItemDB.Get("ScrapMetal") } }
        {"test", new List<ItemData>() { ItemDB.Get("test") }}
    };

    //태그에 해당하는 재료 목록 가져오기, 태그가 없으면 default
    public static List<ItemData> GetMaterialsForTag(string tag)
    {
        if (!tagToMaterials.ContainsKey(tag))
            tag = "default";

        return tagToMaterials[tag];
    }

    //GetMaterialsForTag에서 생성한 재료 아이템 목록을 기반으로 랜덤 드롭 시켜주기
    public static List<ItemData> GenerateRandomMaterialDrops(string tag)
    {
        var pool = GetMaterialsForTag(tag);
        var count = Random.Range(1, 4); // 1~3개
        var result = new List<ItemData>();

        for (int i = 0; i < count; i++)
        {
            var selected = pool[Random.Range(0, pool.Count)];
            result.Add(selected);
        }

        return result;
    }
}