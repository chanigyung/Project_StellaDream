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
    public static void SpawnHitbox(SkillContext context, SkillInstance skill, HitboxModuleData data)
    {
        Vector2 offset = data.spawnOffset;
        CalculateSpawnTransform(context, skill, context.spawnPointType, offset, out var pos, out var rot, out var spawnPoint);

        GameObject hitbox = Object.Instantiate(data.hitboxPrefab, pos, Quaternion.identity);

        if (hitbox.TryGetComponent(out BoxCollider2D box))
        {
            box.size = data.hitboxSize;
            box.offset = Vector2.zero;
        }

        // 방향/회전/플립/수명/OnObjectSpawned는 SkillHitbox(SkillObjectBase)가 처리
        if (hitbox.TryGetComponent(out SkillHitbox hitboxComp))
        {
            hitboxComp.Initialize(context, skill, data.lifetime);
        }

        // Register는 SkillUtils가 유지
        skill.RegisterSpawnedObject(hitbox);
    }

    // 투사체 생성(원거리)
    public static void SpawnProjectile(SkillContext context, SkillInstance skill, ProjectileModuleData data)
    {
        Vector2 offset = data.spawnOffset;
        CalculateSpawnTransform(context, skill, context.spawnPointType, offset, out var pos, out var rot, out var spawnPoint);

        GameObject projectile = Object.Instantiate(data.projectilePrefab, pos, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(context, skill, data.speed, data.lifetime);
        }

        skill.RegisterSpawnedObject(projectile);
    }

    // 장판스킬 히트박스 생성
    // public static void SpawnAreaHitbox( GameObject attacker, SkillInstance skill, Vector2 direction, AreaHitboxModuleData data)
    // {
    //     Vector2 offset = data.spawnOffset;
    //     CalculateSpawnTransform(attacker, skill, direction, skill.data.spawnPointType, offset, out var pos, out var rot, out var spawnPoint);

    //     GameObject hitboxObj = Object.Instantiate(data.hitboxPrefab, pos, rot);

    //     if (hitboxObj.TryGetComponent(out BoxCollider2D col))
    //     {
    //         col.isTrigger = true;
    //         col.size = data.size;
    //         col.offset = Vector2.zero;
    //     }

    //     // OnObjectSpawned는 AreaHitbox(SkillObjectBase)가 InitializeCommon에서 처리
    //     if (hitboxObj.TryGetComponent(out AreaHitbox area))
    //     {
    //         area.Initialize(attacker, skill, direction, data);
    //     }

    //     // Register는 SkillUtils가 유지
    //     skill.RegisterSpawnedObject(hitboxObj);
    // }

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

    // VFX 실행
    public static GameObject SpawnVFX(SkillContext context, SkillInstance skill, VFXEntry entry)
    {
        if (entry == null || entry.prefab == null) return null;

        SkillContext vfxContext = context.Clone();
        vfxContext.spawnPointType = entry.spawnPointType;

        CalculateSpawnTransform(vfxContext, skill, entry.spawnPointType, entry.spawnOffset, out var pos, out var rot, out var spawnPoint);

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

        if (spawnPoint != null)
            position = spawnPoint.position + worldOffset;
        else
            position = context.position + worldOffset;

        rotation = skill.RotateEffect ? Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg): context.rotation;
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
