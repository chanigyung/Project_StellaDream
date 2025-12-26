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
    public static void ApplyKnockback(GameObject attacker, GameObject target, Vector2 force)
    {
        if (force == Vector2.zero) return;
        if (target.TryGetComponent<IKnockbackable>(out var knockbackable))
            knockbackable.ApplyKnockback(force);
    }

    // 상태이상 적용
    public static void ApplyStatusEffects(GameObject attacker, GameObject target, SkillInstance skill)
    {
        if (skill.statusEffects == null || skill.statusEffects.Count == 0) return;
        if (!target.TryGetComponent<StatusEffectManager>(out var eManager)) return;

        foreach (var effect in skill.statusEffects)
        {
            var instance = StatusEffectFactory.CreateEffectInstance(effect, target, attacker, eManager);
            if (instance != null)
                eManager.ApplyEffect(instance);
        }
    }

    // 넉백 방향 계산만 분리
    public static Vector2 GetKnockbackDirection(SkillInstance skill, GameObject attacker, GameObject target)
    {
        float xDir = Mathf.Sign(target.transform.position.x - attacker.transform.position.x);
        return new Vector2(skill.baseData.knockbackX * xDir, skill.baseData.knockbackY);
    }

    //히트박스 생성(근접)
    public static void SpawnHitbox(GameObject attacker, SkillInstance skill, Vector2 direction)
    {
        if (skill is not IHitboxInfo info) return;

        Transform spawnPoint = GetSpawnPoint(attacker, skill);

        Vector2 offset = skill.spawnOffset;
        offset.x *= Mathf.Sign(direction.x); // 좌우 반전

        Vector3 spawnPos = spawnPoint.position + (Vector3)(spawnPoint.rotation * offset);

        GameObject hitbox = Object.Instantiate(info.HitboxPrefab, spawnPos, Quaternion.identity);

        if (skill.ShouldAttachToSpawnPoint())
        {
            hitbox.transform.SetParent(spawnPoint, worldPositionStays: true);
        }

        if (hitbox.TryGetComponent(out SkillHitbox hitboxComp))
        {
            hitboxComp.Initialize(attacker, skill, direction);
        }

        skill.spawnedHitbox = hitbox;
        skill.spawnedEffect = PlayEffect(skill, spawnPos, direction);
    }

    //투사체 생성(원거리)
    public static void SpawnProjectile(GameObject attacker, SkillInstance skill, Vector2 direction)
    {
        if (skill is not IProjectileInfo info) return;

        Transform spawnPoint = GetSpawnPoint(attacker, skill);

        Vector2 offset = skill.spawnOffset;
        offset.x *= Mathf.Sign(direction.x); // 좌우 반전

        Vector3 spawnPos = spawnPoint.position + (Vector3)offset;

        GameObject projectile = Object.Instantiate(info.ProjectilePrefab, spawnPos, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile projComp))
            projComp.Initialize(attacker, skill, direction);

        skill.spawnedProjectile = projectile;
        skill.spawnedEffect = PlayEffect(skill, spawnPos, direction);
    }

    //스킬 사용 좌표 계산
    public static Transform GetSpawnPoint(GameObject attacker, SkillInstance skill)
    {
        var pointHolder = attacker.GetComponent<SkillSpawnPoints>();
        if (pointHolder == null)
        {
            Debug.LogWarning("[SkillUtils] SkillSpawnPoints가 존재하지 않습니다.");
            return attacker.transform; // fallback
        }

        return pointHolder.GetPoint(skill.baseData.spawnPointType);
    }

    //스킬 이펙트 재생
    public static GameObject PlayEffect(SkillInstance skill, Vector2 position, Vector2 direction)
    {
        if (skill.effectPrefab == null || skill.effectAnimator == null)
            return null;

        GameObject effect = Object.Instantiate(skill.effectPrefab, position, Quaternion.identity);
        if (effect.TryGetComponent(out SkillVFXController vfx))
        {
            vfx.applyRotation = skill.RotateEffect;
            vfx.Initialize(direction.normalized, skill.effectDuration, skill.effectAnimator, skill.FlipSpriteY);
        }

        return effect;
    }

    // 예시: 스킬에서 세 가지 모두 사용하는 경우
    // (예: 충돌 시 데미지 + 넉백 + 상태이상 적용)
    public static void ApplyAllEffects(GameObject attacker, GameObject target, SkillInstance skill)
    {
        // 데미지
        ApplyDamage(target, (skill is MeleeSkillInstance melee) ? melee.damage : (skill is ProjectileSkillInstance proj ? proj.damage : 0f));

        //넉백
        Vector2 force = GetKnockbackDirection(skill, attacker, target);
        ApplyKnockback(attacker, target, force);

        // 상태이상
        ApplyStatusEffects(attacker, target, skill);
    }
}
