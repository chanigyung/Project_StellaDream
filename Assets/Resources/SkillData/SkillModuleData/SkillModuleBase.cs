using UnityEngine;

public abstract class SkillModuleBase : ISkillModule
{
    protected SkillInstance owner;

    protected SkillModuleBase(SkillInstance owner)
    {
        this.owner = owner;
    }

    public virtual void OnDelay(SkillContext context) { }
    public virtual void OnExecute(SkillContext context) { }
    public virtual void OnObjectSpawned(SkillContext context) { }
    public virtual void OnHit(SkillContext context) { }
    public virtual void OnTick(SkillContext context) { }
    public virtual void OnExpire(SkillContext context) { }
    public virtual void OnPostDelay(SkillContext context) { }
}
