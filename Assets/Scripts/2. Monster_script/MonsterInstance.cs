using System.Collections.Generic;
public class MonsterInstance : BaseUnitInstance
{
    public MonsterData data { get; private set; }

    public override float MaxHealth => data.maxHealth;
    public List<MonsterSkillList> skillList => data.skillList;

    public List<SkillInstance> skillInstances { get; private set; } = new();

    public MonsterInstance(MonsterData data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
        baseMoveSpeed = data.moveSpeed;
        baseJumpPower = data.jumpPower;

        // foreach (var trigger in data.skillList)
        // {
        //     if (trigger.skillData is MeleeSkillData melee)
        //         skillInstances.Add(new MeleeSkillInstance(melee));
        //     else if (trigger.skillData is ProjectileSkillData proj)
        //         skillInstances.Add(new ProjectileSkillInstance(proj));
        //     else
        //         skillInstances.Add(null); // fallback
        // }
    }

    public void InitializeBehavior(MonsterDecisionMaker decisionMaker)
    {
        //몬스터가 보유한 행동 목록 decision에 부여해주기, 우선순위 높은 행동 먼저 작성
        decisionMaker.AddAction(new TraceAction());
        decisionMaker.AddAction(new WanderAction());
    }

    public override bool IsKnockbackImmune() => data.knockbackImmune;
    public override float GetKnockbackResistance() => data.knockbackResistance;
}
