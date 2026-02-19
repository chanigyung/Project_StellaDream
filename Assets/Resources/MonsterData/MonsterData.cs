using UnityEngine;
using System.Collections.Generic;

public enum MonsterGrade { Normal, Elite, Rare, Boss }

[CreateAssetMenu(menuName = "Monster/MonsterData")]
public class MonsterData : ScriptableObject
{
    public GameObject monsterPrefab;
    public RuntimeAnimatorController animatorController;

    public string monsterName;
    public MonsterGrade grade;
    public float maxHealth = 100f;
    public float moveSpeed = 1f;
    public float jumpPower = 0f;

    public bool knockbackImmune = false;
    public float knockbackResistance = 0f;

    [Header("패턴")]
    public List<ActionType> actionTypes = new();

    public List<MonsterSkillList> skillList;

    public RuntimeAnimatorController deathAnimator;
    public GameObject dieEffectPrefab;

    public List<string> tags;
    public List<WeaponDropFilter> dropFilters;
}