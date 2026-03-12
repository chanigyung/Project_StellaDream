using UnityEngine;

public class AreaHitboxModule : SkillModuleBase
{
    private readonly AreaHitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;


    public AreaHitboxModule(AreaHitboxModuleData data)
    {
        this.data = data;
        hitboxSize = data.hitboxSize;
        spawnOffset = data.spawnOffset;
        lifetime = data.lifetime; 
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, data.hitboxPrefab, spawnOffset, hitboxSize, lifetime);
    }
}