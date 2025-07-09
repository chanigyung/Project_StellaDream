using UnityEngine;

public class PoisonEffect : DotDamageEffect
{
    private int stackCount = 1;
    private float baseTickDamage;
    private float baseSlowRate;

    public PoisonEffect(GameObject target, StatusEffectManager manager, GameObject attacker,
                        float duration, float tickDamage, float tickInterval, float slowRate)
        : base(target, manager, attacker, duration, tickDamage, tickInterval)
    {
        this.effectType = StatusEffectType.Poison;
        this.baseTickDamage = tickDamage;
        this.baseSlowRate = slowRate;
        this.icon = StatusEffectIconLibrary.Instance.poisonSprite;
    }

    public override void Start()
    {
        ApplyPoisonSlow();
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        if (newEffect is PoisonEffect newPoison)
        {
            stackCount += 1;

            duration = Mathf.Max(duration - elapsedTime, newPoison.duration);
            elapsedTime = 0f;

            ApplyPoisonSlow();
            Debug.Log($"{target.name}의 중독 스택 증가 → {stackCount}, 슬로우 비율 적용");

            return false; // 기존 인스턴스 유지
        }

        return false;
    }

    public override void Expire()
    {
        // 중독 만료 → 슬로우 효과 제거용 슬로우 0 적용
        base.Expire();
    }

    public override int GetStackCount() => stackCount;

    protected override void ApplyTickDamage()
    {
        float totalDamage = baseTickDamage * stackCount;

        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(totalDamage);
            // Debug.Log($"{target.name}에게 중독 틱딜 {totalDamage} (스택: {stackCount})");
        }
    }

    private void ApplyPoisonSlow()
    {
        float slowRate = 1f - Mathf.Pow(1f - baseSlowRate, stackCount);
        ApplySlowEffect(slowRate);
    }

    private void ApplySlowEffect(float slowRate)
    {
        // 기존 SlowEffect 재사용
        var slowInfo = new SlowEffectInfo
        {
            type = StatusEffectType.Slow,
            duration = this.duration,
            slowRate = slowRate
        };

        StatusEffect slow = StatusEffectFactory.CreateEffectInstance(slowInfo, target, attacker, manager);
        manager.ApplyEffect(slow);
    }
}
