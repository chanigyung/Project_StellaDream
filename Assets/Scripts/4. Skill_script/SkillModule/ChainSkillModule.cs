using UnityEngine;

public class ChainSkillModule : SkillModuleBase
{
    private readonly ChainSkillModuleData data;

    public ChainSkillModule(ChainSkillModuleData data)
    {
        this.data = data;
    }

    public override void OnHit(SkillContext context)
    {
        if (data == null) return;
        if (data.triggerHook != ChainTriggerHook.Hit) return;

        TriggerChainSkill(context);
    }

    public override void OnExpire(SkillContext context)
    {
        if (data == null) return;
        if (data.triggerHook != ChainTriggerHook.Expire) return;

        TriggerChainSkill(context);
    }

    // 연계 스킬 실행
    private void TriggerChainSkill(SkillContext context)
    {
        if (data.reactionSkillData == null) return;
        if (context.attacker == null) return;

        SkillExecutor executor = context.attacker.GetComponent<SkillExecutor>();
        if (executor == null) return;

        SkillInstance reactionSkillInstance = data.reactionSkillData.CreateInstance();
        if (reactionSkillInstance == null) return;

        SkillContext reactionContext = CreateReactionContext(context, reactionSkillInstance);
        executor.UseSkill(reactionContext);
    }

    // sourceObject 기준으로 연계 스킬용 Context 생성
    private SkillContext CreateReactionContext(SkillContext context, SkillInstance reactionSkillInstance)
    {
        SkillContext reactionContext = context.Clone();

        reactionContext.skillInstance = reactionSkillInstance;
        reactionContext.contextOwner = null;
        reactionContext.sourceObject = context.sourceObject;
        reactionContext.targetObject = context.targetObject;
        reactionContext.attacker = context.attacker;
        reactionContext.position = context.position;
        reactionContext.rotation = context.rotation;
        reactionContext.direction = GetHorizontalDirection(context.direction);
        reactionContext.hasDirection = true;

        return reactionContext;
    }

    // 기존 direction에서 좌/우 방향만 추출
    private Vector2 GetHorizontalDirection(Vector2 direction)
    {
        if (direction.x < 0f)
            return Vector2.left;

        return Vector2.right;
    }
}