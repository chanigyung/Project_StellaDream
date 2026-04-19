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
        if (hitCount <= 0) return;

        for (int i = 0; i < hitCount; i++)
        {
            SkillUtils.ApplyDamage(context.targetObject, damage);
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
