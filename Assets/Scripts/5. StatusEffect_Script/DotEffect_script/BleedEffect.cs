using UnityEngine;

public class BleedEffect : DotDamageEffect
{
    public BleedEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float duration, float tickDamage, float tickInterval)
        : base(target, manager, attacker, duration, tickDamage, tickInterval)
    {
        this.effectType = StatusEffectType.Bleed;
        this.icon = StatusEffectIconLibrary.Instance.bleedSprite;
    }

    public override void Start()
    {
        // 필요시 아이콘 출력, 로그 등
        Debug.Log($"{target.name}에게 출혈 효과 적용됨");
    }

    public override void Expire()
    {
        base.Expire();
        Debug.Log($"{target.name}의 출혈 효과 종료됨");
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        // 출혈은 항상 새로운 인스턴스를 허용함 (중복 허용)
        return false;
    }
}
