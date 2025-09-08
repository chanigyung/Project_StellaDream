using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : UnitController
{
    private MonsterHealthUI healthUI;
    private MonsterDeathHandler deathHandler;
    private MonsterAnimator animator;

    private MonsterContext context;
    public MonsterContext Context => context;

    public override void Initialize(IUnitInstance instance)
    {
        base.Initialize(instance);

        healthUI = GetComponent<MonsterHealthUI>();
        deathHandler = GetComponent<MonsterDeathHandler>();
        animator = GetComponent<MonsterAnimator>();

        //context 생성 및 오브젝트 동기화, 컴포넌트 캐싱
        context = new MonsterContext();
        context.selfTransform = transform;
        context.movement = GetComponent<MonsterMovement>();
        context.movement.Initialize(context);
        context.animator = GetComponent<MonsterAnimator>();
        context.instance = instance as MonsterInstance;

        //의사결정 트리와 추적 로직에 context연결
        GetComponent<MonsterDecisionMaker>()?.Initialize(context);
        GetComponentInChildren<MonsterTraceHandler>()?.Initialize(context);

        (instance as MonsterInstance)?.InitializeBehavior(GetComponent<MonsterDecisionMaker>());
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        healthUI?.SetHealth(instance.CurrentHealth);

        GetComponentInChildren<MonsterTraceHandler>()?.NotifyDamaged();
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
