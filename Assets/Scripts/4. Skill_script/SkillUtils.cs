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
        Vector2 offset = data.spawnOffset;
        CalculateSpawnTransform(attacker,skill,direction, skill.data.spawnPointType, offset, out var pos, out var rot, out var spawnPoint);

        GameObject hitbox = Object.Instantiate(data.hitboxPrefab, pos, Quaternion.identity);

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
        Vector2 offset = data.spawnOffset;
        CalculateSpawnTransform(attacker,skill,direction, skill.data.spawnPointType,offset, out var pos, out var rot, out var spawnPoint);

        GameObject projectile = Object.Instantiate(data.projectilePrefab, pos, Quaternion.identity);

        // 이동/회전/플립/수명은 Projectile이 처리
        if (projectile.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(attacker,skill,direction,data.speed,data.lifetime);
        }

        skill.spawnedProjectile = projectile;
    }

    //장판스킬 히트박스 생성
    public static void SpawnAreaHitbox( GameObject attacker, SkillInstance skill, Vector2 direction, AreaHitboxModuleData data)
    {
        Vector2 offset = data.spawnOffset;
        CalculateSpawnTransform(attacker, skill, direction, skill.data.spawnPointType, offset, out var pos, out var rot, out var spawnPoint);

        GameObject hitboxObj = Object.Instantiate(data.hitboxPrefab, pos, rot);

        if (hitboxObj.TryGetComponent(out BoxCollider2D col))
        {
            col.isTrigger = true;
            col.size = data.size;
            col.offset = Vector2.zero;
        }

        if (hitboxObj.TryGetComponent(out AreaHitbox area))
        {
            area.Initialize(attacker, skill, data);
        }

        skill.spawnedHitbox = hitboxObj;
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

    // VFX 실행
    public static GameObject SpawnVFX(GameObject spawnOwner, SkillInstance skill, Vector2 direction, VFXEntry entry)
    {
        if (entry == null || entry.prefab == null) return null;

        CalculateSpawnTransform(spawnOwner, skill, direction, entry.spawnPointType, entry.spawnOffset, out var pos, out var rot, out var spawnPoint);

        GameObject vfx = Object.Instantiate(entry.prefab, pos, rot);

        if (skill.RotateEffect && direction.x < 0f && skill.FlipSpriteY)
        {
            Vector3 scale = vfx.transform.localScale;
            scale.y *= -1f;
            vfx.transform.localScale = scale;
        }

        if (entry.animator != null && vfx.TryGetComponent(out Animator anim))
        {
            anim.runtimeAnimatorController = entry.animator;

            if (!string.IsNullOrEmpty(entry.trigger))
                anim.SetTrigger(entry.trigger);
        }

        return vfx;
    }

    // 스킬orVFX 스폰 위치 및 방향 계산
    public static void CalculateSpawnTransform(GameObject spawnOwner,SkillInstance skill, Vector2 direction,
        SkillSpawnPointType spawnPointType, Vector2 offset, out Vector3 position, out Quaternion rotation, out Transform spawnPoint)
    {
        spawnPoint = GetSpawnPoint(spawnOwner, spawnPointType);
        if (spawnPoint == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

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
