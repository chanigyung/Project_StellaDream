using UnityEngine;

public class AreaHitboxModule : SkillModuleBase
{
    private readonly AreaHitboxModuleData data;
    private readonly Vector2 hitboxSize;
    private readonly Vector2 spawnOffset;
    private readonly float lifetime;

    public AreaHitboxModule(AreaHitboxModuleData data)
    {
        this.data = data;
        hitboxSize = data.hitboxSize;
        spawnOffset = data.spawnOffset;
        lifetime = data.lifetime;
    }

    public override void OnExecute(SkillContext context)
    {
        // 수정: area hitbox도 동일한 owner/prefab 기준점 구조 사용
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
