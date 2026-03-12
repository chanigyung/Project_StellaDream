public class AreaHitboxModule : SkillModuleBase
{
    private readonly AreaHitboxModuleData data;

    public AreaHitboxModule(AreaHitboxModuleData data)
    {
        this.data = data;
    }

    public override void OnExecute(SkillContext context)
    {
        SkillUtils.SpawnHitbox(context, data);
    }
}