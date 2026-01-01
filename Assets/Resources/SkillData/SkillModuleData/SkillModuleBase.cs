using UnityEngine;

public abstract class SkillModuleBase : ISkillModule
{
    protected SkillInstance owner;

    protected SkillModuleBase(SkillInstance owner)
    {
        this.owner = owner;
    }

    public virtual void OnExecute(GameObject attacker, Vector2 direction) { }
    public virtual void OnHit(GameObject attacker, GameObject target) { }
    public virtual void OnTick(GameObject attacker) { }
    public virtual void OnExpire(GameObject attacker) { }
}
