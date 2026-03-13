using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Melee, Projectile, Combo }
public enum SkillActivationType { OnPress, OnRelease, WhileHeld }
public enum SkillSpawnPointType { Center, Left, Right, Ground }

[CreateAssetMenu(menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("스킬 모듈")]
    public List<SkillModuleData> modules;

    [Header("스킬 기본 스탯")]
    public float cooldown;
    public float delay = 0f;
    public float postDelay = 0f;

    [Header("스킬 활성화 타입")]
    public SkillActivationType activationType;

    [Header("스킬 위치 설정")]
    public SkillSpawnPointType spawnPointType = SkillSpawnPointType.Center; // 유지: VFX/레거시 fallback 용도

    [Header("스킬 상태이상")]
    [UnityEngine.SerializeReference] public List<StatusEffectInfo> statusEffects;

    [Header("스킬 회전여부")]
    public bool rotateSkill = true;
    public bool flipSpriteY = true;

    [Header("스킬 동시 사용 가능 여부")]
    public bool ignoreCastLock = false;

    public virtual SkillInstance CreateInstance()
    {
        return new SkillInstance(this);
    }
}
