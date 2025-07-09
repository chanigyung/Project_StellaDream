public class PlayerInstance : BaseUnitInstance
{
    public PlayerData data { get; private set; }

    public override float MaxHealth => data.maxHealth;

    // 확장: 추후 장비, 스킬, 경험치, 상태이상 등
    // public WeaponManager weaponManager;
    // public int level;
    // public float experience;

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