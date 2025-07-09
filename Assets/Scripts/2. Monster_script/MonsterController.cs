using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : UnitController
{
    private MonsterHealthUI healthUI;
    private MonsterDeathHandler deathHandler;
    private MonsterAnimator animator;

    public override void Initialize(IUnitInstance instance)
    {
        base.Initialize(instance);
        healthUI = GetComponent<MonsterHealthUI>();
        deathHandler = GetComponent<MonsterDeathHandler>();
        animator = GetComponent<MonsterAnimator>();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        healthUI?.SetHealth(instance.CurrentHealth);
    }

    protected override void HandleDeath()
    {
        deathHandler?.Die(); // 몬스터 전용 사망 처리
    }

    public override void ApplyKnockback(Vector2 force)
    {
        base.ApplyKnockback(force);
        animator.PlayHit();
    }
}
