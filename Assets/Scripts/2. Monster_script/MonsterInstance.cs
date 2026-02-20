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

        skillInstances.Clear();

        if (data.skillList == null)
            return;

        foreach (var entry in data.skillList)
        {
            if (entry == null || entry.skillData == null)
                continue;

            SkillInstance instance = entry.skillData.CreateInstance();

            if (instance != null)
                skillInstances.Add(instance);
        }
    }

    public override bool IsKnockbackImmune() => data.knockbackImmune;
    public override float GetKnockbackResistance() => data.knockbackResistance;
}
