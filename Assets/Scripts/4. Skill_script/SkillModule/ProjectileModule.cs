using UnityEngine;

public class ProjectileModule : SkillModuleBase
{
    private readonly ProjectileModuleData data;

    public ProjectileModule(SkillInstance owner, ProjectileModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnExecute(GameObject attacker, Vector2 direction)
    {
        SkillUtils.SpawnProjectile(attacker, owner, direction, data);
    }
}

