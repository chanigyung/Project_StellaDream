using UnityEngine;

public class ProjectileModule : SkillModuleBase
{
    private readonly ProjectileModuleData data;
    private readonly Vector2 spawnOffset;
    private readonly float speed;
    private readonly float lifetime;

    public ProjectileModule(ProjectileModuleData data)
    {
        this.data = data;
        this.spawnOffset = data.spawnOffset;
        this.speed = data.speed;
        this.lifetime = data.lifetime;
    }

    public override void OnExecute(SkillContext context)
    {
        // 수정: owner/prefab 기준점을 함께 넘기도록 변경
        SkillUtils.SpawnProjectile(
            context,
            data.projectilePrefab,
            data.ownerSpawnPointType,
            data.prefabSpawnPointType,
            spawnOffset,
            speed,
            lifetime);
    }
}
