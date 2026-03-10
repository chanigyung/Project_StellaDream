using UnityEngine;

public class HitboxModule : SkillModuleBase
{
    private readonly HitboxModuleData data;

    public HitboxModule(HitboxModuleData data)
    {
        this.data = data;
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, data);
    }
}

