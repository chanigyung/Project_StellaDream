using UnityEngine;

[CreateAssetMenu(menuName = "Skill/ProjectileSkill")]
public class ProjectileSkillData : SkillData
{
    public override SkillType SkillType => SkillType.Projectile;

    [Header("투사체 프리팹")]
    public GameObject projectilePrefab;

    [Header("기본 데미지 및 강화배율")]
    public float baseDamage;
    public float damagePerUpgrade = 1f;

    [Header("투사체 속도 및 지속시간")]
    public float projectileLifetime;
    public float lifetimePerUpgrade = 1f;
    public float projectileSpeed;
    public float projSpeedPerUpgrade = 2f;

    [Header("기본 넉백 수치")]
    public float knockbackX = 0;
    public float knockbackY = 0;
    public bool useBasicKnockback = true;

    [Header("스킬 이펙트")]
    public RuntimeAnimatorController skillEffectAnimation;
    public float skillEffectDuration = 0.2f;

    public override SkillInstance CreateInstance()
    {
        return new ProjectileSkillInstance(this);
    }
}