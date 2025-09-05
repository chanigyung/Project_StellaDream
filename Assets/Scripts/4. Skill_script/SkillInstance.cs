using System.Collections.Generic;

public abstract class SkillInstance
{
    public SkillData baseData;

    public float cooldown;
    public List<StatusEffectInfo> statusEffects = new();
    public float CastDelay => baseData.castDelay; //스킬 선딜레이
    public float CastPostDelay => baseData.castPostDelay; //스킬 후딜레이

    public bool RotateEffect => baseData.rotateSkill;
    public bool FlipSpriteY => baseData.flipSpriteY;
    
    protected SkillInstance(SkillData data)
    {
        baseData = data;
        cooldown = data.cooldown;
    }

    // 강화 반영
    public abstract void ApplyUpgrade(WeaponUpgradeInfo upgrade);

    protected static StatusEffectInfo CopyEffect(StatusEffectInfo original) //상태이상 정보 복사해서 인스턴스화시키는 함수
    {
        switch (original)
        {
            case PowerKnockbackEffectInfo pk:
                return new PowerKnockbackEffectInfo
                {
                    duration = pk.duration,
                    power = pk.power
                };

            case SlowEffectInfo s:
                return new SlowEffectInfo
                {
                    duration = s.duration,
                    slowRate = s.slowRate
                };

            case RootEffectInfo r:
                return new RootEffectInfo
                {
                    duration = r.duration
                };

            case StunEffectInfo b:
                return new StunEffectInfo
                {
                    duration = b.duration
                };    

            case BleedEffectInfo bleed:
                return new BleedEffectInfo
                {
                    duration = bleed.duration,
                    damagePerTick = bleed.damagePerTick,
                    tickInterval = bleed.tickInterval
                };

            case IgniteEffectInfo ignite:
                return new IgniteEffectInfo
                {
                    duration = ignite.duration,
                    damagePerTick = ignite.damagePerTick,
                    tickInterval = ignite.tickInterval
                };

            case PoisonEffectInfo poison:
                return new PoisonEffectInfo
                {
                    duration = poison.duration,
                    damagePerTick = poison.damagePerTick,
                    tickInterval = poison.tickInterval,
                    slowRate = poison.slowRate
                };

            default:
                UnityEngine.Debug.LogWarning("정의되지 않은 상태이상 타입입니다.");
                return null;
        }
    }

    protected void ApplyStatusEffectUpgrade(int masteryLevel) //강화 적용시 함께 적용될 상태이상 강화 로직
    {
        if (masteryLevel <= 0) return;

        foreach (var effect in statusEffects)
        {
            switch (effect)
            {
                case PowerKnockbackEffectInfo pk:
                    pk.power += 1f * masteryLevel;
                    break;

                case SlowEffectInfo slow:
                    slow.slowRate += 0.05f * masteryLevel;
                    break;

                case RootEffectInfo rooted:
                    rooted.duration += 0.5f * masteryLevel;
                    break;

                case StunEffectInfo stun:
                    stun.duration += 0.5f * masteryLevel;
                    break;

                case BleedEffectInfo bleed:
                    bleed.damagePerTick += 1f * masteryLevel;
                    break;

                case IgniteEffectInfo ignite:
                    ignite.duration += 0.5f * masteryLevel;
                    break;

                case PoisonEffectInfo poison:
                    poison.duration += 0.5f * masteryLevel;
                    break;
            }
        }
    }
}