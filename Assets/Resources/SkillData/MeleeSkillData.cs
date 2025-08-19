using UnityEngine;

[CreateAssetMenu(menuName = "Skill/MeleeSkill")]
public class MeleeSkillData : SkillData
{
    public override SkillType SkillType => SkillType.Melee;
    public GameObject hitboxPrefab;

    [Header("기본 데미지 / 강화배율")]
    public float baseDamage;
    public float damagePerUpgrade = 1f;

    [Header("기본 넉백 수치")]
    public float knockbackX = 5f;
    public float knockbackY = 5f;
    public bool useBasicKnockback = true;

    [Header("범위 / 강화배율")]
    public float hitboxWidth = 1.0f;
    public float hitboxHeight = 1.0f;
    public float widthPerUpgrade = 0.1f;
    public float heightPerUpgrade = 0.1f;

    [Header("스킬 이펙트")]
    public RuntimeAnimatorController skillEffectAnimation;
    public float skillEffectDuration = 0.2f;

    public override SkillInstance CreateInstance()
    {
        return new MeleeSkillInstance(this);
    }
}