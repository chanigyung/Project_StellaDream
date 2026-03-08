using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Projectile : SkillObjectBase
{
    protected float speed;

    public int HitCount { get; protected set; }

    private readonly HashSet<GameObject> alreadyHit = new();

    public virtual void Initialize(SkillContext context, SkillInstance skill, float speed, float lifetime)
    {
        this.speed = speed;

        base.Initialize(context, skill, lifetime);
    }

    protected override void OnInitialize()
    {
        alreadyHit.Clear();
        HitCount = 0;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (direction.x < 0 && skill != null && skill.FlipSpriteY)
        {
            Vector3 scale = transform.localScale;
            scale.y *= -1;
            transform.localScale = scale;
        }
    }

    private void Update()
    {
        if (!initialized) return;

        Move();
    }

    protected virtual void Move()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;

        GameObject target = damageable.gameObject;

        if (!CanHitTarget(target)) return;

        RegisterHitTarget(target);
        HandleHit(target);
    }

    // 이미 공격한 대상 판정
    protected virtual bool CanHitTarget(GameObject target)
    {
        return !alreadyHit.Contains(target);
    }

    // 공격한 대상 alreadyHit리스트에 넣어주기
    protected virtual void RegisterHitTarget(GameObject target)
    {
        alreadyHit.Add(target);
    }

    //투사체 충돌시 처리
    protected virtual void HandleHit(GameObject target)
    {
        if (skill != null)
        {
            SkillContext hitContext = CreateHitContext(target);
            skill.OnHit(hitContext);
        }

        HitCount++;

        StopLifetime();
        ExpireNowAndDestroy();
    }

    // 투사체 전용 피격 컨텍스트 생성
    protected override SkillContext CreateHitContext(GameObject targetObject)
    {
        SkillContext hitContext = base.CreateHitContext(targetObject);

        hitContext.position = transform.position;
        hitContext.rotation = transform.rotation;
        hitContext.direction = direction;
        hitContext.hasDirection = true;

        return hitContext;
    }

    // 투사체 전용 만료 컨텍스트 생성
    protected override SkillContext CreateExpireContext()
    {
        SkillContext expireContext = base.CreateExpireContext();

        expireContext.position = transform.position;
        expireContext.rotation = transform.rotation;
        expireContext.direction = direction;
        expireContext.hasDirection = context.hasDirection;

        return expireContext;
    }
}
