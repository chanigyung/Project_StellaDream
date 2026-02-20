using System.Collections.Generic;
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

        //몬스터 애니메이션 설정(데이터 기반)
        var animatorComponent = GetComponentInChildren<Animator>();
        if (animatorComponent != null && context.instance?.data?.animatorController != null)
        {
            animatorComponent.runtimeAnimatorController = context.instance.data.animatorController;
        }

        //의사결정 트리와 추적 로직에 context연결
        var decisionMaker = GetComponent<MonsterDecisionMaker>();
        decisionMaker?.Initialize(context);
        context.skillAI = GetComponent<MonsterSkillAI>();
        context.skillAI?.Initialize(context);

        GetComponentInChildren<MonsterTraceHandler>()?.Initialize(context);

        BuildActions(decisionMaker);

        deathHandler?.InitFromData(context.instance.data);
    }

    // MonsterData.actionTypes 기반으로 액션 조립 후 DecisionMaker에 주입
    private void BuildActions(MonsterDecisionMaker decisionMaker)
    {
        if (decisionMaker == null || context == null || context.instance == null || context.instance.data == null)
            return;

        List<IMonsterAction> actionList = new();

        // MonsterData에 actionTypes가 비어있으면 안전 기본값 적용
        if (context.instance.data.actionTypes == null || context.instance.data.actionTypes.Count == 0)
        {
            actionList.Add(new TraceAction());
            actionList.Add(new WanderAction());
            decisionMaker.SetActions(actionList);
            return;
        }

        foreach (var type in context.instance.data.actionTypes)
        {
            IMonsterAction action = CreateAction(type);
            if (action != null)
                actionList.Add(action);
        }

        decisionMaker.SetActions(actionList);
    }

    // ActionType -> IMonsterAction 생성
    private IMonsterAction CreateAction(MonsterActionType type)
    {
        return type switch
        {
            MonsterActionType.Trace => new TraceAction(),
            MonsterActionType.Wander => new WanderAction(),
            MonsterActionType.Attack => new AttackAction(), // AttackAction 단계에서 연결 예정
            _ => null,
        };
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
