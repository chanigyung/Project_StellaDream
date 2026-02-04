using UnityEngine;

public static class SkillUtils
{
    // 데미지 적용
    public static void ApplyDamage(GameObject target, float damage)
    {
        if (target.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);
    }

    // 넉백 적용
    public static void ApplyKnockback(GameObject attacker, GameObject target, float knockbackX, float knockbackY)
    {
        if (!target.TryGetComponent<Rigidbody2D>(out var rb))
            return;

        float dir = Mathf.Sign(target.transform.position.x - attacker.transform.position.x);
        Vector2 force = new Vector2(dir * knockbackX, knockbackY);

        rb.velocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    // 상태이상 적용
    // public static void ApplyStatusEffects(GameObject attacker, GameObject target, SkillInstance skill)
    // {
    //     if (skill.statusEffects == null || skill.statusEffects.Count == 0) return;
    //     if (!target.TryGetComponent<StatusEffectManager>(out var eManager)) return;

    //     foreach (var effect in skill.statusEffects)
    //     {
    //         var instance = StatusEffectFactory.CreateEffectInstance(effect, target, attacker, eManager);
    //         if (instance != null)
    //             eManager.ApplyEffect(instance);
    //     }
    // }

    //히트박스 생성(근접)
    public static void SpawnHitbox(GameObject attacker, SkillInstance skill, Vector2 direction, HitboxModuleData data)
    {
        CalculateSpawnTransform(attacker,skill,direction, out var pos, out var rot, out var spawnPoint);

        GameObject hitbox = Object.Instantiate(data.hitboxPrefab,pos,Quaternion.identity);

        if (skill.data.attachToSpawnPoint)
            hitbox.transform.SetParent(spawnPoint, true);

        if (hitbox.TryGetComponent(out BoxCollider2D box))
        {
            box.size = data.hitboxSize;
            box.offset = Vector2.zero;
        }

        // 방향/회전/플립은 SkillHitbox가 처리
        if (hitbox.TryGetComponent(out SkillHitbox hitboxComp))
        {
            hitboxComp.Initialize(attacker, skill, direction);
        }

        Object.Destroy(hitbox, data.lifetime);
        skill.spawnedHitbox = hitbox;
    }

    //투사체 생성(원거리)
    public static void SpawnProjectile(GameObject attacker, SkillInstance skill, Vector2 direction, ProjectileModuleData data)
    {
        CalculateSpawnTransform(attacker,skill,direction, out var pos, out var rot, out var spawnPoint);

        GameObject projectile = Object.Instantiate(data.projectilePrefab, pos, Quaternion.identity);

        // 이동/회전/플립/수명은 Projectile이 처리
        if (projectile.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(attacker,skill,direction,data.speed,data.lifetime);
        }

        skill.spawnedProjectile = projectile;
    }

    //스킬 사용 좌표 계산
    public static Transform GetSpawnPoint(GameObject spawnOwner, SkillInstance skill)
    {
        if (spawnOwner == null)
            return null;

        var spawnPoint = spawnOwner.GetComponent<SkillSpawnPoints>();
        if (spawnPoint == null)
        {
            return spawnOwner.transform; // fallback
        }

        return spawnPoint.GetPoint(skill.data.spawnPointType);
    }

    public static GameObject SpawnVFX(GameObject spawnOwner, SkillInstance skill, Vector2 direction,
        GameObject prefab, RuntimeAnimatorController animator, string trigger)
    {
        if (prefab == null) return null;

        CalculateSpawnTransform(spawnOwner, skill, direction, out var pos, out var rot, out var spawnPoint);

        GameObject vfx = Object.Instantiate(prefab, pos, rot);

        if (skill.data.attachToSpawnPoint && spawnPoint != null)
            vfx.transform.SetParent(spawnPoint, true);

        if (skill.RotateEffect && direction.x < 0f && skill.FlipSpriteY)
        {
            Vector3 scale = vfx.transform.localScale;
            scale.y *= -1f;
            vfx.transform.localScale = scale;
        }

        if (animator != null && vfx.TryGetComponent(out Animator anim))
        {
            anim.runtimeAnimatorController = animator;

            if (!string.IsNullOrEmpty(trigger))
                anim.SetTrigger(trigger);
        }

        return vfx;
    }

    public static void CalculateSpawnTransform(GameObject spawnOwner,SkillInstance skill,
        Vector2 direction, out Vector3 position, out Quaternion rotation, out Transform spawnPoint)
    {
        spawnPoint = GetSpawnPoint(spawnOwner, skill);
        if (spawnPoint == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

        Vector2 offset = skill.spawnOffset;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector3 worldOffset = (Vector3)(dir * offset.x + perp * offset.y);

        position = spawnPoint.position + worldOffset;

        rotation = Quaternion.identity;
        if (skill.RotateEffect)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public static Quaternion CalculateRotation(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle);
    }
    
}
