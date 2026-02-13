using UnityEngine;

public class AreaHitboxModule : SkillModuleBase
{
    private readonly AreaHitboxModuleData data;

    public AreaHitboxModule(SkillInstance owner, AreaHitboxModuleData data) : base(owner)
    {
        this.data = data;
    }

    public override void OnExecute(GameObject attacker, Vector2 direction)
    {
        if (owner.data.activationType == SkillActivationType.WhileHeld)
            {
                if (owner.spawnedHitbox != null) return;
            }

        SkillUtils.SpawnAreaHitbox(attacker, owner, direction, data);
    }
}
