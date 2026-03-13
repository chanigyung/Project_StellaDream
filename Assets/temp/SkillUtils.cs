using System.Collections.Generic;
using UnityEngine;

public static class SkillUtils
{
    public static void ApplyDamage(GameObject target, float damage)
    {
        if (target.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);
    }

    public static void ApplyKnockback(GameObject attacker, GameObject target, float knockbackX, float knockbackY)
    {
        if (target == null) return;
        if (attacker == null) return;
        if (!target.TryGetComponent<Rigidbody2D>(out var rb))
            return;

        float dir = Mathf.Sign(target.transform.position.x - attacker.transform.position.x);
        Vector2 force = new Vector2(dir * knockbackX, knockbackY);

        if (target.TryGetComponent<IKnockbackable>(out var knockbackable))
        {
            knockbackable.ApplyKnockback(force);
            return;
        }
    }

    // 수정: 히트박스는 owner 기준점 + prefab 기준점을 모두 사용
    public static void SpawnHitbox(
        SkillContext context,
        GameObject hitboxPrefab,
        SkillSpawnPointType ownerSpawnPointType,
        SkillSpawnPointType prefabSpawnPointType,
        Vector2 spawnOffset,
        Vector2 hitboxSize,
        float lifetime)
    {
        SkillInstance skill = context.skillInstance;
        if (skill == null || hitboxPrefab == null) return;

        Vector2 dir = ResolveDirection(context);
        Quaternion rotation = ResolveSpawnRotation(context, skill, dir);
        Vector3 ownerPoint = GetOwnerWorldPoint(context, ownerSpawnPointType);

        GameObject hitbox = Object.Instantiate(hitboxPrefab, ownerPoint, Quaternion.identity);

        if (hitbox.TryGetComponent(out BoxCollider2D box))
        {
            box.size = hitboxSize;
            box.offset = Vector2.zero;
        }

        if (hitbox.TryGetComponent(out SkillHitbox hitboxComp))
        {
            hitboxComp.Initialize(context, lifetime);
        }

        // 추가: collider size/회전 반영 후 동적 prefab 기준점을 맞춰 최종 위치 보정
        AlignSpawnedObject(hitbox, ownerPoint, prefabSpawnPointType, dir, spawnOffset, rotation, skill);

        skill.RegisterSpawnedObject(hitbox);
    }

    // 수정: 투사체도 owner 기준점 + prefab 기준점을 모두 사용
    public static void SpawnProjectile(
        SkillContext context,
        GameObject projectilePrefab,
        SkillSpawnPointType ownerSpawnPointType,
        SkillSpawnPointType prefabSpawnPointType,
        Vector2 spawnOffset,
        float speed,
        float lifetime)
    {
        SkillInstance skill = context.skillInstance;
        if (skill == null || projectilePrefab == null) return;

        Vector2 dir = ResolveDirection(context);
        Quaternion rotation = ResolveSpawnRotation(context, skill, dir);
        Vector3 ownerPoint = GetOwnerWorldPoint(context, ownerSpawnPointType);

        GameObject projectile = Object.Instantiate(projectilePrefab, ownerPoint, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(context, speed, lifetime);
        }

        // 추가: 회전/플립 반영 후 prefab 기준점 정렬
        AlignSpawnedObject(projectile, ownerPoint, prefabSpawnPointType, dir, spawnOffset, rotation, skill);

        skill.RegisterSpawnedObject(projectile);
    }

    public static Transform GetSpawnPoint(GameObject spawnOwner, SkillSpawnPointType type)
    {
        if (spawnOwner == null)
            return null;

        var spawnPoint = spawnOwner.GetComponent<SkillSpawnPoints>();
        if (spawnPoint == null)
        {
            return spawnOwner.transform;
        }

        return spawnPoint.GetPoint(type);
    }

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

    public static void CalculateSpawnTransform(SkillContext context, SkillInstance skill,
        SkillSpawnPointType spawnPointType, Vector2 offset, out Vector3 position, out Quaternion rotation, out Transform spawnPoint)
    {
        GameObject spawnOwner = context.contextOwner;
        spawnPoint = GetSpawnPoint(spawnOwner, spawnPointType);

        Vector2 dir = context.hasDirection && context.direction.sqrMagnitude > 0.0001f
            ? context.direction.normalized
            : Vector2.right;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        if (dir.x < 0f && skill.FlipSpriteY)
        {
            offset.y *= -1f;
        }

        Vector3 worldOffset = (Vector3)(dir * offset.x + perp * offset.y);

        if (spawnPoint != null)
            position = spawnPoint.position + worldOffset;
        else
            position = context.position + worldOffset;

        rotation = skill.RotateEffect
            ? Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg)
            : context.rotation;
    }

    // 추가: owner 쪽 기준점 월드좌표 계산
    public static Vector3 GetOwnerWorldPoint(SkillContext context, SkillSpawnPointType ownerSpawnPointType)
    {
        GameObject spawnOwner = context.contextOwner;
        if (spawnOwner == null)
            return context.position;

        if (spawnOwner.TryGetComponent<ISkillSpawnPointProvider>(out var provider))
            return provider.GetWorldPoint(ownerSpawnPointType);

        Transform fallbackPoint = GetSpawnPoint(spawnOwner, ownerSpawnPointType);
        return fallbackPoint != null ? fallbackPoint.position : spawnOwner.transform.position;
    }

    // 추가: 생성된 오브젝트의 prefab 기준점을 찾아 owner 기준점에 정렬
    public static void AlignSpawnedObject(
        GameObject spawnedObject,
        Vector3 ownerPoint,
        SkillSpawnPointType prefabSpawnPointType,
        Vector2 direction,
        Vector2 spawnOffset,
        Quaternion rotation,
        SkillInstance skill)
    {
        if (spawnedObject == null) return;

        Vector3 prefabPoint = GetPrefabWorldPoint(spawnedObject, prefabSpawnPointType);
        Vector3 alignDelta = ownerPoint - prefabPoint;

        Vector3 worldOffset = CalculateDirectionalOffset(direction, spawnOffset, skill);
        spawnedObject.transform.position += alignDelta + worldOffset;
    }

    // 추가: 일반 프리팹/동적 히트박스 프리팹 모두 대응
    public static Vector3 GetPrefabWorldPoint(GameObject spawnedObject, SkillSpawnPointType prefabSpawnPointType)
    {
        if (spawnedObject == null)
            return Vector3.zero;

        if (spawnedObject.TryGetComponent<ISkillSpawnPointProvider>(out var provider))
            return provider.GetWorldPoint(prefabSpawnPointType);

        return spawnedObject.transform.position;
    }

    // 추가: owner/prefab 정렬 이후 적용할 방향성 offset 계산
    public static Vector3 CalculateDirectionalOffset(Vector2 direction, Vector2 spawnOffset, SkillInstance skill)
    {
        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        Vector2 finalOffset = spawnOffset;

        if (dir.x < 0f && skill != null && skill.FlipSpriteY)
            finalOffset.y *= -1f;

        return (Vector3)(dir * finalOffset.x + perp * finalOffset.y);
    }

    // 추가: 생성 로직 공용 방향 계산
    public static Vector2 ResolveDirection(SkillContext context)
    {
        if (context.hasDirection && context.direction.sqrMagnitude > 0.0001f)
            return context.direction.normalized;

        return Vector2.right;
    }

    // 추가: 생성 로직 공용 회전 계산
    public static Quaternion ResolveSpawnRotation(SkillContext context, SkillInstance skill, Vector2 direction)
    {
        if (skill == null)
            return context.rotation;

        if (!skill.RotateEffect)
            return context.rotation;

        return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

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
