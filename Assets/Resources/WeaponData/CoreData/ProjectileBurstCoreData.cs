using UnityEngine;

[CreateAssetMenu(menuName = "Core/Special/Projectile Burst CoreData")]
public class ProjectileBurstCoreData : CoreData
{
    [Header("Projectile Burst")]
    public float projectileSpreadAngle = 0f;
    public int additionalProjectileCount = 0;

    public override void ApplyValues(ref SkillContext context)
    {
        base.ApplyValues(ref context);

        context.values.projectileSpreadAngle += projectileSpreadAngle;
        context.values.additionalProjectileCount += additionalProjectileCount;
    }
}
