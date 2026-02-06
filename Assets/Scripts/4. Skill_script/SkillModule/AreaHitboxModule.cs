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
        // [추가] Area 히트박스 스폰
        SkillUtils.SpawnAreaHitbox(attacker, owner, direction, data);
    }
}
