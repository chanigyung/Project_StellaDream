using UnityEngine;

public class AreaHitboxModule : SkillModuleBase
{
    private readonly AreaHitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;
    private readonly hitEffect hitEffect;
    private readonly hitEffect tickHitEffect;

    public AreaHitboxModule(AreaHitboxModuleData data)
    {
        this.data = data;
        hitboxSize = data.hitboxSize;
        spawnOffset = data.spawnOffset;
        lifetime = data.lifetime; 
        hitEffect = data.hitEffect?.Clone();
        tickHitEffect = data.tickHitEffect?.Clone();
    }

    public override void OnExecute(SkillContext context)
    {
        context.EnsureValues();

        Vector2 finalHitboxSize = hitboxSize * Mathf.Max(0f, context.values.hitboxSizeMultiplier);
        float finalLifetime = lifetime * Mathf.Max(0f, context.values.lifetimeMultiplier);

        SkillUtils.SpawnHitbox(context, data, spawnOffset, finalHitboxSize, finalLifetime, hitEffect, tickHitEffect, data.tickInterval);
    }
}
