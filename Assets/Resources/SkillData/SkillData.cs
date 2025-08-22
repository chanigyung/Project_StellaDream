using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Melee, Projectile }
public enum SkillActivationType { OnPress, OnRelease, WhileHeld }

public abstract class SkillData : ScriptableObject
{
    public abstract SkillType SkillType { get; }

    [Header("스킬 이름과 정보")]
    public string skillName;
    public string description;

    [Header("스킬 기본 스탯")]
    public float cooldown;

    [Header("스킬 활성화 타입")]
    public SkillActivationType activationType;

    [Header("공격자와 이펙트간 거리")]
    public float hitboxDistanceFromUser = 1.0f;

    [Header("스킬의 상태이상 정보")]
    [UnityEngine.SerializeReference] public List<StatusEffectInfo> statusEffects;

    [Header("이펙트 회전 여부")]
    public bool rotateSkill = true;

    public abstract SkillInstance CreateInstance();
}
