public interface IUnitInstance
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    float selfSpeedMultiplier { get; set; }
    float externalSpeedMultiplier { get; set; }
    float jumpPowerMultiplier { get; set; }
    
    bool IsKnockbackActive { get; set; }

    void TakeDamage(float damage);
    bool IsDead();

    bool IsKnockbackImmune();
    float GetKnockbackResistance();

    float GetCurrentMoveSpeed();
    float GetCurrentJumpPower();

    // 선택 사항: 향후 상태이상 시스템 대응
    // bool IsStunned { get; }
    // void ApplyStatusEffect(StatusEffect effect);
}