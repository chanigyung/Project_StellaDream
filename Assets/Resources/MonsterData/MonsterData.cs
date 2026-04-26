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
    public List<MonsterActionType> actionTypes = new();

    [Header("Flying")]
    public bool isFlying = false; // 비행 가능 여부
    public float flyingMoveSpeedMultiplier = 1f; // 비행 이동 배율
    public float flyingWanderRadius = 2.5f; // 배회 반경
    public float flyingWanderInterval = 2f; // 배회 갱신 주기
    [Range(0f, 1f)] public float flyingIdleChance = 0.2f; // 정지 확률
    public float flyingTraceStopDistance = 0.3f; // 추적 정지 거리
    public Vector2 flyingHeightOffsetRange = new Vector2(-1f, 1f); // 배회 높이 범위

    public List<MonsterSkillList> skillList;

    public RuntimeAnimatorController deathAnimator;
    public GameObject dieEffectPrefab;

    public List<string> tags;
    public List<WeaponDropFilter> dropFilters;
}
