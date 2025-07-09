using UnityEngine;

public abstract class DotDamageEffect : StatusEffect
{
    protected float tickDamage;
    protected float tickInterval;
    private float tickTimer = 0f;

    public DotDamageEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float duration, float tickDamage, float tickInterval)
        : base(target, manager, attacker)
    {
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.duration = duration;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        tickTimer += deltaTime;

        if (tickTimer >= tickInterval)
        {
            ApplyTickDamage();
            tickTimer = 0f;
        }
    }

    protected virtual void ApplyTickDamage()
    {
        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(tickDamage);
            Debug.Log($"{target.name}에게 {tickDamage} 틱 데미지 적용");
        }
    }

    // 중복 적용 여부는 각 하위 클래스에서 결정
    public override abstract bool TryReplace(StatusEffect newEffect);
}