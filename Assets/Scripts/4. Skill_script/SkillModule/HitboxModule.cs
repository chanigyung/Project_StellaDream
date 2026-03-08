using UnityEngine;

public class HitboxModule : SkillModuleBase
{
    private readonly HitboxModuleData data;

    public HitboxModule(SkillInstance owner, HitboxModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, owner, data);
    }
}

