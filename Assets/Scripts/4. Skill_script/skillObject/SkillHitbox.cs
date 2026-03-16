using System.Collections.Generic;
using UnityEngine;

public class SkillHitbox : SkillObjectBase
{
    [SerializeField] private BoxCollider2D hitboxCollider;
    [SerializeField] private SkillSpawnPoints spawnPoints;

    protected readonly HashSet<GameObject> alreadyHit = new();

    protected override void OnInitialize()
    {
        alreadyHit.Clear();
        UpdateDynamicSpawnPoints();

        if (skill != null && skill.RotateEffect)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (direction.x < 0 && skill.FlipSpriteY)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (!TryGetDamageableTarget(other, out GameObject target)) return;
        if (alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);
        HandleHitTarget(target);
    }

    // 충돌한 Collider에서 실제 피격 대상 GameObject 추출
    protected bool TryGetDamageableTarget(Collider2D other, out GameObject target)
    {
        target = null;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return false;

        target = damageable.gameObject;
        return target != null;
    }

    // 일반 Hit 처리 공통 함수
    protected virtual void HandleHitTarget(GameObject target)
    {
        SkillContext hitContext = CreateHitContext(target);
        skill.OnHit(hitContext);
    }

    // 추가: 현재 collider 크기/offset 기준으로 spawn point들의 localPosition 갱신
    protected virtual void UpdateDynamicSpawnPoints()
    {
        if (hitboxCollider == null) return;
        if (spawnPoints == null) return;

        Vector2 size = hitboxCollider.size;
        Vector2 offset = hitboxCollider.offset;

        if (spawnPoints.centerPoint != null)
        {
            spawnPoints.centerPoint.localPosition = offset;
        }

        if (spawnPoints.leftPoint != null)
        {
            spawnPoints.leftPoint.localPosition = offset + new Vector2(-size.x * 0.5f, 0f);
        }

        if (spawnPoints.rightPoint != null)
        {
            spawnPoints.rightPoint.localPosition = offset + new Vector2(size.x * 0.5f, 0f);
        }

        if (spawnPoints.groundPoint != null)
        {
            spawnPoints.groundPoint.localPosition = offset + new Vector2(0f, -size.y * 0.5f);
        }
    }
}
