using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Object/Projectile")]
public class ProjectileModuleData : SkillModuleData
{
    public string objectId;

    public GameObject projectilePrefab;
    public float speed = 10f;
    public float lifetime = 2f;

    public Vector2 spawnOffset = Vector2.zero;

    public SkillSpawnPointType ownerSpawnPointType = SkillSpawnPointType.Left;
    public SkillSpawnPointType prefabSpawnPointType = SkillSpawnPointType.Left;

    public hitEffect hitEffect = new hitEffect();

    public override ISkillModule CreateModule()
    {
        return new ProjectileModule(this);
    }
}
