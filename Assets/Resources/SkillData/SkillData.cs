using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Melee, Projectile, Combo }
public enum SkillActivationType { OnPress, OnRelease, WhileHeld }
public enum SkillSpawnPointType {Center, Left, Right, Ground}

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
    public SkillSpawnPointType spawnPointType = SkillSpawnPointType.Center; //스킬 소환 지점

    [Header("스킬 상태이상")]
    [UnityEngine.SerializeReference] public List<StatusEffectInfo> statusEffects;

    [Header("스킬 회전여부")]
    public bool rotateSkill = true;
    public bool flipSpriteY = true;
    //이펙트 뒤집을지 여부. 360도 회전하는스킬은 true, 좌우방향 고정되는스킬은 false로 설정할것

    [Header("스킬 동시 사용 가능 여부")]
    public bool ignoreCastLock = false; 

    public virtual SkillInstance CreateInstance()
    {
        return new SkillInstance(this);
    }
}
