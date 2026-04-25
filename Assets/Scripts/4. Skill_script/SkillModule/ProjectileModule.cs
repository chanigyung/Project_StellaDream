using UnityEngine;

public class ProjectileModule : SkillModuleBase
{
    private readonly ProjectileModuleData data;
    private readonly Vector2 spawnOffset;
    private readonly float speed;
    private readonly float lifetime;
    private readonly hitEffect hitEffect;

    public ProjectileModule(ProjectileModuleData data)
    {
        this.data = data;
        this.spawnOffset = data.spawnOffset; 
        this.speed = data.speed;
        this.lifetime = data.lifetime;
        this.hitEffect = data.hitEffect?.Clone();
    }

    public override void OnExecute(SkillContext context)
    {
        context.EnsureValues();

        float finalSpeed = speed * Mathf.Max(0f, context.values.projectileSpeedMultiplier);
        float finalLifetime = lifetime * Mathf.Max(0f, context.values.lifetimeMultiplier);
        int projectileCount = Mathf.Max(1, 1 + context.values.additionalProjectileCount);
        float spreadAngle = context.values.projectileSpreadAngle;

        if (projectileCount == 1 || Mathf.Approximately(spreadAngle, 0f))
        {
            SkillUtils.SpawnProjectile(context, data, spawnOffset, finalSpeed, finalLifetime, hitEffect);
            return;
        }

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            SkillContext projectileContext = context.Clone();
            float angleOffset = startAngle + (angleStep * i);
            projectileContext.direction = RotateDirection(context.direction, angleOffset);
            projectileContext.hasDirection = true;

            SkillUtils.SpawnProjectile(projectileContext, data, spawnOffset, finalSpeed, finalLifetime, hitEffect);
        }
    }

    private static Vector2 RotateDirection(Vector2 direction, float angleOffset)
    {
        Vector2 baseDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector2.right;

        float radians = angleOffset * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            baseDirection.x * cos - baseDirection.y * sin,
            baseDirection.x * sin + baseDirection.y * cos
        ).normalized;
    }
}

