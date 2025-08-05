using System.Collections.Generic;

public class PlayerInstance : BaseUnitInstance
{
    public PlayerData data { get; private set; }

    public override float MaxHealth => data.maxHealth;

    // 추후 버프 만들면 구현
    // public List<BuffInstance> activeBuffs = new();

    public PlayerInstance(PlayerData data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
        baseMoveSpeed = data.moveSpeed;
        baseJumpPower = data.jumpPower;
    }

    public override bool IsKnockbackImmune() => data.knockbackImmune;
    public override float GetKnockbackResistance() => data.knockbackResistance;
}