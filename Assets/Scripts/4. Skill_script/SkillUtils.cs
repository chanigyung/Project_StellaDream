using System.Collections.Generic;
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
    public static void ApplyKnockback(GameObject attacker, GameObject target, float knockbackX, float knockbackY, float duration = 0.3f)
    {
        if (target == null) return;
        if (attacker == null) return;
        if (!target.TryGetComponent<Rigidbody2D>(out var rb))
            return;

        float dir = Mathf.Sign(target.transform.position.x - attacker.transform.position.x);
        Vector2 force = new Vector2(dir * knockbackX, knockbackY);

        if (target.TryGetComponent<IKnockbackable>(out var knockbackable))
        {
            knockbackable.ApplyKnockback(force, duration);
            return;
        }
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
    public static void SpawnHitbox(SkillContext context, HitboxModuleData data, Vector2 spawnOffset, Vector2 hitboxSize, float lifetime) 
    {
        SkillInstance skill = context.skillInstance;
        if (skill == null) return;

        Vector3 ownerWorldPoint = GetSpawnPoint(context, data.ownerSpawnPointType);

        CalculateSpawnTransform(context, skill, data.ownerSpawnPointType, 
            Vector2.zero, out var basePos, out var rot, out var spawnPoint);

        GameObject hitbox = Object.Instantiate(data.hitboxPrefab, ownerWorldPoint, Quaternion.identity);

        if (hitbox.TryGetComponent(out BoxCollider2D box))
        {
            box.size = hitboxSize;
            box.offset = Vector2.zero;
        }

        if (hitbox.TryGetComponent(out SkillHitbox hitboxComp))
        {
            hitboxComp.Initialize(context, lifetime);
        }

        // owner 기준점에 hitbox 프리팹 기준점 정렬
        AlignSpawnedObject(hitbox, ownerWorldPoint, data.prefabSpawnPointType);

        // 정렬 후 방향성 offset 적용
        Vector2 offset = spawnOffset;
        CalculateSpawnTransform(context, skill, data.ownerSpawnPointType, 
            offset, out var finalPos, out rot, out spawnPoint);

        hitbox.transform.position += finalPos - ownerWorldPoint;

        hitboxComp?.ObjectSpawned();
        skill.RegisterSpawnedObject(hitbox);
    }

    // 투사체 생성(원거리)
    public static void SpawnProjectile(SkillContext context, ProjectileModuleData data, Vector2 spawnOffset, float speed, float lifetime)
    {
        SkillInstance skill = context.skillInstance;
        if (skill == null) return;

        Vector3 ownerWorldPoint = GetSpawnPoint(context, data.ownerSpawnPointType);

        CalculateSpawnTransform(context, skill, data.ownerSpawnPointType, 
            Vector2.zero, out var basePos, out var rot, out var spawnPoint);

        GameObject projectile = Object.Instantiate(data.projectilePrefab, ownerWorldPoint, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(context, speed, lifetime);
        }

        // owner 기준점에 projectile 프리팹 기준점 정렬
        AlignSpawnedObject(projectile, ownerWorldPoint, data.prefabSpawnPointType);

        // 정렬 후 방향성 offset 적용
        Vector2 offset = spawnOffset;
        CalculateSpawnTransform(context, skill, data.ownerSpawnPointType, 
            offset, out var finalPos, out rot, out spawnPoint);

        projectile.transform.position += finalPos - ownerWorldPoint;

        proj.ObjectSpawned();
        skill.RegisterSpawnedObject(projectile);
    }

    //스킬 소환되는 좌표 계산
    public static Transform GetSpawnPoint(GameObject spawnOwner, SkillSpawnPointType type)
    {
        if (spawnOwner == null)
            return null;

        var spawnPoint = spawnOwner.GetComponent<SkillSpawnPoints>();
        if (spawnPoint == null)
        {
            return spawnOwner.transform; // fallback
        }

        return spawnPoint.GetPoint(type);
    }

    public static Vector3 GetSpawnPoint(SkillContext context, SkillSpawnPointType type)
    {
        return type switch
        {
            SkillSpawnPointType.Ground => context.groundPoint,
            SkillSpawnPointType.Left => context.leftPoint,
            SkillSpawnPointType.Right => context.rightPoint,
            _ => context.position
        };
    }

    // VFX 실행
    public static GameObject SpawnVFX(SkillContext context, VFXEntry entry)
    {
        if (entry == null || entry.prefab == null) return null;

        SkillInstance skill = context.skillInstance;
        if (skill == null) return null;

        CalculateSpawnTransform(context, skill, entry.spawnPointType, entry.spawnOffset, out var pos, out var rot, out var spawnPoint);

        GameObject vfx = Object.Instantiate(entry.prefab, pos, rot);
        skill.RegisterSpawnedObject(vfx);

        if (entry.attachToSpawnPoint && spawnPoint != null)
        {
            vfx.transform.SetParent(spawnPoint, true);
        }
        
        if (context.direction.x < 0f && skill.FlipSpriteY)
        {
            Vector3 scale = vfx.transform.localScale;
            scale.y *= -1f;
            vfx.transform.localScale = scale;
        }

        if (entry.animator != null && vfx.TryGetComponent(out Animator anim))
        {
            anim.runtimeAnimatorController = entry.animator;
        }

        return vfx;
    }

    // 생성된 프리팹의 spawn point 월드 좌표 반환
    public static Vector3 GetPrefabWorldPoint(GameObject spawnedObject, SkillSpawnPointType type)
    {
        if (spawnedObject == null)
            return Vector3.zero;

        SkillSpawnPoints spawnPoints = spawnedObject.GetComponent<SkillSpawnPoints>();
        if (spawnPoints == null)
            return spawnedObject.transform.position;

        return spawnPoints.GetWorldPoint(type);
    }

    // owner 기준점에 prefab 기준점을 맞추는 위치 보정
    public static void AlignSpawnedObject(GameObject spawnedObject, Vector3 ownerWorldPoint, SkillSpawnPointType prefabSpawnPointType)
    {
        if (spawnedObject == null) return;

        Vector3 prefabWorldPoint = GetPrefabWorldPoint(spawnedObject, prefabSpawnPointType);
        Vector3 correction = ownerWorldPoint - prefabWorldPoint;
        spawnedObject.transform.position += correction;
    }

    // 스킬orVFX 스폰 위치 및 방향 계산
    public static void CalculateSpawnTransform(SkillContext context, SkillInstance skill, 
        SkillSpawnPointType spawnPointType, Vector2 offset, out Vector3 position, out Quaternion rotation, out Transform spawnPoint)
    {
        GameObject spawnOwner = context.contextOwner;
        spawnPoint = GetSpawnPoint(spawnOwner, spawnPointType);

        Vector2 dir = context.hasDirection && context.direction.sqrMagnitude > 0.0001f
            ? context.direction.normalized
            : Vector2.right;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        // if (skill.RotateEffect && dir.x < 0f && skill.FlipSpriteY)
        if (dir.x < 0f && skill.FlipSpriteY)
        {
            offset.y *= -1f;
        }

        Vector3 worldOffset = (Vector3)(dir * offset.x + perp * offset.y);

        position = GetSpawnPoint(context, spawnPointType) + worldOffset;

        rotation = skill.RotateEffect ? Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg): context.rotation;
    }

    public static void FillContextSpawnPoints(ref SkillContext context, GameObject pointOwner)
    {
        Vector3 fallback = context.position;

        context.groundPoint = fallback;
        context.leftPoint = fallback;
        context.rightPoint = fallback;

        if (pointOwner == null)
            return;

        SkillSpawnPoints spawnPoints = pointOwner.GetComponent<SkillSpawnPoints>();
        if (spawnPoints == null)
            return;

        if (spawnPoints.groundPoint != null)
            context.groundPoint = spawnPoints.groundPoint.position;

        if (spawnPoints.leftPoint != null)
            context.leftPoint = spawnPoints.leftPoint.position;

        if (spawnPoints.rightPoint != null)
            context.rightPoint = spawnPoints.rightPoint.position;
    }

    //카메라 시야 내의 몬스터들 찾아서 List로 반환
    public static List<Transform> FindEnemyInCamera(Vector2 from, Camera cam, LayerMask enemyLayer)
    {
        List<Transform> result = new();

        if (cam == null)
            return result;

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        Collider2D[] hits = Physics2D.OverlapAreaAll(min, max, enemyLayer);

        HashSet<GameObject> unique = new();

        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>() as Component;
            if (damageable == null) continue;

            GameObject go = damageable.gameObject;
            if (go == null || !go.activeInHierarchy) continue;
            if (!unique.Add(go)) continue;

            result.Add(go.transform);
        }

        return result;
    }   
}
