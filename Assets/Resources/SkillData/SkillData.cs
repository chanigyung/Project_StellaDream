using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Melee, Projectile, Combo }
public enum SkillActivationType { OnPress, OnRelease, WhileHeld }
public enum SkillSpawnPointType {LeftArm,RightArm,GroundCenter}

public abstract class SkillData : ScriptableObject
{
    public abstract SkillType SkillType { get; }

    [Header("스킬 이름과 정보")]
    public string skillName;
    public string description;

    [Header("스킬 기본 스탯")]
    public float cooldown;
    public float knockbackX = 5f;
    public float knockbackY = 5f;

    [Header("스킬 활성화 타입")]
    public SkillActivationType activationType;

    [Header("스킬 위치 설정")]
    public SkillSpawnPointType spawnPointType = SkillSpawnPointType.LeftArm; //스킬 소환 지점
    public Vector2 spawnOffset = Vector2.zero;

    [Header("스킬 상태이상")]
    [UnityEngine.SerializeReference] public List<StatusEffectInfo> statusEffects;

    [Header("스킬 이펙트 설정")]
    public RuntimeAnimatorController skillEffectAnimation;
    public GameObject skillEffectPrefab;
    public float skillEffectDuration = 0.2f;
    public bool rotateSkill = true;
    public bool flipSpriteY = true;
    //이펙트 뒤집을지 여부. 360도 회전하는스킬은 true, 좌우방향 고정되는스킬은 false로 설정할것

    [Header("스킬 시전 딜레이")]
    public float castDelay = 0.5f;
    public float castPostDelay = 0.5f;

    public abstract SkillInstance CreateInstance();
}
