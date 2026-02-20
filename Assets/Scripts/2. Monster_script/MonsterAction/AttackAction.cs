public class AttackAction : IMonsterAction
{
    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (!context.canAct) return false;
        if (!context.isTracing) return false;
        if (context.target == null) return false;
        if (context.skillAI == null) return false;

        // 변경: 실제 스킬 사용 로직은 SkillAI가 유지. 여기서는 "지금 시전 시작 가능한가"만 확인.
        return context.skillAI.CanCastNow();
    }

    public void Execute(MonsterContext context)
    {
        if (context == null) return;
        if (context.skillAI == null) return;

        // 변경: 공격 실행은 MonsterSkillAI에 완전 위임
        context.skillAI.TryUseSkill();
    }
}