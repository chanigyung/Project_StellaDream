using UnityEngine;

public class ProjectileModule : SkillModuleBase
{
    private readonly ProjectileModuleData data;

    public ProjectileModule(SkillInstance owner, ProjectileModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnProjectile(context, data);
    }
}

