using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    protected GameObject attacker;
    protected SkillInstance skill;
    protected Vector2 direction;
    protected float speed;

    public int HitCount { get; protected set; }
    private Coroutine lifetimeRoutine; // [추가]
    private bool isHit;

    private readonly HashSet<GameObject> alreadyHit = new();
    protected bool initialized;

    public virtual void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction, float speed, float lifetime)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;
        this.speed = speed;

        HitCount = 0;
        isHit = false;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (this.direction.x < 0 && skill.FlipSpriteY)
        {
            Vector3 scale = transform.localScale;
            scale.y *= -1;
            transform.localScale = scale;
        }

        initialized = true;
        if (lifetimeRoutine != null) StopCoroutine(lifetimeRoutine);
        if (lifetime > 0f)
            lifetimeRoutine = StartCoroutine(LifetimeRoutine(lifetime));
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
        skill.OnHit(attacker, target);
        HitCount++;
        isHit = true;

        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);

        Destroy(gameObject);
    }

    // 투사체 지속시간 후 파괴용 코루틴
    private IEnumerator LifetimeRoutine(float lifetime)
    {
        if (lifetime > 0f)
            yield return new WaitForSeconds(lifetime);

        if (!isHit)
            Expire();

        Destroy(gameObject);
    }

    private void Expire()
    {
        if (skill == null) return;
        skill.OnExpire(attacker, gameObject);
    }
}
