using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeaponDropFilter
{
    public Rarity rarity;
    public List<string> requiredTags; // 몬스터가 가진 태그 중 하나 이상 일치
}
