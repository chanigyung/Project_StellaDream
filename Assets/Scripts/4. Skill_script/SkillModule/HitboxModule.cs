using UnityEngine;

public class HitboxModule : SkillModuleBase
{
    private readonly HitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;
    private readonly hitEffect hitEffect;

    public HitboxModule(HitboxModuleData data)
    {
        this.data = data;
        this.hitboxSize = data.hitboxSize;
        this.spawnOffset = data.spawnOffset;
        this.lifetime = data.lifetime;
        this.hitEffect = data.hitEffect?.Clone();
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, data, spawnOffset, hitboxSize, lifetime, hitEffect);
    }
}

