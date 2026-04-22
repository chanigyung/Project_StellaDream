public class CoreInstance
{
    public CoreData data { get; }

    public CoreInstance(CoreData data)
    {
        this.data = data;
    }

    public void ApplyValues(ref SkillContext context)
    {
        data?.ApplyValues(ref context);
    }
}
