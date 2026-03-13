using UnityEngine;

public class HitboxModule : SkillModuleBase
{
    private readonly HitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;

    public HitboxModule(HitboxModuleData data)
    {
        this.data = data;
        this.hitboxSize = data.hitboxSize;
        this.spawnOffset = data.spawnOffset;
        this.lifetime = data.lifetime;
    }

    public override void OnExecute(SkillContext context)
    {
        // 수정: owner/prefab 기준점을 함께 넘기도록 변경
        SkillUtils.SpawnHitbox(
            context,
            data.hitboxPrefab,
            data.ownerSpawnPointType,
            data.prefabSpawnPointType,
            spawnOffset,
            hitboxSize,
            lifetime);
    }
}
