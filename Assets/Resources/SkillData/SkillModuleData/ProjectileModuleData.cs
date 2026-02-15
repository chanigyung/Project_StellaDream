using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Projectile")]
public class ProjectileModuleData : SkillModuleData
{
    public GameObject projectilePrefab;
    public float speed = 10f;
    public float lifetime = 2f;

    public Vector2 spawnOffset = Vector2.zero;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new ProjectileModule(owner, this);
    }
}
