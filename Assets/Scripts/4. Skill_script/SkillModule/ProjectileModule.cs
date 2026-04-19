using UnityEngine;

public class ProjectileModule : SkillModuleBase
{
    private readonly ProjectileModuleData data;
    private readonly Vector2 spawnOffset;
    private readonly float speed;
    private readonly float lifetime;
    private readonly hitEffect hitEffect;

    public ProjectileModule(ProjectileModuleData data)
    {
        this.data = data;
        this.spawnOffset = data.spawnOffset; 
        this.speed = data.speed;
        this.lifetime = data.lifetime;
        this.hitEffect = data.hitEffect?.Clone();
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnProjectile(context, data, spawnOffset, speed, lifetime, hitEffect);
    }
}

