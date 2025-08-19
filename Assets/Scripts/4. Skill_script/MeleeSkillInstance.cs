using UnityEngine;

public class MeleeSkillInstance : SkillInstance
{
    public float damage;
    public float width;
    public float height;

    public float distanceFromUser;
    public GameObject hitboxPrefab;

    public RuntimeAnimatorController effectAnimator;
    public float effectDuration;

    public MeleeSkillInstance(MeleeSkillData data) : base(data)
    {
        damage = data.baseDamage;
        width = data.hitboxWidth;
        height = data.hitboxHeight;

        distanceFromUser = data.hitboxDistanceFromUser;
        hitboxPrefab = data.hitboxPrefab;

        effectAnimator = data.skillEffectAnimation;
        effectDuration = data.skillEffectDuration;

        if (data.statusEffects != null)
        {
            foreach (var effect in data.statusEffects)
            {
                statusEffects.Add(CopyEffect(effect));
            }
        }
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