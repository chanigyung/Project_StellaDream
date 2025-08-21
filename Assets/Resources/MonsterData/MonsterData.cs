using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Monster/MonsterData")]
public class MonsterData : ScriptableObject
{
    public GameObject monsterPrefab;

    public string monsterName;
    public float maxHealth = 100f;
    public float moveSpeed = 1f;
    public float jumpPower = 0f;

    public bool knockbackImmune = false;
    public float knockbackResistance = 0f;

     public List<MonsterSkillList> skillList;

    public RuntimeAnimatorController deathAnimator;
    public GameObject dieEffectPrefab;

    public List<string> monsterTag;
    public List<WeaponDropFilter> dropFilters;
}