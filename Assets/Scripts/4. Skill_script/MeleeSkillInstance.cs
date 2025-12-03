using UnityEngine;

public class MeleeSkillInstance : SkillInstance, IHitboxInfo
{
    public float damage;
    public float width;
    public float height;
    public GameObject hitboxPrefab;

    //Hitbox 정보 인터페이스
    public float Width => width;
    public float Height => height;
    public GameObject HitboxPrefab => hitboxPrefab;

    public MeleeSkillInstance(MeleeSkillData data) : base(data)
    {
        damage = data.baseDamage;
        width = data.hitboxWidth;
        height = data.hitboxHeight;

        hitboxPrefab = data.hitboxPrefab;

        if (data.statusEffects != null)
        {
            foreach (var effect in data.statusEffects)
            {
                statusEffects.Add(CopyEffect(effect));
            }
        }
    }

    public override void Execute(GameObject attacker, Vector2 direction)
    {
        SkillUtils.SpawnHitbox(attacker, this, direction);
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        SkillUtils.ApplyDamage(target, damage);

        SkillUtils.ApplyKnockback(attacker, target, SkillUtils.GetKnockbackDirection(this, attacker, target));

        SkillUtils.ApplyStatusEffects(attacker, target, this);
    }

    public override void ApplyUpgrade(WeaponUpgradeInfo upgrade)
    {
        if (upgrade == null) return;

        var data = (MeleeSkillData)baseData;
        
        damage = damage + data.damagePerUpgrade * upgrade.baseUpgradeLevel;
        width = width + data.widthPerUpgrade * upgrade.efficiencyUpgradeLevel;
        height = height + data.heightPerUpgrade * upgrade.efficiencyUpgradeLevel;

        ApplyStatusEffectUpgrade(upgrade.masteryUpgradeLevel);
    }
}