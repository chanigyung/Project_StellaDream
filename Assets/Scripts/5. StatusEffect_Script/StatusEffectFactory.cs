using UnityEngine;

public static class StatusEffectFactory
{
    //상태이상 인스턴스화용 클래스
    public static StatusEffect CreateEffectInstance(StatusEffectInfo info,
        GameObject target, GameObject attacker, StatusEffectManager manager)
    {
        switch (info)
        {
            case SlowEffectInfo s:
                return new SlowEffect(target, manager, attacker, s.slowRate, s.duration);
                
            case PowerKnockbackEffectInfo pk:
                return new PowerKnockbackEffect(target, manager, attacker, pk.power, pk.duration);

            case RootEffectInfo root:
                return new RootEffect(target, manager, attacker, root.duration);

            case StunEffectInfo stun:
                return new StunEffect(target, manager, attacker, stun.duration);

            case BleedEffectInfo bleed:
                return new BleedEffect(target, manager, attacker, bleed.duration, bleed.damagePerTick, bleed.tickInterval);

            case IgniteEffectInfo burn:
                return new IgniteEffect(target, manager, attacker, burn.duration, burn.damagePerTick, burn.tickInterval);

            case PoisonEffectInfo poison:
                return new PoisonEffect(target, manager, attacker,
                    poison.duration, poison.damagePerTick, poison.tickInterval, poison.slowRate);

            // 추후 다른 상태이상도 여기에 추가
            default:
                Debug.LogWarning($"상태이상 {info.type} 는 아직 구현되지 않음");
                return null;
        }
    }
}