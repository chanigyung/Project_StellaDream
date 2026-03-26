using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : UnitController
{
    private MonsterDecisionMaker decisionMaker;
    private MonsterHealthUI healthUI;
    private MonsterDeathHandler deathHandler;
    private MonsterAnimator animator;

    private MonsterContext context;
    public MonsterContext Context => context;

    private UnitCensor censor;

    public override void Initialize(IUnitInstance instance)
    {
        base.Initialize(instance);

        healthUI = GetComponent<MonsterHealthUI>();
        deathHandler = GetComponent<MonsterDeathHandler>();
        animator = GetComponent<MonsterAnimator>();
        UnitController unit = GetComponent<UnitController>();

        //context 생성 및 오브젝트 동기화, 컴포넌트 캐싱
        context = new MonsterContext();
        context.selfTransform = transform;
        context.instance = instance as MonsterInstance;
        context.unitMovement = GetComponent<UnitMovement>();
        context.movement = GetComponent<MonsterMovement>();
        context.animator = GetComponent<MonsterAnimator>();
        context.selfGroundPoint = unit != null ? unit.GroundPoint : transform;

        context.movement.Initialize(context);
        context.unitMovement?.Initialize(context);

        //censor 생성과 캐싱
        censor = GetComponentInChildren<UnitCensor>();

        //몬스터 애니메이션 설정(데이터 기반)
        var animatorComponent = GetComponentInChildren<Animator>();
        if (animatorComponent != null && context.monsterInstance?.data?.animatorController != null)
        {
            animatorComponent.runtimeAnimatorController = context.monsterInstance.data.animatorController;
        }

        //의사결정 트리와 추적 로직에 context연결
        decisionMaker = GetComponent<MonsterDecisionMaker>();
        decisionMaker?.Initialize(context);

        context.skillAI = GetComponent<MonsterSkillAI>();
        context.skillAI?.Initialize(context);
        censor?.Initialize(context);

        var traceHandler = GetComponentInChildren<MonsterTraceHandler>();
        context.traceHandler = traceHandler;
        traceHandler?.Initialize(context);

        BuildActions(decisionMaker);

        deathHandler?.InitFromData(context.monsterInstance.data);
    }

    protected override void Update()
    {
        base.Update();

        if (context == null)
            return;

        context.UpdateContext();

        if (context.skillAI != null && context.skillAI.TryUseSkill())
            return;

        decisionMaker?.DecideAndExecute();
    }

    // MonsterData.actionTypes 기반으로 액션 조립 후 DecisionMaker에 주입
    private void BuildActions(MonsterDecisionMaker decisionMaker)
    {
        if (decisionMaker == null || context == null || context.monsterInstance == null || context.monsterInstance.data == null)
            return;

        List<IMonsterAction> actionList = new();

        // MonsterData에 actionTypes가 비어있으면 안전 기본값 적용
        if (context.monsterInstance.data.actionTypes == null || context.monsterInstance.data.actionTypes.Count == 0)
        {
            actionList.Add(new TraceAction());
            actionList.Add(new WanderAction());
            decisionMaker.SetActions(actionList);
            return;
        }

        foreach (var type in context.monsterInstance.data.actionTypes)
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
            MonsterActionType.WanderSkill => new WanderSkillAction(),
            _ => null,
        };
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        healthUI?.SetHealth(instance.CurrentHealth);

        context.traceHandler?.NotifyDamaged();
    }

    protected override void HandleDeath()
    {
        deathHandler?.Die(); // 몬스터 전용 사망 처리
    }

    public override void ApplyKnockback(Vector2 force, float duration)
    {
        base.ApplyKnockback(force, duration);
        // animator?.PlayHit();
    }

    protected override void OnKnockbackStarted()
    {
        context?.movement?.ClearMove();
        animator?.PlayStunned(true);
    }

    protected override void OnKnockbackEnded()
    {
        animator?.PlayStunned(false);
    }
}
