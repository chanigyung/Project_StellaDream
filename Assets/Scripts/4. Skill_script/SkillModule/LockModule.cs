using UnityEngine;

public class LockModule : SkillModuleBase
{
    private readonly LockModuleData data;

    public LockModule(LockModuleData data)
    {
        this.data = data;
    }

    public override void OnDelay(SkillContext context)
    {
        ApplyRules(SkillLockHook.Delay, context);
    }

    public override void OnExecute(SkillContext context)
    {
        ApplyRules(SkillLockHook.Execute, context);
    }

    public override void OnPostDelay(SkillContext context)
    {
        ApplyRules(SkillLockHook.PostDelay, context);
    }

    public override void OnObjectSpawned(SkillContext context)
    {
        ApplyRules(SkillLockHook.ObjectSpawned, context);
    }

    public override void OnHit(SkillContext context)
    {
        ApplyRules(SkillLockHook.Hit, context);
    }

    public override void OnTick(SkillContext context)
    {
        ApplyRules(SkillLockHook.Tick, context);
    }

    public override void OnExpire(SkillContext context)
    {
        ApplyRules(SkillLockHook.Expire, context);
    }

    private void ApplyRules(SkillLockHook hook, SkillContext context)
    {
        if (data == null || data.ruleList == null || context.skillInstance == null)
            return;

        for (int i = 0; i < data.ruleList.Count; i++)
        {
            LockRule rule = data.ruleList[i];
            if (rule == null) continue;
            if (rule.hook != hook) continue;

            if (rule.addLock)
                context.skillInstance.AddLock(rule.reason);
            else
                context.skillInstance.RemoveLock(rule.reason);
        }
    }
}