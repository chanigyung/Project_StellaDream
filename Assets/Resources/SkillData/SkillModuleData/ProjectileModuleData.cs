using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Projectile")]
public class ProjectileModuleData : SkillModuleData
{
    public GameObject projectilePrefab;
    public float speed = 10f;
    public float lifetime = 2f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new ProjectileModule(owner, this);
    }
}
