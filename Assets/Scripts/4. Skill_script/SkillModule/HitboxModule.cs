using UnityEngine;

public class HitboxModule : SkillModuleBase
{
    private readonly HitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;

    public HitboxModule(HitboxModuleData data)
    {
        this.data = data;
        this.hitboxSize = data.hitboxSize;
        this.spawnOffset = data.spawnOffset;
        this.lifetime = data.lifetime;
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, data, spawnOffset, hitboxSize, lifetime);
    }
}

