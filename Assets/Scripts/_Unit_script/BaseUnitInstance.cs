public abstract class BaseUnitInstance : IUnitInstance
{
    protected float currentHealth;
    public float CurrentHealth => currentHealth;
    public abstract float MaxHealth { get; }

    public bool IsKnockbackActive { get; set; } = false;

    protected float baseMoveSpeed;
    public float selfSpeedMultiplier { get; set; } = 1f;
    public float externalSpeedMultiplier { get; set; } = 1f;

    protected float baseJumpPower;
    public float jumpPowerMultiplier { get; set; } = 1f;

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public bool IsDead() => currentHealth <= 0;

    public abstract bool IsKnockbackImmune();
    public abstract float GetKnockbackResistance();

    public float GetCurrentMoveSpeed() => baseMoveSpeed * selfSpeedMultiplier * externalSpeedMultiplier;
    public float GetCurrentJumpPower() => baseJumpPower * jumpPowerMultiplier;
}