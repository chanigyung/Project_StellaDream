using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class hitEffect
{
    public float damage = 0f;
    public int hitCount = 1;

    public float knockbackX = 0f;
    public float knockbackY = 0f;
    public float knockbackDuration = 0.3f;

    [SerializeReference] public List<StatusEffectInfo> statusEffects;

    public void Apply(SkillContext context)
    {
        ApplyDamage(context);
        ApplyKnockback(context);
        ApplyStatusEffects(context);
    }

    private void ApplyDamage(SkillContext context)
    {
        if (context.targetObject == null) return;
        if (damage <= 0f) return;

        context.EnsureValues();

        int finalHitCount = Mathf.Max(0, hitCount + context.values.additionalHitCount);
        if (finalHitCount <= 0) return;

        float finalDamage = damage * Mathf.Max(0f, context.values.damageMultiplier);
        if (finalDamage <= 0f) return;

        for (int i = 0; i < finalHitCount; i++)
        {
            SkillUtils.ApplyDamage(context.targetObject, finalDamage);
        }
    }

    private void ApplyKnockback(SkillContext context)
    {
        if (knockbackX == 0f && knockbackY == 0f) return;

        SkillUtils.ApplyKnockback(
            context.attacker,
            context.targetObject,
            knockbackX,
            knockbackY,
            knockbackDuration
        );
    }

    private void ApplyStatusEffects(SkillContext context)
    {
        if (statusEffects == null || statusEffects.Count == 0) return;

        // StatusEffect is not wired into the current hit flow yet.
    }

    public hitEffect Clone()
    {
        return new hitEffect
        {
            damage = damage,
            hitCount = hitCount,
            knockbackX = knockbackX,
            knockbackY = knockbackY,
            knockbackDuration = knockbackDuration,
            statusEffects = statusEffects != null ? new List<StatusEffectInfo>(statusEffects) : null
        };
    }
}
