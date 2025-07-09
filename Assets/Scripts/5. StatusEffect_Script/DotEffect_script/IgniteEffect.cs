using UnityEngine;

public class IgniteEffect : DotDamageEffect
{
    private int stackCount = 1;
    private float tickBaseDamage;

    public IgniteEffect(GameObject target, StatusEffectManager manager, GameObject attacker,
                      float duration, float baseDamage, float tickInterval)
        : base(target, manager, attacker, duration, baseDamage, tickInterval)
    {
        this.effectType = StatusEffectType.Ignite;
        this.tickBaseDamage = baseDamage;
        this.icon = StatusEffectIconLibrary.Instance.igniteSprite;
    }

    public override void Start()
    {
        Debug.Log($"{target.name}에게 발화 효과 적용됨 (스택: {stackCount})");
    }
    
    public override int GetStackCount() => stackCount;

    protected override void ApplyTickDamage()
    {
        float multiplier = 1f + 0.1f * (stackCount - 1);
        float totalDamage = tickBaseDamage * multiplier * stackCount;

        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(totalDamage);
            // Debug.Log($"{target.name}에게 발화 틱딜 {totalDamage} (스택: {stackCount}, 배율: {multiplier:F2})");
        }
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        if (newEffect is IgniteEffect newIgnite)
        {
            stackCount += 1;

            duration = Mathf.Max(duration - elapsedTime, newIgnite.duration); // 남은 시간과 새 지속시간 중 더 긴 걸 유지
            elapsedTime = 0f;

            Debug.Log($"{target.name}의 발화 스택 증가 → {stackCount}, 지속시간 갱신 : {duration}초");
            return false; // 기존 인스턴스를 유지 (새 인스턴스는 버림)
        }

        return false;
    }
}
