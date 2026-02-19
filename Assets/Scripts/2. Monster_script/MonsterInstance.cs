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
    }

    public override bool IsKnockbackImmune() => data.knockbackImmune;
    public override float GetKnockbackResistance() => data.knockbackResistance;
}
