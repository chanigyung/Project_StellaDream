using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterDropHandler : MonoBehaviour
{
    public List<WeaponDropFilter> dropFilters;
    
    public GameObject dropItemPrefab;

    public void DropItem()
    {
        List<WeaponData> allWeapons = Resources.LoadAll<WeaponData>("WeaponData").ToList(); // Resources에 무기 저장되어야 함
        List<WeaponData> filtered = new List<WeaponData>();

        foreach (var filter in dropFilters) //희귀도 및 태그 일치하는 무기 분류해 filtered에 저장해주는 로직
        {
            var matched = allWeapons.Where(w =>
                w.rarity == filter.rarity &&
                filter.requiredTags.Any(tag => w.tags.Contains(tag))
            );
            filtered.AddRange(matched);
        }

        if (filtered.Count > 0)
        {
            WeaponData selected = filtered[Random.Range(0, filtered.Count)];
            WeaponInstance instance = new WeaponInstance(selected, true);
            
            GameObject dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            dropObj.GetComponent<ItemDrop>().Initialize(instance);
        }
        // else if(filtered.Count == 0)
        // {
        //     Debug.LogWarning("필터에 해당하는 무기가 없습니다.");
        //     return;
        // }
    }
}
