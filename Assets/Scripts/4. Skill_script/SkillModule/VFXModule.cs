using UnityEngine;

public class VFXModule : SkillModuleBase
{
    private readonly VFXModuleData data;

    public VFXModule(SkillInstance owner, VFXModuleData data)
        : base(owner)
    {
        this.data = data;
    }

    public override void OnExecute(GameObject attacker, Vector2 direction)
    {
        SkillUtils.PlayEffect(attacker, owner, direction, data);
    }
}
