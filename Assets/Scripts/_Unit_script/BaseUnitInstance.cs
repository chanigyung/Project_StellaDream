public abstract class BaseUnitInstance : IUnitInstance
{
    protected float currentHealth;
    public float CurrentHealth => currentHealth;
    public abstract float MaxHealth { get; }

    public bool IsKnockbackActive { get; set; } = false;
    private float knockbackRemainingTime = 0f;

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

    // 넉백 시작
    public void StartKnockback(float duration)
    {
        if (duration <= 0f)
        {
            EndKnockback();
            return;
        }

        IsKnockbackActive = true;
        knockbackRemainingTime = duration;
    }

    // 넉백 시간 갱신
    public bool TickKnockback(float deltaTime)
    {
        if (!IsKnockbackActive)
            return false;

        knockbackRemainingTime -= deltaTime;
        if (knockbackRemainingTime > 0f)
            return false;

        EndKnockback();
        return true;
    }

    // 넉백 종료
    public void EndKnockback()
    {
        IsKnockbackActive = false;
        knockbackRemainingTime = 0f;
    }
}